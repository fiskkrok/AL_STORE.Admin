// src/app/core/services/signalr-factory.service.ts
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class SignalRFactoryService {
    private hubConnections = new Map<string, BehaviorSubject<{
        connection: signalR.HubConnection;
        status: 'connected' | 'disconnected' | 'connecting' | 'error';
    }>>();

    constructor(private authService: AuthService) { }

    getHubConnection(hubUrl: string): Observable<signalR.HubConnection> {
        // Create a subject for this hub if it doesn't exist
        if (!this.hubConnections.has(hubUrl)) {
            const connectionSubject = new BehaviorSubject<{
                connection: signalR.HubConnection;
                status: 'connected' | 'disconnected' | 'connecting' | 'error';
            }>({
                connection: this.createHubConnection(hubUrl),
                status: 'disconnected'
            });

            this.hubConnections.set(hubUrl, connectionSubject);
            this.initializeConnection(hubUrl);
        }

        // Return the connection
        return this.hubConnections.get(hubUrl)!.pipe(
            map(info => {
                if (info.status === 'error') {
                    throw new Error(`Failed to connect to ${hubUrl}`);
                }
                return info.connection;
            })
        );
    }

    getConnectionStatus(hubUrl: string): Observable<string> {
        if (!this.hubConnections.has(hubUrl)) {
            return of('not_initialized');
        }

        return this.hubConnections.get(hubUrl)!.pipe(
            map(info => info.status)
        );
    }

    private createHubConnection(hubUrl: string): signalR.HubConnection {
        return new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => '' // Will be set later
            })
            .withAutomaticReconnect()
            .build();
    }

    private initializeConnection(hubUrl: string): void {
        const subject = this.hubConnections.get(hubUrl)!;

        // Update status to connecting
        subject.next({
            ...subject.value,
            status: 'connecting'
        });

        // Get auth token and connect
        this.authService.getAccessToken().subscribe({
            next: token => {
                if (!token) {
                    subject.next({
                        ...subject.value,
                        status: 'error'
                    });
                    return;
                }

                // Set the token factory
                const connection = subject.value.connection;
                (connection as any).httpConnectionOptions = {
                    ...(connection as any).httpConnectionOptions,
                    accessTokenFactory: () => token
                };

                // Start the connection
                connection.start()
                    .then(() => {
                        console.log(`Connected to ${hubUrl}`);
                        subject.next({
                            connection,
                            status: 'connected'
                        });
                    })
                    .catch(err => {
                        console.error(`Error connecting to ${hubUrl}:`, err);
                        subject.next({
                            ...subject.value,
                            status: 'error'
                        });
                    });
            },
            error: err => {
                console.error(`Error getting auth token for ${hubUrl}:`, err);
                subject.next({
                    ...subject.value,
                    status: 'error'
                });
            }
        });
    }
}