import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private readonly hubConnection: signalR.HubConnection;
  public messages$ = new BehaviorSubject<string[]>([]);

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7048/hubs/notification')
      .withAutomaticReconnect()
      .build();

    this.start();

    this.hubConnection.on('ReceiveMessage', (message: string) => {
      const currentMessages = this.messages$.value;
      this.messages$.next([...currentMessages, message]);
    });
  }

  private async start() {
    try {
      await this.hubConnection.start();
      console.log('SignalR Connected!');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      setTimeout(() => this.start(), 5000);
    }
  }

  async sendMessage(message: string) {
    try {
      await this.hubConnection.invoke('SendMessage', message);
    } catch (err) {
      console.error('Error sending message: ', err);
    }
  }
}