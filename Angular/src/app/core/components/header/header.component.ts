import { Component, EventEmitter, Output } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { OidcAuthService } from '../../services/auth/oidc-auth.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { HeaderTitleService } from '../../services/header-title.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-header',
  standalone: false,

  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  currentLanguage: string;
  userImageUrl: SafeUrl | null = null;
  userName: string = '';
  userInitials: string = '';
  pageTitle: string = 'Dashboard';
  @Output() menuToggle = new EventEmitter<void>();

  constructor(private router: Router, private authService: AuthService,
    private translate: TranslateService,
    private oidcAuthService: OidcAuthService,
    private sanitizer: DomSanitizer,
    private headerTitleService: HeaderTitleService
  ) {
    this.currentLanguage = this.translate.currentLang || 'en';
  }

  ngOnInit(): void {
    this.loadUserImage();
    this.headerTitleService.title$.subscribe(title => {
      if (title) {
        this.pageTitle = title;
      }
    });
  }

  loadUserImage() {
    const profile = this.oidcAuthService.getUserProfile();
    if (profile) {
      this.userName = profile.name || profile.preferred_username || 'User';
      this.userInitials = this.getInitials(this.userName);

      if (profile.user_photo_id) {
        this.authService.getUserImage(profile.user_photo_id).subscribe({
          next: (blob) => {
            const objectURL = URL.createObjectURL(blob);
            this.userImageUrl = this.sanitizer.bypassSecurityTrustUrl(objectURL);
          },
          error: (err) => console.error('Failed to load user image', err)
        });
      }
    }
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0][0].toUpperCase();
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

  goToProfile() {
    if (environment.identityAccountUrl?.Url) {
      window.open(environment.identityAccountUrl.Url, '_blank');
    }
  }
}
