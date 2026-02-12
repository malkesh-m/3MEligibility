import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { PermissionsService } from '../../services/setting/permission.service';

export const permissionGuard: CanActivateFn = (route, state) => {
  const permissionsService = inject(PermissionsService);
  const router = inject(Router);
  
  const requiredPermissionId = route.data['requiredPermissionId']; // Get the required permission ID

  if (!permissionsService.hasPermission(requiredPermissionId)) {
    router.navigate(['/setting/unauthorized']); // Redirect to Unauthorized Page
    return false;
  }
  return true;
};



