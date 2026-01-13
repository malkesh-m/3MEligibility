import { Injectable } from '@angular/core';
import Keycloak, { KeycloakInstance } from 'keycloak-js';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class KeycloakService {
  refreshTokenIfNeeded(arg0: number) {
    throw new Error('Method not implemented.');
  }  
  private keycloak: KeycloakInstance | undefined;

  loginAutomatically = true;

 async init(): Promise<boolean> {
this.keycloak = new Keycloak({
  url: 'https://auth.3m-technology.com',
  realm: '3M',
  clientId: '3MEligibility-Client'
});

  try {
    return await this.keycloak.init({
      onLoad: 'check-sso',
      pkceMethod: 'S256',
      checkLoginIframe: false,
       enableLogging: true,
      redirectUri: environment.identitySettings.loginCallBackPath,
       responseMode: 'query'
      
    });
  } catch (err:any) {
    console.error('Keycloak init failed', err);
    return false;
  }
}

getToken(): string {
  if (!this.keycloak || !this.keycloak.authenticated) {
    return '';
  }
  return this.keycloak.token ?? '';
}

  login(): void {
    this.keycloak?.login({ redirectUri: environment.identitySettings.loginCallBackPath });
  }

  logout(): void {
    this.keycloak?.logout({ redirectUri: environment.identitySettings.logoutCallBackPath });
  }

  isLoggedIn(): boolean {
    console.log('Keycloak authenticated status:', this.keycloak?.authenticated);
    return !!this.keycloak?.authenticated;

  }
}
