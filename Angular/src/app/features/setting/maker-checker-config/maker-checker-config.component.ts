import { Component, effect, inject, OnInit, signal, Signal } from '@angular/core';
import { MakerCheckerService } from '../../../core/services/setting/makerchecker.service';
import { MatDialog } from '@angular/material/dialog';
import { UtilityService } from '../../../core/services/utility/utils';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RolesService } from '../../../core/services/setting/role.service';

@Component({
  selector: 'app-maker-checker-config',
  standalone: false,
  templateUrl: './maker-checker-config.component.html',
  styleUrl: './maker-checker-config.component.scss'
})
export class MakerCheckerConfigComponent implements OnInit {
  private service = inject(MakerCheckerService);
  isMakerCheckerEnabled: Signal<boolean> = this.service.isMakerCheckerEnabled;
  maxChildEntityLimit: string = "";
  private _snackBar = inject(MatSnackBar);
  apiSuccessMsg = "Max child entity limit updated successfully";
  apiFailureMsg = "Error updating Maker-Checker config";
  isDataReady = signal(false);
  private updateReadyEffect = effect(() => {
    const value = this.service.isMakerCheckerEnabled();
    if (value === true || value === false) {
      this.isDataReady.set(true);
    }
  });
  constructor(private dialog: MatDialog,private rolesService:RolesService,private utilityService: UtilityService) {}

  ngOnInit(): void {
    this.service.getMakerCheckerConfig();  // API call sets the signal
   
  }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  toggleMakerChecker(event: any) {
    const isEnabled = event.checked;
    this.service.updateMakerCheckerConfig(isEnabled).subscribe({
      next: () => {
        this.service.getMakerCheckerConfig(); // ✅ Fetch updated value from API after change
      },
      error: (error) => {
        console.error('Error updating Maker-Checker config:', error);
      }
    });
  }

  updateMaxChildEntityValue() {
    // Ensure the value is a valid number before sending to API
    const numericValue = Number(this.maxChildEntityLimit);

    if (!isNaN(numericValue) && numericValue >= 0) {
      this.service.updateChildEntityLimitConfig(numericValue).subscribe({
        next: () => {
          this._snackBar.open(this.apiSuccessMsg, 'Okay', {
            duration: 2000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
        error: (error) => {
          //console.error('Error updating Maker-Checker config:', error);
          this._snackBar.open(this.apiFailureMsg, 'Okay', {
            duration: 2000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        }
      });
    } else {
      console.warn('Invalid maxChildEntityLimit value:', this.maxChildEntityLimit);
    }
  }



  sanitizeCode(event: any) {
    const sanitizedValue = this.utilityService.sanitizeCode(event.target.value);
    // Ensure that only a number or valid string is set
    this.maxChildEntityLimit = sanitizedValue ? sanitizedValue.toString() : '';
  }

}
