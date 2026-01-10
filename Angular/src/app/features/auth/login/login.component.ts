import { Component, inject, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth/auth.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-login',
  standalone: false,

  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  currentLanguage: string;
  private _snackBar = inject(MatSnackBar);
  loginForm: FormGroup;
  forgetForm: FormGroup;
  isForgetPassword: boolean = false;
  isForceReset: boolean = false;
  userIdForReset: number = 0;
  resetForm!: FormGroup;
  @Output() menuToggle = new EventEmitter<void>();
  private passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router, private translate: TranslateService) {
    this.loginForm = this.fb.group({
      loginId: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
    this.forgetForm = this.fb.group({
      email: ['', [Validators.required]],
    });
    this.resetForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(this.passwordRegex)
        ]
      ],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
    
    this.currentLanguage = this.translate.currentLang || 'en';
  }
  passwordMatchValidator(group: FormGroup) {
    const currentCtrl = group.get('currentPassword');
    const newCtrl = group.get('newPassword');
    const confirmCtrl = group.get('confirmPassword');

    if (!currentCtrl || !newCtrl || !confirmCtrl) return null;

    const current = currentCtrl.value;
    const newPwd = newCtrl.value;
    const confirmPwd = confirmCtrl.value;

    if (newCtrl.errors?.['sameAsCurrent']) {
      const { sameAsCurrent, ...rest } = newCtrl.errors;
      newCtrl.setErrors(Object.keys(rest).length ? rest : null);
    }

    if (confirmCtrl.errors?.['mismatch']) {
      const { mismatch, ...rest } = confirmCtrl.errors;
      confirmCtrl.setErrors(Object.keys(rest).length ? rest : null);
    }

    if (current && newPwd && current === newPwd) {
      newCtrl.setErrors({
        ...(newCtrl.errors || {}),
        sameAsCurrent: true
      });
    }

    if (newPwd && confirmPwd && newPwd !== confirmPwd) {
      confirmCtrl.setErrors({
        ...(confirmCtrl.errors || {}),
        mismatch: true
      });
    }

    return null;
  }
  onSubmit() {
    if (this.loginForm.valid) {
      try {
        this.authService.login(this.loginForm.value).subscribe({
          next: (response) => {
            (console.log(response))
            if (response.forcePasswordChange || response.ForcePasswordChange) {
              this.isForceReset = true;
              this.userIdForReset =
                response.user?.userId ||
                response.user?.UserId ||   
                response.User?.UserId ||
                0;
              this.resetForm.patchValue({
                currentPassword: this.loginForm.value.password
              });
              this._snackBar.open(
                response.message || 'Password expired. Please reset.',
                'Okay',
                { duration: 4000 }
              );
              return; 
            }

            if (response.token) {
              localStorage.setItem('token', response.token);
              this.router.navigate(['/setting']);
            }
          },
          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      } catch (error) {
        console.log("error ====> ", error)
      }
    } else {
      console.log('Form Invalid');
    }
  }
  
  navigateToLogin(){
    this.isForgetPassword=false;
  }
  get confirmPassword() {
    return this.resetForm.get('confirmPassword');
  }
  forgetOnSubmit(){
    if (this.forgetForm.valid) {
      try{
        const loginUrl = window.location.origin +  '/auth/forgetPassword';
        const requestBody = {
          email: this.forgetForm.value.email,
          resetLink: loginUrl
        }
        this.authService.forget(requestBody).subscribe({
          next: (response) => {
      
            if (response.isSuccess) {
              this.isForgetPassword = false;
              this._snackBar.open(response.message, 'Okay', {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
            } else {
              this._snackBar.open(response.message, 'Okay', {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
            }
          },
          error: (error) => console.error('Error adding group:', error),
        });
      }catch (error) {
        console.log("error ====> ", error)
      }
    }
  }
  resetOnSubmit() {
    if (!this.resetForm.valid) return;

    const payload = {
      userId: this.userIdForReset,
      currentPassword: this.resetForm.value.currentPassword,
      newPassword: this.resetForm.value.newPassword,
      confirmNewPassword: this.resetForm.value.confirmPassword
    };

    this.authService.changePassword(payload).subscribe({
      next: (res: any) => {
        if (res.isSuccess) {
          // Reset the form fields
          this.resetForm.reset();

          // Set a placeholder message in the newPassword field
          this.resetForm.get('newPassword')?.setValue('');
          this.resetForm.get('newPassword')?.setErrors({ required: true });
          this.loginForm.get('password')?.setValue('');
          this._snackBar.open(
            res.message || 'Password changed successfully. Please fill new password to login.',
            'Okay',
            
            { duration: 4000 }
          );

          // Reset flags
          this.isForceReset = false;
          this.userIdForReset = 0;

          // Optionally redirect to login form
          this.isForgetPassword = false;
        }
        else {
          this._snackBar.open(res.message, 'Okay', { duration: 3000 });
        }
      },
      error: (err) => {
        this._snackBar.open(
          err?.error?.message || 'Password reset failed',
          'Okay',
          { duration: 3000 }
        );
      }
    });
  }

  forgetPassword() {
    this.isForgetPassword = true;
  }
  get showLoginForm() {
    return !this.isForgetPassword && !this.isForceReset;
  }
  changeLanguage(lang: string) {
    this.translate.use(lang);
    this.currentLanguage = lang;
  }
  hasTypedPassword(): boolean {
    const password = this.resetForm.get('newPassword')?.value || '';
    return password.length > 0;
  }

  hasLengthError(): boolean {
    const password = this.resetForm.get('newPassword')?.value || '';
    return password.length > 0 && password.length < 8;
  }

  hasUppercase(): boolean {
    const password = this.resetForm.get('newPassword')?.value || '';
    return /[A-Z]/.test(password);
  }

  hasLowercase(): boolean {
    const password = this.resetForm.get('newPassword')?.value || '';
    return /[a-z]/.test(password);
  }

  hasNumber(): boolean {
    const password = this.resetForm.get('newPassword')?.value || '';
    return /[0-9]/.test(password);
  }

  hasSpecialChar(): boolean {
    const password = this.resetForm.get('newPassword')?.value || '';
    return /[^A-Za-z0-9]/.test(password);
  }

  onPasswordBlur(): void {
    // Trigger validation display
    this.resetForm.get('newPassword')?.markAsTouched();
  }
  canSubmitReset(): boolean {
    if (!this.resetForm.valid) {
      return false;
    }

    // Check if passwords match
    const newPwd = this.resetForm.get('newPassword')?.value;
    const confirmPwd = this.resetForm.get('confirmPassword')?.value;

    if (newPwd !== confirmPwd) {
      return false;
    }

    // Check if new password is same as current
    const currentPwd = this.resetForm.get('currentPassword')?.value;
    if (currentPwd && newPwd && currentPwd === newPwd) {
      return false;
    }

    // Check all password rules
    return this.hasUppercase() &&
      this.hasLowercase() &&
      this.hasNumber() &&
      this.hasSpecialChar() &&
      newPwd.length >= 8;
  }
}
