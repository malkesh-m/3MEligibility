import { Component, signal } from '@angular/core';
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

    permissions$ = this.PermissionsService.permissions$;

  setStep(index: number) {
    this.step.set(index);
  }
  permissions = signal<string[]>([]);



  constructor(private PermissionsService: PermissionsService, private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.loadUserPermissions().subscribe({
      next: () => {
        this.PermissionsService.permissions$.subscribe(perms => {
          this.permissions.set(perms);
        });
      },
      error: err => console.error('Failed to load permissions', err)
    });
  }




  loadPermissions() {
    this.authService.loadUserPermissions()
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
}



