import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PermissionsService {

  private permissionsSubject = new BehaviorSubject<string[]>([]);
  permissions$ = this.permissionsSubject.asObservable();

  constructor() {
    const stored = localStorage.getItem('userPermissions');
    if (stored) {
      this.permissionsSubject.next(JSON.parse(stored));
    }
  }

  setPermissions(permissions: string[] = []): void {
    this.permissionsSubject.next(permissions);
    localStorage.setItem('userPermissions', JSON.stringify(permissions));
  }

hasPermission(permission: string): boolean {
  if (!permission) return false;

  return this.permissionsSubject.value
    .map(p => p.toLowerCase())
    .includes(permission.toLowerCase());
}

  clearPermissions(): void {
    this.permissionsSubject.next([]);
    localStorage.removeItem('userPermissions');
  }
}

