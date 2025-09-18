export const environment = {
    production: false,
    hubUrl: 'https://api.localtest.me/hubs/schedulerHub',
    auth: {
        issuer: 'https://idp.localtest.me/', // OpenIddict Issuer (https://idp.localtest.me/.well-known/openid-configuration)
        clientId: 'angular-spa', // MUST exist in OpenIddict applications
        redirectUri: 'https://ui.localtest.me/auth/callback',//["http://localhost:4200/auth/callback"]
        postLogoutRedirectUri: 'https://ui.localtest.me/', 
        responseType: 'code', // Authorization Code Flow with PKCE
        scopes: 'openid profile payments.read accounting.read offline_access' // offline_access to get refresh tokens,
    },
    apis: {
        payments: 'https://payapi.localtest.me', // Payments.Api
        accounting: 'https://acctapi.localtest.me' // Accounting.Api
    }
};