import { Component } from '@angular/core';
import { OidcAuthService } from './core/services/auth/oidc-auth.service';
import { AuthService } from './core/services/auth/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  standalone: false,
})
export class AppComponent {
  title = '3MEligibilityFrontend';
   constructor(
    private authService: AuthService,
    private oidcAuthService: OidcAuthService
  ) {}

  ngOnInit(): void {
    if (this.oidcAuthService.isAuthenticated()) {
      this.authService.loadUserPermissions();
    }
}
}