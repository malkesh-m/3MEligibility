import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { RolesService } from '../../services/setting/role.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const rolesService = inject(RolesService);
  const router = inject(Router);
  
  const requiredRoleId = route.data['requiredRoleId']; // Get the required role ID

  if (!rolesService.hasPermission(requiredRoleId)) {
    router.navigate(['/setting/unauthorized']); // Redirect to Unauthorized Page
    return false;
  }
  return true;
};
