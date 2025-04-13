// src/app/core/services/signalr-service.base.ts
import { Injectable, OnDestroy, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { AuthService } from './auth.service';

export type ConnectionStatus = 'connected' | 'disconnected' | 'connecting' | 'error';

@Injectable()
export abstract class BaseSignalRService implements OnDestroy {
  protected readonly authService = inject(AuthService);

  protected hubConnection?: signalR.HubConnection;
  protected authSubscription?: Subscription;

  // Connection status tracking
  private readonly connectionStatusSubject = new BehaviorSubject<ConnectionStatus>('disconnected');
  readonly connectionStatus$ = this.connectionStatusSubject.asObservable();

  // Must be implemented by child classes
  protected abstract hubUrl: string;

  constructor() {
    this.initializeConnection();
  }

  ngOnDestroy(): void {
    this.closeConnection();
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  /**
   * Initialize the SignalR connection and set up auth status monitoring
   */
  protected initializeConnection(): void {
    // Get initial connection with current token
    this.authService.getAccessToken().subscribe(token => {
      if (token) {
        this.createConnection(token);
      } else {
        console.warn(`No auth token available for ${this.hubUrl} SignalR connection`);
      }
    });

    // Monitor auth state for changes
    this.authSubscription = this.authService.authState$.subscribe(state => {
      if (state.isAuthenticated && state.accessToken) {
        if (!this.hubConnection || this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
          this.createConnection(state.accessToken);
        }
      } else if (this.hubConnection) {
        // User logged out or token invalid
        this.closeConnection();
      }
    });
  }

  /**
   * Create a new SignalR connection with the provided token
   * @param token Auth token for SignalR connection
   */
  protected createConnection(token: string): void {
    this.updateConnectionStatus('connecting');

    // Close existing connection if it exists
    this.closeConnection();

    // Create new connection
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    // Set up connection event handlers
    this.setupConnectionEvents();

    // Set up message handlers
    this.registerEventHandlers();

    // Start the connection
    this.hubConnection.start()
      .then(() => {
        console.log(`Connected to ${this.hubUrl}`);
        this.updateConnectionStatus('connected');
        this.onConnected();
      })
      .catch(err => {
        console.error(`Error connecting to ${this.hubUrl}:`, err);
        this.updateConnectionStatus('error');
      });
  }

  /**
   * Close the current SignalR connection
   */
  protected closeConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .catch(err => console.error('Error stopping SignalR connection:', err));
      this.hubConnection = undefined;
      this.updateConnectionStatus('disconnected');
    }
  }

  /**
   * Set up connection status events
   */
  private setupConnectionEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting(() => {
      console.log(`Reconnecting to ${this.hubUrl}...`);
      this.updateConnectionStatus('connecting');
    });

    this.hubConnection.onreconnected(() => {
      console.log(`Reconnected to ${this.hubUrl}`);
      this.updateConnectionStatus('connected');
      this.onReconnected();
    });

    this.hubConnection.onclose(() => {
      console.log(`Connection to ${this.hubUrl} closed`);
      this.updateConnectionStatus('disconnected');
    });
  }

  /**
   * Update the connection status
   * @param status New connection status
   */
  protected updateConnectionStatus(status: ConnectionStatus): void {
    this.connectionStatusSubject.next(status);
  }

  /**
   * Check if the connection is active
   * @returns True if connected, false otherwise
   */
  isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Get the current connection status
   * @returns Current connection status value
   */
  getConnectionStatus(): ConnectionStatus {
    return this.connectionStatusSubject.value;
  }

  /**
   * Callback for successful connection - can be overridden by child classes
   */
  protected onConnected(): void {
    // Optional override by child classes
  }

  /**
   * Callback for successful reconnection - can be overridden by child classes
   */
  protected onReconnected(): void {
    // Optional override by child classes
  }

  /**
   * Register all event handlers - must be implemented by child classes
   */
  protected abstract registerEventHandlers(): void;
}