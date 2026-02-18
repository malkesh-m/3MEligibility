import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MakerCheckerService } from '../../../../core/services/setting/makerchecker.service';
import { PermissionsService } from '../../../../core/services/setting/permission.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-maker-checker-details-dialog',
  standalone: false,
  templateUrl: './maker-checker-details-dialog.component.html',
  styleUrl: './maker-checker-details-dialog.component.scss'
})
export class MakerCheckerDetailsDialogComponent {
  parsedOldJson: any;
  parsedNewJson: any;
  comment: string = '';
  changedFields: { key: string; oldValue?: any; newValue?: any }[] = [];
  fromhistory: boolean = false;

  constructor(
    public dialogRef: MatDialogRef<MakerCheckerDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private snackBar: MatSnackBar,
    private makerCheckerService: MakerCheckerService,
    private PermissionsService: PermissionsService,
    private translate: TranslateService
  ) {
    // Handle both MakerChecker & Audit log formats
    const oldValue = data.oldValueJson || data.oldValue;
    const newValue = data.newValueJson || data.newValue;
    this.fromhistory = !!data.fromhistory;

    this.parsedOldJson = this.parseJson(oldValue);
    this.parsedNewJson = this.parseJson(newValue);

    this.extractChangedFields();
  }

  parseJson(jsonString: string): any {
    try {
      const parsed = JSON.parse(jsonString);
      return parsed && typeof parsed === 'object' ? parsed : {};
    } catch {
      return {};
    }
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) return '';
    if (typeof value === 'boolean') return value ? this.translate.instant('Yes') : this.translate.instant('No');
    if (typeof value === 'string' && value.match(/^\d{4}-\d{2}-\d{2}T/)) {
      const date = new Date(value);
      return date.toLocaleString('en-IN', {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true,
      });
    }
    if (Array.isArray(value) && value.length > 0) return JSON.stringify(value, null, 2);
    if (Array.isArray(value) && value.length === 0) return ''; // Skip empty arrays
    return value.toString();
  }

  extractChangedFields(): void {
    const oldData = this.parsedOldJson || {};
    const newData = this.parsedNewJson || {};

    const allKeys = new Set([...Object.keys(oldData), ...Object.keys(newData)]);

    this.changedFields = Array.from(allKeys)
      .filter((key) => {
        const oldVal = oldData[key];
        const newVal = newData[key];

        if (Array.isArray(oldVal) || Array.isArray(newVal)) return false;

        const bothEmpty = (
          (oldVal == null || oldVal === '') &&
          (newVal == null || newVal === '')
        );
        return !bothEmpty && JSON.stringify(oldVal) !== JSON.stringify(newVal);
      })
      .map((key) => ({
        key,
        oldValue: this.formatValue(oldData[key]),
        newValue: this.formatValue(newData[key]),
      }));
  }


  getKeyLabel(key: unknown): string {
    return typeof key === 'string' ? key : '';
  }

  isArray(value: any): boolean {
    return Array.isArray(value);
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  approve(): void {
    this.makerCheckerService
      .updateStatus(this.data.makerCheckerId, 'Approved', this.comment)
      .subscribe({
        next: () => {
          this.dialogRef.close({
            action: 'Approved',
            id: this.data.makerCheckerId,
            comment: this.comment,
          });
        },
        error: (error) => {
          this.snackBar.open(error?.error?.errors?.Comment?.[0] || 'Something went wrong', 'Close', {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000,
          });
        },
      });
  }

  reject(): void {
    this.makerCheckerService
      .updateStatus(this.data.makerCheckerId, 'Declined', this.comment)
      .subscribe({
        next: () => {
          this.dialogRef.close({
            action: 'Declined',
            id: this.data.makerCheckerId,
            comment: this.comment,
          });
        },
        error: (error) => {
          this.snackBar.open(this.translate.instant(error?.error?.errors?.Comment?.[0] || 'Something went wrong'), this.translate.instant('Close'), {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000,
          });
        },
      });
  }
}



