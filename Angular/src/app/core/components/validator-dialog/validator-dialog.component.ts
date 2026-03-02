import { Component, inject, Inject, Input, Output } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FactorsService } from '../../services/setting/factors.service';
import { ValidationType, ValidationDialogData } from './validation-dialog.model';
import { RulesService } from '../../services/setting/rules.service';
import { CardsService } from '../../services/setting/cards.service';
import { ProductCardsService } from '../../services/setting/product-cards.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-validator-dialog',
  standalone: false,
  templateUrl: './validator-dialog.component.html',
  styleUrls: ['./validator-dialog.component.scss'],
})
export class ValidatorDialogComponent {
  private _snackBar = inject(MatSnackBar);
  parameters: any[] = [];
  filteredParameters: any[] = [];
  factorsByParameter: { [parameterName: string]: any[] } = {};
  selectedFactors: { [parameterName: string]: string } = {};
  expression: string = '';
  expshown: string = '';
  actionType: string = '';
  evaluateExpression: string = '';
  validationMessage: string = '';
  validationResult: boolean | null = null;
  valideeId: number;
  validationType: ValidationType;
  AllList: any[] = [];

  constructor(
    private factorService: FactorsService,
    private eRulesService: RulesService,
    private eCardsService: CardsService,
    private pCardsService: ProductCardsService,
    public dialogRef: MatDialogRef<ValidatorDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ValidationDialogData
  ) {
    this.expshown = data?.expshown || '';
    this.actionType = data?.actionType;
    this.evaluateExpression = data?.expshown || '';
    this.expression = data?.expression;
    this.parameters = data?.parameters || [];
    this.valideeId = data.valideeId || 0;
    this.validationType = data.validationType;
    this.extractParametersFromExpression();
  }
  extractParametersFromExpression() {

    // Normalize expression: remove multiple spaces, trim, lowercase
    let cleaned = this.expshown
      .replace(/\s+/g, ' ')       // convert multiple spaces → single space
      .trim()
      .toLowerCase();

    // Split into tokens (words)
    const tokens = cleaned.split(' ');

    this.filteredParameters = this.parameters.filter(p => {
      const paramNormalized = p.parameterName.toLowerCase();

      // Match only if entire token equals parameter
      return tokens.includes(paramNormalized);
    });

    this.filteredParameters.forEach(param => {
      if (param) {
        this.fetchFactorsForParameter(param.parameterName, param.parameterId);
      }
    });
  }

  fetchFactorsForParameter(parameterName: string, parameterId: number) {
    this.factorService.getFactorsByParameterId(parameterId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factorsByParameter[parameterName] = response.data ?? [];
        } else {
          this.factorsByParameter[parameterName] = [];
        }
      },
      error: (error) => {
        console.error('Error fetching factors:', error);
        this.factorsByParameter[parameterName] = [];
      },
    });
  }

  onFactorChange(parameterName: string, event: Event) {
    const selectElement = event.target as HTMLSelectElement;
    this.selectedFactors[parameterName] = selectElement?.value || '';
    this.updateEvaluateExpression();
  }

  updateEvaluateExpression() {
    this.evaluateExpression = this.expshown;
    Object.entries(this.selectedFactors).forEach(([paramName, factorValue]) => {
      if (factorValue) {
        this.evaluateExpression = this.replaceSelectedCondition(
          this.evaluateExpression,
          paramName,
          factorValue
        );
      }
    });
    this.evaluateExpression = this.evaluateExpression.replace(/\s+/g, ' ').trim();
  }

  replaceSelectedCondition(expshown: string, paramName: string, factorValue: string): string {
    const regex = new RegExp(
      `(\\b${paramName}\\b)\\s*([=<>!]+|In List|Not In List|Range)\\s*([^()\\s]+)`,
      'g'
    );
    return expshown.replace(regex, (match, _param, operator, existingValue) => {
      return `${factorValue} ${operator} ${existingValue}`;
    });
  }

  validateExpression() {
    console.log(this.expshown)
    const allFactorsSelected = Object.keys(this.selectedFactors).length === this.filteredParameters.length;
    if (!allFactorsSelected) {
      this.validationMessage = '⚠️ Please select a factor for each parameter before validating.';
      this.validationResult = false;
      return;
    }

    const expressionData: { [parameterId: number]: string } = {};
    this.filteredParameters.forEach((param) => {
      if (param && this.selectedFactors[param.parameterName]) {
        expressionData[param.parameterId] = this.selectedFactors[param.parameterName];
      }
    });

    if (this.validationType == 'ERule') {
      if (this.actionType === 'form') {
        // Use evaluateExpression: the backend now handles both
        // Range (via literal regex) and In List (via new Case 2b literal handler).
        // Factor names have been substituted with user-selected values.
        this.expression = this.evaluateExpression;
        this.eRulesService.validateFormErule(this.expression, expressionData).subscribe({
          next: (response) => {
            const validationPassed = response.isValidationPassed;
            this.validationMessage = validationPassed ? '✅ Expression is valid!' : '❌ Expression is invalid!';
            if (validationPassed) {
              setTimeout(() => {
                this.dialogRef.close(true);
              }, 100000);
              // ✅ ADD THIS
            }
          },

          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      } else {
        this.eRulesService.validateRule(this.valideeId, expressionData).subscribe({
          next: (response) => {
            const validationPassed = response.isValidationPassed;
            this.validationMessage = validationPassed ? '✅ Expression is valid!' : '❌ Expression is invalid!';
          },

          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      }
    }

    if (this.validationType == 'ECard') {
      if (this.actionType === 'form') {
        // Use evaluateExpression: the backend handles both Range and In List via literal matching.
        this.expression = this.evaluateExpression;
        console.log('ECard form expression:', this.expression);
        this.eCardsService.validateFormECard(this.expression, expressionData).subscribe({
          next: (response) => {
            const validationPassed = response.isValidationPassed;
            this.validationMessage = validationPassed ? '✅ Expression is valid!' : '❌ Expression is invalid!';
          },

          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      } else {
        this.eCardsService.validateCard(this.valideeId, expressionData).subscribe({
          next: (response) => {
            const validationPassed = response.isValidationPassed;
            this.validationMessage = validationPassed ? '✅ Expression is valid!' : '❌ Expression is invalid!';
          },

          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      }
    }

    if (this.validationType == 'PCard') {
      if (this.actionType === 'form') {
        // Use evaluateExpression: the backend handles both Range and In List via literal matching.
        this.expression = this.evaluateExpression;
        console.log('PCard form expression:', this.expression);
        this.pCardsService.validateFormPCard(this.expression, expressionData).subscribe({
          next: (response) => {
            const validationPassed = response.isValidationPassed;
            this.validationMessage = validationPassed ? '✅ Expression is valid!' : '❌ Expression is invalid!';
          },

          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      } else {
        this.pCardsService.validatePCard(this.valideeId, expressionData).subscribe({
          next: (response) => {
            const validationPassed = response.isValidationPassed;
            this.validationMessage = validationPassed ? '✅ Expression is valid!' : '❌ Expression is invalid!';
          },

          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      }
    }
  }
}
