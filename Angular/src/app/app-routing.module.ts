import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthLayoutComponent } from './core/components/auth-layout/auth-layout.component';
import { LayoutComponent } from './core/components/layout/layout.component';
import { authGuard } from './core/guards/auth/auth.guard';
import { guestGuard } from './core/guards/guest/guest.guard';

const routes: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    children: [
      { path: '', redirectTo: 'auth', pathMatch: 'full' },
      {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule)
      }
    ]
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'setting', pathMatch: 'full' },
      { 
        path: 'setting', 
        loadChildren: () => import('./features/setting/setting.module').then(m => m.SettingModule) 
      },
      { 
        path: 'security', 
        loadChildren: () => import('./features/security/security.module').then(m => m.SecurityModule) 
      },
      { 
        path: 'connections', 
        loadChildren: () => import('./features/connections/connections.module').then(m => m.ConnectionsModule) 
      },
      { 
        path: 'configuration', 
        loadChildren: () => import('./features/configuration/configuration.module').then(m => m.ConfigurationModule) 
      }
    ]
  },
  { path: '**', redirectTo: 'auth', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
