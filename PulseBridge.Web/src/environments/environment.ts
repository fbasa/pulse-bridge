export const environment = {
    production: false,
    hubUrl: 'https://api.localtest.me/hubs/schedulerHub',
    auth: {
        issuer: 'https://idp.localtest.me/', // OpenIddict Issuer (https://idp.localtest.me/.well-known/openid-configuration)
        clientId: 'angular-spa', // MUST exist in OpenIddict applications
        redirectUri: 'http://localhost:4200/auth/callback',//["http://localhost:4200/auth/callback"]
        postLogoutRedirectUri: 'http://localhost:4200/',
        scopes: 'openid profile payments.read accounting.read',
    },
    apis: {
        payments: 'https://localhost:7024', // Payments.Api
        accounting: 'https://localhost:7095' // Accounting.Api
    }
};