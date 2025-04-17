// src/app/core/services/notification-signalr.service.ts
import { Injectable } from '@angular/core';
import { BaseSignalRService } from './signalr-service.base';
import { environment } from '../../../environments/environment';
import { BehaviorSubject } from 'rxjs';

export interface Notification {
    id: string;
    type: string;
    message: string;
    title: string;
    createdAt: Date;
    read: boolean;
    link?: string;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationSignalRService extends BaseSignalRService {
    protected override hubUrl = environment.signalR.notifications;

    // Messages state
    private messagesSubject = new BehaviorSubject<string[]>([]);
    public messages$ = this.messagesSubject.asObservable();

    // Notifications state
    private notificationsSubject = new BehaviorSubject<Notification[]>([]);
    public notifications$ = this.notificationsSubject.asObservable();

    // Unread count
    private unreadCountSubject = new BehaviorSubject<number>(0);
    public unreadCount$ = this.unreadCountSubject.asObservable();

    /**
     * Register SignalR event handlers for notification hub
     */
    protected override registerEventHandlers(): void {
        if (!this.hubConnection) return;

        // Simple message handler
        this.hubConnection.on('ReceiveMessage', (message: string) => {
            const currentMessages = this.messagesSubject.value;
            this.messagesSubject.next([...currentMessages, message]);
        });

        // Structured notification handler
        this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
            const currentNotifications = this.notificationsSubject.value;
            const newNotifications = [notification, ...currentNotifications];
            this.notificationsSubject.next(newNotifications);

            // Update unread count
            this.unreadCountSubject.next(
                newNotifications.filter(n => !n.read).length
            );
        });

        // Handle notification read status update
        this.hubConnection.on('NotificationRead', (notificationId: string) => {
            const currentNotifications = this.notificationsSubject.value;
            const updatedNotifications = currentNotifications.map(n =>
                n.id === notificationId ? { ...n, read: true } : n
            );

            this.notificationsSubject.next(updatedNotifications);
            this.unreadCountSubject.next(
                updatedNotifications.filter(n => !n.read).length
            );
        });

        // Handle notification clear
        this.hubConnection.on('NotificationsCleared', () => {
            this.notificationsSubject.next([]);
            this.unreadCountSubject.next(0);
        });
    }

    /**
     * Send a message through the hub
     * @param message Message to send
     */
    async sendMessage(message: string): Promise<void> {
        if (!this.isConnected()) {
            console.error('Cannot send message: SignalR connection not established');
            return;
        }

        try {
            await this.hubConnection!.invoke('SendMessage', message);
        } catch (err) {
            console.error('Error sending message: ', err);
        }
    }

    /**
     * Mark a notification as read
     * @param notificationId ID of notification to mark as read
     */
    async markAsRead(notificationId: string): Promise<void> {
        if (!this.isConnected()) return;

        try {
            await this.hubConnection!.invoke('MarkAsRead', notificationId);

            // Optimistically update local state
            const currentNotifications = this.notificationsSubject.value;
            const updatedNotifications = currentNotifications.map(n =>
                n.id === notificationId ? { ...n, read: true } : n
            );

            this.notificationsSubject.next(updatedNotifications);
            this.unreadCountSubject.next(
                updatedNotifications.filter(n => !n.read).length
            );
        } catch (err) {
            console.error('Error marking notification as read: ', err);
        }
    }

    /**
     * Mark all notifications as read
     */
    async markAllAsRead(): Promise<void> {
        if (!this.isConnected()) return;

        try {
            await this.hubConnection!.invoke('MarkAllAsRead');

            // Optimistically update local state
            const currentNotifications = this.notificationsSubject.value;
            const updatedNotifications = currentNotifications.map(n => ({ ...n, read: true }));

            this.notificationsSubject.next(updatedNotifications);
            this.unreadCountSubject.next(0);
        } catch (err) {
            console.error('Error marking all notifications as read: ', err);
        }
    }

    /**
     * Clear all notifications
     */
    async clearAllNotifications(): Promise<void> {
        if (!this.isConnected()) return;

        try {
            await this.hubConnection!.invoke('ClearNotifications');
            this.notificationsSubject.next([]);
            this.unreadCountSubject.next(0);
        } catch (err) {
            console.error('Error clearing notifications: ', err);
        }
    }
}