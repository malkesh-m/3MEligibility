import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RolesService {

  private permissionsSubject = new BehaviorSubject<string[]>([]);
  permissions$ = this.permissionsSubject.asObservable();

  constructor() {
    const stored = localStorage.getItem('userRoles');
    if (stored) {
      this.permissionsSubject.next(JSON.parse(stored));
    }
  }

  setRoles(permissions: string[] = []): void {
    this.permissionsSubject.next(permissions);
    localStorage.setItem('userRoles', JSON.stringify(permissions));
  }

  hasPermission(permission: string): boolean {
    return this.permissionsSubject.value.includes(permission);
  }

  clearRoles(): void {
    this.permissionsSubject.next([]);
    localStorage.removeItem('userRoles');
  }
}
