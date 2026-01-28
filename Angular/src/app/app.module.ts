import { NgModule, CUSTOM_ELEMENTS_SCHEMA, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { LocationStrategy } from '@angular/common';

import { OidcAuthService } from './core/services/auth/oidc-auth.service';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { GlobalErrorInterceptor } from './core/interceptors/error.interceptor';
import { AuthService } from './core/services/auth/auth.service';

export function initializeOidc(
  oidcAuthService: OidcAuthService,
  authService: AuthService
) {
  return () => {
    return oidcAuthService.init().then(initialized => {
      if (initialized) {
        return authService.loadUserPermissions().toPromise();
      }
      return true;
    }).catch(err => {
      console.error('APP_INITIALIZER error', err);
      return true;
    });
  };
}
@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    CoreModule,
    RouterModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: (http: HttpClient, locationStrategy: LocationStrategy) => {
          return new TranslateHttpLoader(
            http,
            `${window.location.protocol}//${window.location.host}${locationStrategy.getBaseHref()}assets/translations/`,
            '.json'
          );
        },
        deps: [HttpClient, LocationStrategy]
      }
    }),
  ],
  providers: [
    provideHttpClient(
      withInterceptors([AuthInterceptor, GlobalErrorInterceptor])
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeOidc,
      deps: [OidcAuthService,AuthService],
      multi: true
    },
    provideAnimationsAsync()
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule { }