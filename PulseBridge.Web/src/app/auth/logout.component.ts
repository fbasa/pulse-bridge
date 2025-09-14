import { Component, OnInit, inject } from '@angular/core';
import { AuthService } from './auth.service';
@Component({ selector: 'app-logout', standalone: true, template: `<div class="p-4">Signing you out…</div>` })
export class LogoutComponent implements OnInit {
    private readonly auth = inject(AuthService);
    async ngOnInit() { await this.auth.logout(); }
}