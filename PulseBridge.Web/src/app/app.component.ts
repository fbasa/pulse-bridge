import { Component, inject, signal } from '@angular/core';
import { JobPayload, SignalRService } from './signalr.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent {
  
  svc = inject(SignalRService);

  messages = signal<JobPayload[]>([]); // placeholder to keep type help in IDE
  connState = signal<'disconnected' | 'connecting' | 'connected' | 'reconnecting'>('disconnected');


  ngOnInit(): void {
    this.svc.chat$.subscribe(msg => this.messages.set(msg));
    this.svc.connectionState$.subscribe(state => this.connState.set(state));
    this.connect();
  }

  ngOnDestroy(): void {
    this.svc.stop();
  }

  connect(): void {
    this.svc.start();
  }

}
