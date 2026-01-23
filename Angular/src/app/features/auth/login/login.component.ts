import { Component } from '@angular/core';
import { OidcAuthService } from '../../../core/services/auth/oidc-auth.service';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: false
})
export class LoginComponent  {
  isLoading = false;
ngOnInit() {
     this.authService.loadUserPermissions();

}

  constructor(private oidcAuthService: OidcAuthService,private authService: AuthService ) { }

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
