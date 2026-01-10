import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ValidatorDialogComponent } from '../../components/validator-dialog/validator-dialog.component';
import { ValidationDialogData } from '../../components/validator-dialog/validation-dialog.model';

@Injectable({
  providedIn: 'root',
})
export class ValidationDialogService {
  constructor(private dialog: MatDialog) {}

  openValidationDialog(data: ValidationDialogData) : MatDialogRef<ValidatorDialogComponent> {
    return this.dialog.open(ValidatorDialogComponent, {
          width: '50vw',
          data,
      });
  }
}
