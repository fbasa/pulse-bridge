import { Injectable, NgZone, inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { AuthService } from '../auth/auth.service';

export interface JobPayload {
  user: string;
  message: string;
  at: Date;
}

const MAX_START_TRIES = 5;

function delay(ms: number) {
  return new Promise(res => setTimeout(res, ms));
}

// Try starting the connection with MAX_START_TRIES and backoff
async function startWithRetry(conn: HubConnection) : Promise<boolean> {
  for (let attempt = 1; attempt <= MAX_START_TRIES; attempt++) {
    try {
      await conn.start();
      return true; // connected!
    } catch (err) {
      console.warn(`[SignalR] start failed (#${attempt})`, err);
      if (attempt === MAX_START_TRIES) break;
      // backoff (tweak as you like)
      await delay([500, 2000, 5000, 10000, 20000][attempt - 1] ?? 20000);
    }
  }
  return false; // give up -> mark UI offline
}


type ConnState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private zone = inject(NgZone);

  private connection?: HubConnection;
  private state$ = new BehaviorSubject<ConnState>('disconnected');
  private messages$ = new BehaviorSubject<JobPayload[]>([]);
  private auth = inject(AuthService);
  readonly connectionState$: Observable<ConnState> = this.state$.asObservable();
  readonly chat$: Observable<JobPayload[]> = this.messages$.asObservable();

  async start(): Promise<void> {
    if (this.connection && this.connection.state !== 'Disconnected') return;

    this.state$.next('connecting');

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl, 
        { 
          accessTokenFactory: async () => this.auth.accessToken || ''
        })
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

    await startWithRetry(this.connection)
      .then((result) => {
        if (result) {
          console.log('SignalR connected');
          this.zone.run(() => this.state$.next('connected'));
        }else {
          console.error('SignalR failed to connect');
          this.zone.run(() => this.state$.next('disconnected'));
        }
      })
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
