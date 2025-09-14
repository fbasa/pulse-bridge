import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { importProvidersFrom, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { OAuthModule } from 'angular-oauth2-oidc';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { AuthService } from './app/auth/auth.service';
import { environment } from './environments/environment';


function initAuthFactory(auth: AuthService) { return () => auth.initAuth(); }


bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    importProvidersFrom(
      OAuthModule.forRoot({
        resourceServer: {
          allowedUrls: [environment.apis.payments, environment.apis.accounting],
          sendAccessToken: true
        }
      })
    ),
    { provide: APP_INITIALIZER, useFactory: initAuthFactory, deps: [AuthService], multi: true }
  ]
}).catch(console.error);