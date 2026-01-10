import { Component, signal } from '@angular/core';
import { RolesService } from '../../services/setting/role.service';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  step = signal(0);

  constructor(private rolesService:RolesService){}
  
  setStep(index: number) {
    this.step.set(index);
  }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }
}
