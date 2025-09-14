import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';


export const authConfig: AuthConfig = {
    issuer: environment.auth.issuer,
    clientId: environment.auth.clientId,
    redirectUri: environment.auth.redirectUri,
    postLogoutRedirectUri: environment.auth.postLogoutRedirectUri,
    responseType: 'code', // Authorization Code + PKCE
    scope: environment.auth.scopes, // 'openid profile payments.read accounting.read'
    requireHttps: true,
    showDebugInformation: !environment.production
};