import { Component, EventEmitter, Output } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { OidcAuthService } from '../../services/auth/oidc-auth.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ChangeDetectorRef } from '@angular/core';
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
  tenantLogoUrl: SafeUrl | null = null;
  tenantName: string = '';
  tenantInitials: string = '';
  @Output() menuToggle = new EventEmitter<void>();

  constructor(private router: Router, private authService: AuthService,
    private translate: TranslateService,
    private oidcAuthService: OidcAuthService,
    private sanitizer: DomSanitizer,
    private headerTitleService: HeaderTitleService,
    private cdr: ChangeDetectorRef
  ) {
    this.currentLanguage = this.translate.currentLang || 'en';
  }

  ngOnInit(): void {
    this.loadUserImage();
    this.loadTenantLogo();
    this.headerTitleService.title$.subscribe(title => {
      if (title) {
        this.pageTitle = title;
        this.cdr.detectChanges(); // Vital to make breadcrumb dynamic on navigation
      }
    });
  }

  private formatTenantName(name: string, id: any): string {
    if (!name) return `Tenant ${id || 'Default'}`;
    const strName = String(name).trim();
    if (!isNaN(Number(strName)) && strName === String(id)) {
      return `Tenant ${strName}`;
    }
    return strName;
  }

  loadTenantLogo() {
    const profile = this.oidcAuthService.getUserProfile();
    console.log('OIDC Profile:', profile);

    let tenantId = profile?.tenant_id || profile?.tenantId || null;
    let localTenantName = profile?.tenant_name || '';

    const currentUserJson = localStorage.getItem('currentUser');
    if (!tenantId && currentUserJson) {
      const currentUser = JSON.parse(currentUserJson);
      tenantId = currentUser?.user?.tenantId || currentUser?.user?.entityId;
      if (!localTenantName) {
        localTenantName = currentUser?.user?.tenantName || '';
      }
    }

    console.log('Extracted Tenant ID:', tenantId);

    if (tenantId) {
      this.authService.getTenantInfo(tenantId).subscribe({
        next: (res) => {
          console.log('Tenant Info API Response:', res);
          const tenantData = res?.data || res?.Data; // handle both camelCase and PascalCase
          if (tenantData && tenantData.name) {
            this.tenantName = this.formatTenantName(tenantData.name, tenantId);
          } else {
            this.tenantName = this.formatTenantName(localTenantName, tenantId);
          }
          
          this.tenantInitials = this.tenantName ? this.tenantName.charAt(0).toUpperCase() : '';
          this.cdr.detectChanges(); // Force UI update
        },
        error: (err) => {
          console.error('Failed to load tenant info API', err);
          this.tenantName = this.formatTenantName(localTenantName, tenantId);
          this.tenantInitials = this.tenantName ? this.tenantName.charAt(0).toUpperCase() : '';
          this.cdr.detectChanges(); // Force UI update
        }
      });
    } else if (profile) {
      // Fallback if no tenantId was found anywhere
      const pName = profile.tenant_name || '';
      this.tenantName = this.formatTenantName(pName, 'Unknown');
      this.tenantInitials = this.tenantName ? this.tenantName.charAt(0).toUpperCase() : '';
      this.cdr.detectChanges(); // Force UI update
    }

    // Always fetch the logo if the ID exists in the profile
    if (profile && profile.tenant_logo_id && !this.tenantLogoUrl) {
        this.authService.getUserImage(profile.tenant_logo_id).subscribe({
          next: (blob) => {
            if (blob && blob.size > 0 && blob.type.startsWith('image/')) {
              const objectURL = URL.createObjectURL(blob);
              this.tenantLogoUrl = this.sanitizer.bypassSecurityTrustUrl(objectURL);
              this.cdr.detectChanges();
            } else {
              console.warn('Invalid logo blob received:', blob);
            }
          },
          error: (err) => {
             console.error('Failed to load tenant logo image, falling back to name initials.', err);
             this.cdr.detectChanges();
          }
        });
    }
  }

  loadUserImage() {
    const profile = this.oidcAuthService.getUserProfile();
    if (profile) {
      console.log(profile)
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
