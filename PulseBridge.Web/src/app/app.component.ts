import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from './auth/auth.service';
import { PaymentsApiService } from './services/payments-api.service';
import { AccountingApiService } from './services/accounting-api.service';
import { JobPayload, SignalRService } from './services/signalr.service';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NgIf, RouterLink, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']  
})
export class AppComponent implements OnInit {
  payments: any[] | null = null;
  accounting: any[] | null = null;

  private readonly signalrApi = inject(SignalRService);
  private readonly paymentsApi = inject(PaymentsApiService);
  private readonly accountingApi = inject(AccountingApiService);

  public readonly auth = inject(AuthService);
  async ngOnInit() { 

    this.signalrApi.chat$.subscribe(msg => this.messages.set(msg));
    this.signalrApi.connectionState$.subscribe(state => this.connState.set(state));
    this.connect();

    this.loadPayments(); 
    this.loadAccounting();
  }
  loadPayments() { this.paymentsApi.getAll().subscribe(r => this.payments = r); }
  loadAccounting() { this.accountingApi.getAll().subscribe(r => this.accounting = r); }

  logout() { this.auth.logout(); }

  messages = signal<JobPayload[]>([]); // placeholder to keep type help in IDE
  connState = signal<'disconnected' | 'connecting' | 'connected' | 'reconnecting'>('disconnected');

  ngOnDestroy(): void {
    this.signalrApi.stop();
  }

  connect(): void {
    this.signalrApi.start();
  }

}