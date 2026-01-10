import { HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class RolesService {
  private userRoleIds: number[] = []; // Stores allowed role IDs

  constructor() {
    this.loadRoles(); // Load roles when service is initialized
  }
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'X-Component': 'Roles'
    });
  }
  setRoles(roles: any[]) {
    this.userRoleIds = roles.map(role => role.roleId);
    localStorage.setItem('userRoles', JSON.stringify(this.userRoleIds)); // Store in localStorage
  }

  hasPermission(requiredRoleId: number): boolean {
    return this.userRoleIds.includes(requiredRoleId);
  }

  private loadRoles() {
    const storedRoles = localStorage.getItem('userRoles');
    if (storedRoles) {
      this.userRoleIds = JSON.parse(storedRoles);
    }
  }

  clearRoles() {
    this.userRoleIds = [];
    localStorage.removeItem('userRoles'); // Clear roles on logout
  }
}
