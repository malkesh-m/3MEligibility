import { Component, signal } from '@angular/core';
import { RolesService } from '../../services/setting/role.service';
import { AuthService } from '../../services/auth/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  step = signal(0);

    permissions$ = this.rolesService.permissions$;

  setStep(index: number) {
    this.step.set(index);
  }
  permissions = signal<string[]>([]);



  constructor(private rolesService: RolesService, private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.loadUserPermissions().subscribe({
      next: () => {
        this.rolesService.permissions$.subscribe(perms => {
          this.permissions.set(perms);
        });
      },
      error: err => console.error('Failed to load permissions', err)
    });
  }




  loadPermissions() {
    this.authService.loadUserPermissions()
  }

  hasPermission(roleId: string): boolean {
    return this.rolesService.hasPermission(roleId);
  }
}
