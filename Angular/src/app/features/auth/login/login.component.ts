import { Component } from '@angular/core';
import { OidcAuthService } from '../../../core/services/auth/oidc-auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: false
})
export class LoginComponent {
  isLoading = false;

  constructor(private oidcAuthService: OidcAuthService) { }

  async loginWithKeycloak() {
    this.isLoading = true;
    try {
      await this.oidcAuthService.login();
    } catch (error) {
      console.error('Login error:', error);
      this.isLoading = false;
    }
  }
}
