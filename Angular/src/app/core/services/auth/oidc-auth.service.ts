// src/app/core/services/auth/oidc-auth.service.ts
import { Injectable } from '@angular/core';
import { UserManager, User, UserManagerSettings } from 'oidc-client-ts';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OidcAuthService {
    private userManager: UserManager;
    private currentUser: User | null = null;

    constructor() {
        const settings: UserManagerSettings = {
            authority: environment.identitySettings.authority + '/realms/3M',
            client_id: environment.identitySettings.clientId,
            redirect_uri: window.location.origin + '/auth/callback', // Hardcoded to match Route, using env origin would be redundant here
            post_logout_redirect_uri: window.location.origin,
            response_type: 'code',
            scope: environment.identitySettings.scope,
            automaticSilentRenew: true,
            silent_redirect_uri: window.location.origin + '/assets/silent-renew.html',
            response_mode: 'fragment'
        };

        this.userManager = new UserManager(settings);

        this.userManager.events.addUserLoaded((user) => {
            console.log('User loaded:', user.profile);
            this.currentUser = user;
        });

        this.userManager.events.addUserUnloaded(() => {
            console.log('User unloaded');
            this.currentUser = null;
        });

        this.userManager.events.addAccessTokenExpiring(() => {
            console.log('Access token expiring...');
        });

        this.userManager.events.addAccessTokenExpired(() => {
            console.log('Access token expired');
            this.login();
        });
    }

    async init(): Promise<boolean> {
        try {

            // Check if user is already authenticated
            this.currentUser = await this.userManager.getUser();

            if (this.currentUser && !this.currentUser.expired) {
                return true;
            }

            return false;
        } catch (error) {
            return false;
        }
    }

    async login(): Promise<void> {
        await this.userManager.signinRedirect();
    }

    async handleCallback(): Promise<void> {
        console.log(' Handling OAuth callback...');
        try {
            const user = await this.userManager.signinRedirectCallback();
            this.currentUser = user;
        } catch (error) {
            throw error;
        }
    }

    async logout(): Promise<void> {
        await this.userManager.signoutRedirect();
    }

    isAuthenticated(): boolean {
        return this.currentUser != null && !this.currentUser.expired;
    }

    getUser(): User | null {
        return this.currentUser;
    }

    async getAccessToken(): Promise<string | null> {
        const user = await this.userManager.getUser();
        return user?.access_token || null;
    }

    getUserProfile(): any {
        return this.currentUser?.profile || null;
    }
}
