import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthComponent } from './auth.component';
import { LoginComponent } from './login/login.component';
import { ForgetPasswordComponent } from './forget-password/forget-password.component';
import { CallbackComponent } from './callback/callback.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full', data: { title: 'Login' } },
  { path: '', component: AuthComponent },
  {
    path: 'login', component: LoginComponent, data: { title: 'Login' }
  },
  { path: 'callback', component: CallbackComponent, data: { title: 'Callback' } },  // OAuth callback route
  { path: 'forgetPassword', component: ForgetPasswordComponent, data: { title: 'Forget Password' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthRoutingModule { }
