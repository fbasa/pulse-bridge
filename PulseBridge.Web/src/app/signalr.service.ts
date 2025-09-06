import { Injectable, NgZone, inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from '../environments/environment';

export interface JobPayload {
  user: string;
  message: string;
  at: Date;
}

type ConnState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private zone = inject(NgZone);

  private connection?: HubConnection;
  private state$ = new BehaviorSubject<ConnState>('disconnected');
  private messages$ = new BehaviorSubject<JobPayload[]>([]);

  readonly connectionState$: Observable<ConnState> = this.state$.asObservable();
  readonly chat$: Observable<JobPayload[]> = this.messages$.asObservable();

  start(): void {
    if (this.connection && this.connection.state !== 'Disconnected') return;

    this.state$.next('connecting');

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl /*, { accessTokenFactory: async () => 'jwt-here' } */)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    // Server → client
    this.connection.on('ReceiveMessage', (user: string, message: string) => {
      this.zone.run(() => {
        const next = [...this.messages$.value, { user, message, at: new Date() }];
        this.messages$.next(next);
      });
    });

    // Reconnect state updates
    this.connection.onreconnecting(() => this.zone.run(() => this.state$.next('reconnecting')));
    this.connection.onreconnected(() => this.zone.run(() => this.state$.next('connected')));
    this.connection.onclose(() => this.zone.run(() => this.state$.next('disconnected')));

    this.connection
      .start()
      .then(() => this.zone.run(() => this.state$.next('connected')))
      .catch(err => {
        console.error('SignalR start failed', err);
        this.zone.run(() => this.state$.next('disconnected'));
        // simple retry
        setTimeout(() => this.start(), 2000);
      });
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
  }

  clear(): void {
    this.messages$.next([]);
  }
}
