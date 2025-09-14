import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';


@Injectable({ providedIn: 'root' })
export class PaymentsApiService {
    private readonly http = inject(HttpClient);
    private readonly base = environment.apis.payments;
    getAll() { return this.http.get<any[]>(`${this.base}/api/payments`); }
    create(payload: any) { return this.http.post(`${this.base}/api/payments`, payload); }
}