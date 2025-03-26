import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  public messages$ = new BehaviorSubject<string[]>([]);
  private readonly authService = inject(AuthService);

  constructor() {
    this.initConnection();
  }

  private initConnection() {
    this.authService.getAccessToken().subscribe(token => {
      if (token) {
        this.createConnection(token);
        this.start();
      } else {
        console.warn('No auth token available for SignalR connection');
      }
    });

    // Listen for auth state changes to recreate connection if needed
    this.authService.authState$.subscribe(state => {
      if (state.isAuthenticated && state.accessToken && (!this.hubConnection || this.hubConnection.state === signalR.HubConnectionState.Disconnected)) {
        this.createConnection(state.accessToken);
        this.start();
      }
    });
  }

  private createConnection(token: string) {
    // Close existing connection if open
    if (this.hubConnection) {
      this.hubConnection.off('ReceiveMessage');
      this.hubConnection.stop().catch(err => console.error('Error stopping connection:', err));
    }

    // Create new connection with auth token
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7048/hubs/notification', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    // Set up message handler
    this.hubConnection.on('ReceiveMessage', (message: string) => {
      const currentMessages = this.messages$.value;
      this.messages$.next([...currentMessages, message]);
    });
  }

  private async start() {
    if (!this.hubConnection) {
      console.error('Cannot start SignalR: No connection created');
      return;
    }

    try {
      await this.hubConnection.start();
      console.log('SignalR Connected!');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      setTimeout(() => this.start(), 5000);
    }
  }

  async sendMessage(message: string) {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.error('Cannot send message: SignalR connection not established');
      return;
    }

    try {
      await this.hubConnection.invoke('SendMessage', message);
    } catch (err) {
      console.error('Error sending message: ', err);
    }
  }
}