import { Injectable, inject } from '@angular/core';
import { OAuthService, OAuthEvent, OAuthSuccessEvent, OAuthErrorEvent } from 'angular-oauth2-oidc';
import { authConfig } from './auth.config';
import { Router } from '@angular/router';
import { filter } from 'rxjs';


@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly oauth = inject(OAuthService);
    private readonly router = inject(Router);


    async initAuth(): Promise<void> {
        this.oauth.configure(authConfig);
        await this.oauth.loadDiscoveryDocument();


        // Complete code->token exchange on /auth/callback
        await this.oauth.tryLoginCodeFlow({
            onTokenReceived: async () => {
                // Clean up the querystring and optionally load profile
                window.history.replaceState({}, document.title, window.location.origin + '/');
                try { await this.oauth.loadUserProfile(); } catch { /* no-op */ }
            }
        });


        // Optional: automatic silent refresh when refresh tokens are allowed
        this.oauth.setupAutomaticSilentRefresh();


        // Debug auth events in dev
        this.oauth.events
            .pipe(filter((e: OAuthEvent) => e instanceof OAuthSuccessEvent || e instanceof OAuthErrorEvent))
            .subscribe(e => console.log('[OAuthEvent]', e));
    }


    login(): void {
        // Loads discovery (if not yet) and starts the OIDC flow
        this.oauth.loadDiscoveryDocumentAndLogin();
    }


    async logout(): Promise<void> {
        // Uses discovery doc's end_session_endpoint when OpenIddict is configured with SetEndSessionEndpointUris
        await this.oauth.logOut();
    }


    isLoggedIn(): boolean { return this.oauth.hasValidAccessToken(); }
    get accessToken(): string | null { return this.oauth.getAccessToken() || null; }
}