import { Routes } from '@angular/router';
import { CallbackComponent } from './auth/callback.component';
import { LogoutComponent } from './auth/logout.component';
import { authGuard } from './auth/auth.guard';
import { Component } from '@angular/core';


@Component({ standalone: true, template: `<div class="p-4"><h2>Protected Area</h2></div>` })
export class ProtectedComponent { }


export const routes: Routes = [
    { path: '', pathMatch: 'full', redirectTo: 'home' },
    { path: 'home', loadComponent: () => import('./app.component').then(m => m.AppComponent) },
    { path: 'auth/callback', component: CallbackComponent },
    { path: 'logout', component: LogoutComponent },
    { path: 'protected', canActivate: [authGuard], component: ProtectedComponent },
    { path: '**', redirectTo: 'home' }
];