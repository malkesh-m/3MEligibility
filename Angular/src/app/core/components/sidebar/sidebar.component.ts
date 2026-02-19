import { Component, signal } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { OidcAuthService } from '../../services/auth/oidc-auth.service';
import { PermissionsService } from '../../services/setting/permission.service';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: false,

  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  step = signal(0);
  tenantLogoUrl: SafeUrl | null = null;

  permissions$ = this.PermissionsService.permissions$;

  setStep(index: number) {
    this.step.set(index);
  }
  permissions = signal<string[]>([]);



  constructor(private PermissionsService: PermissionsService, private authService: AuthService,
    private oidcAuthService: OidcAuthService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit(): void {
    this.authService.loadUserPermissions().subscribe({
      next: () => {
        this.PermissionsService.permissions$.subscribe(perms => {
          this.permissions.set(perms);
        });
      },
      error: err => console.error('Failed to load permissions', err)
    });
    this.loadTenantLogo();
  }

  loadTenantLogo() {
    const profile = this.oidcAuthService.getUserProfile();
    if (profile && profile.tenant_logo_id) {
      this.authService.getUserImage(profile.tenant_logo_id).subscribe({
        next: (blob) => {
          const objectURL = URL.createObjectURL(blob);
          this.tenantLogoUrl = this.sanitizer.bypassSecurityTrustUrl(objectURL);
        },
        error: (err) => console.error('Failed to load tenant logo', err)
      });
    }
  }




  loadPermissions() {
    this.authService.loadUserPermissions()
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
}



