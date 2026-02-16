import { Component } from '@angular/core';
import { OidcAuthService } from '../../../core/services/auth/oidc-auth.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: false
})
export class LoginComponent  {
  isLoading = false;
  currentLanguage: string;
ngOnInit() {
     this.authService.loadUserPermissions();

}

  constructor(
    private oidcAuthService: OidcAuthService,
    private authService: AuthService,
    private translate: TranslateService
  ) {
    this.currentLanguage = this.translate.currentLang || 'en';
  }

  async loginWithKeycloak() {
    this.isLoading = true;
    try {
      await this.oidcAuthService.login();
     
    } catch (error) {
      console.error('Login error:', error);
      this.isLoading = false;
    }
  }

  changeLanguage(lang: string) {
    this.translate.use(lang);
    this.currentLanguage = lang;
  }
}
