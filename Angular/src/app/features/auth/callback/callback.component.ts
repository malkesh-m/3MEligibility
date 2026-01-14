import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OidcAuthService } from '../../../core/services/auth/oidc-auth.service';

@Component({
    selector: 'app-callback',
    template: '<div class="loading">Processing login...</div>',
    styles: ['.loading { text-align: center; padding: 50px; font-size: 18px; }'],
    standalone: false
})
export class CallbackComponent implements OnInit {
    constructor(
        private oidcAuthService: OidcAuthService,
        private router: Router
    ) { }

    async ngOnInit() {
        try {
            await this.oidcAuthService.handleCallback();
            // Redirect to dashboard after successful login
            this.router.navigate(['/setting']);
        } catch (error) {
            console.error('Callback error:', error);
            // Redirect to login on error
            this.router.navigate(['/auth/login']);
        }
    }
}
