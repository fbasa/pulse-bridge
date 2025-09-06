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
    this.svc.chat$.subscribe(list => this.messages.set(list));
    this.svc.connectionState$.subscribe(s => this.connState.set(s));
    this.connect();
  }

  ngOnDestroy(): void {
    this.svc.stop();
  }

  connect(): void {
    this.svc.start();
  }

}
