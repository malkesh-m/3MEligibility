// core/services/auth/keycloak-init-service.service.ts
import { Injectable } from '@angular/core';
import Keycloak from 'keycloak-js';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class KeycloakService {
  private keycloak: Keycloak | undefined;

  async init(): Promise<boolean> {
    console.log('Step 1: Initializing Keycloak with config:', {
      url: environment.identitySettings.authority,
      realm: '3M',
      clientId: environment.identitySettings.clientId
    });

    // Check current URL for any OAuth parameters (both query and hash)
    const currentUrl = window.location.href;
    const queryParams = new URLSearchParams(window.location.search);
    const hashParams = new URLSearchParams(window.location.hash.substring(1)); // Remove # and parse



    if (hashParams.has('code') || hashParams.has('state') || hashParams.has('error')) {

      history.replaceState(null, '', window.location.pathname + window.location.search);


    }

    this.keycloak = new Keycloak({
      url: environment.identitySettings.authority,
      realm: '3M',
      clientId: environment.identitySettings.clientId
    });

    console.log(' Step 2: Keycloak instance created');

    try {

      const authenticated = await this.keycloak.init({
        onLoad: 'check-sso',
        checkLoginIframe: false,
        enableLogging: true
      });


      if (this.keycloak.onTokenExpired) {
        this.keycloak.onTokenExpired = () => {
          this.keycloak?.updateToken(30).catch(() => {
            console.error('Failed to refresh token, redirecting to login');
            this.login();
          });
        };
      }

      if (authenticated) {

        // Load user profile
        try {
          const profile = await this.keycloak.loadUserProfile();
        } catch (profileErr) {
          console.warn('Failed to load user profile:', profileErr);
        }
      } else {
      }

      return authenticated;
    } catch (err: any) {


      // Common error scenarios
      if (err?.message?.includes('CORS')) {
        console.error(' CORS Error: Check Keycloak server Web Origins configuration');
      } else if (err?.message?.includes('Network')) {
        console.error(' Network Error: Cannot reach Keycloak server');
      } else if (err?.message?.includes('404')) {
        console.error(' 404 Error: Keycloak realm or client not found');
      }

      return false;
    }
  }

  /**
   * Get token from Keycloak instance (not localStorage)
   * Automatically refreshes if needed
   */
  async getToken(): Promise<string> {
    if (!this.keycloak?.token) return '';

    try {
      // Refresh token if it expires in the next 30 seconds
      await this.keycloak.updateToken(30);
      return this.keycloak.token || '';
    } catch (error) {
      this.login(); // Redirect to login if refresh fails
      return '';
    }
  }

  isLoggedIn(): boolean {
    return !!this.keycloak?.authenticated;
  }

  login(): void {
    const redirectUri = environment.identitySettings?.loginCallBackPath || window.location.origin;
    this.keycloak?.login({ redirectUri });
  }

  logout(): void {
    const redirectUri = environment.identitySettings?.logoutCallBackPath || window.location.origin;
    this.keycloak?.logout({ redirectUri });
  }

  getUserProfile(): any {
    return this.keycloak?.profile || null;
  }

  getUsername(): string {
    return this.keycloak?.tokenParsed?.['preferred_username'] || '';
  }

  getUserPermissions(): string[] {
    return this.keycloak?.tokenParsed?.['realm_access']?.['roles'] || [];
  }

  hasPermission(permission: string): boolean {
    return this.getUserPermissions().includes(permission);
  }
}
