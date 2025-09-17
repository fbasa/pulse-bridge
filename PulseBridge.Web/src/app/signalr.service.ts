import { Injectable, NgZone, inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel, IRetryPolicy } from '@microsoft/signalr';
import { environment } from '../environments/environment';

const MAX_RETRIES = 5;
let exhaustedRetries = false;   // set when we hit the cap
let manualStop = false;         // set when you intentionally stop()

const reconnectPolicy: IRetryPolicy = {
  nextRetryDelayInMilliseconds: (ctx) => {
    // ctx.previousRetryCount = 0 on first retry, then 1, 2, ...
    if (ctx.previousRetryCount >= MAX_RETRIES) {
      console.log(`SignalR: exhausted max retries (${MAX_RETRIES})`);
      exhaustedRetries = true;      // <-- we'll check this in onclose
      return null;                  // stop reconnecting
    }
    // backoff schedule (tweak as you like)
    const delays = [0, 2000, 5000, 10000, 20000];
    return delays[ctx.previousRetryCount] ?? 20000;
  }
};

export interface JobPayload {
  user: string;
  message: string;
  at: Date;
}

type ConnState = HubConnectionState;

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private zone = inject(NgZone);

  private connection?: HubConnection;
  private state$ = new BehaviorSubject<ConnState>(HubConnectionState.Disconnected);
  private messages$ = new BehaviorSubject<JobPayload[]>([]);

  readonly connectionState$: Observable<ConnState> = this.state$.asObservable();
  readonly chat$: Observable<JobPayload[]> = this.messages$.asObservable();

  start(): void {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) return;

    this.state$.next(HubConnectionState.Connecting);

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl /*, { accessTokenFactory: async () => 'jwt-here' } */)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // custom retry delays
      .configureLogging(LogLevel.Information)
      .build();

    // Server -> client
    this.connection.on('ReceiveMessage', (user: string, message: string) => {
      this.zone.run(() => {
        const next = [...this.messages$.value, { user, message, at: new Date() }];
        this.messages$.next(next);
      });
    });

    // Reconnect state updates
    this.connection.onreconnecting(() => this.zone.run(() => this.state$.next(HubConnectionState.Reconnecting)));
    this.connection.onreconnected(() => this.zone.run(() => this.state$.next(HubConnectionState.Connected)));
    // this.connection.onclose(() => this.zone.run(() => {
    //   console.warn('SignalR disconnected');
    //   this.state$.next(HubConnectionState.Disconnected);
    // }));
    this.connection.onclose((err) => {
      if (manualStop) {
        console.info("SignalR: closed (manual stop)");
        this.state$.next(HubConnectionState.Disconnected);
      } else if (exhaustedRetries) {
        console.warn("SignalR: closed after max (5) reconnect attempts");
        this.state$.next(HubConnectionState.Disconnected);
        // -> update UI: "Realtime offline — click to retry"
      } else if (err) {
        console.error("SignalR: closed due to error before reconnect flow finished", err);
      } else {
        console.info("SignalR: closed gracefully by server");
      }
    });
    this.connection
      .start()
      .then(() => this.zone.run(() => this.state$.next(HubConnectionState.Connected)))
      .catch(err => {
        console.error('SignalR start failed', err);
        this.zone.run(() => this.state$.next(HubConnectionState.Disconnected));
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
