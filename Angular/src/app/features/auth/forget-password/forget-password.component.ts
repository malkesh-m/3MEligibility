import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-forget-password',
  standalone: false,
  templateUrl: './forget-password.component.html',
  styleUrls: ['./forget-password.component.scss']
})
export class ForgetPasswordComponent {
  forgetForm: FormGroup;
  private _snackBar = inject(MatSnackBar);
  token: string | null = null;
  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router, private route: ActivatedRoute, private translate: TranslateService) {
    this.forgetForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmpassword: ['', [Validators.required]],
    }, {
      validators: this.passwordsMatchValidator
    });
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token');
  }


  passwordsMatchValidator(group: AbstractControl): { [key: string]: boolean } | null {
    const password = group.get('password')?.value;
    const confirmpassword = group.get('confirmpassword')?.value;
    return password === confirmpassword ? null : { passwordMismatch: true };
  }

  navigateToLogin() {
    this.router.navigate(['/login']);
  }

  onSubmit() {
    if (this.forgetForm.valid) {
      if (!this.token) {
        this._snackBar.open(this.translate.instant('Token not found'), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
      }
      const requestBody = {
        token: this.token,
        newPassword: this.forgetForm.value.password
      }
      this.authService.resetPassword(requestBody).subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: (error) => {
          this._snackBar.open(this.translate.instant(error.error.message || 'An error occurred'), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            duration: 3000,
            verticalPosition: 'top',
          });
        }
      });
    } else {
      console.log('Form Invalid');
    }
  }
}
