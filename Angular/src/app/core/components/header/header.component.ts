import { Component, EventEmitter, Output } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: false,

  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  currentLanguage: string;
  @Output() menuToggle = new EventEmitter<void>();

  constructor(private router: Router, private authService: AuthService,
    private translate: TranslateService
  ) {
    this.currentLanguage = this.translate.currentLang || 'en';
  }

  toggleSidebar() {
    this.menuToggle.emit()
  }
  logoutUser() {
    this.authService.logout().subscribe({
      next: (res: any) => {
      },
      error: (err: any) => {
        this.router.navigate(['/auth/login']);
      }
    });
  }

  changeLanguage(lang: string) {
    this.translate.use(lang);
    this.currentLanguage = lang;
    localStorage.setItem('appLanguage', lang);
  }
}
