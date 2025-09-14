import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';


@Injectable({ providedIn: 'root' })
export class AccountingApiService {
    private readonly http = inject(HttpClient);
    private readonly base = environment.apis.accounting;
    getAll() { return this.http.get<any[]>(`${this.base}/api/accounting`); }
    post(payload: any) { return this.http.post(`${this.base}/api/accounting`, payload); }
}