import { Component, EventEmitter, inject, Input, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ExceptionManagementService } from '../../../../../core/services/setting/exception-management.service';
import { FactorsService } from '../../../../../core/services/setting/factors.service';
import { UtilityService } from '../../../../../core/services/utility/utils';
import { ExceptionRecord, Parameters } from '../exception-management.component';
import { MatDialog } from '@angular/material/dialog';
import { ParameterService } from '../../../../../core/services/setting/parameter.service';
import { ValidationDialogService } from '../../../../../core/services/setting/validation-dialog.service';
import { RulesService } from '../../../../../core/services/setting/rules.service';
import { AuthService } from '../../../../../core/services/auth/auth.service';
import { RolesService } from '../../../../../core/services/setting/role.service';
import { ProductsService } from '../../../../../core/services/setting/products.service';
import { forkJoin } from 'rxjs';
import { AbstractControl, ValidationErrors } from '@angular/forms';

//type Token = { type: string; value: string };
export function limitRequiresProductValidator(
  control: AbstractControl
): ValidationErrors | null {
  const scope = control.value as string[] | null;

  if (!scope || scope.length === 0) return null;

  const hasLimit = scope.includes('Limit Amount');
  const hasProduct = scope.includes('Product Eligibility');

  if (hasLimit && !hasProduct) {
    return { limitWithoutProduct: true };
  }

  return null;
}
interface Token {
  type: string;
  value: string;

}
@Component({
  selector: 'app-exception-form',
  standalone: false,
  templateUrl: './exception-form.component.html',
  styleUrl: './exception-form.component.scss'
})

export class ExceptionFormComponent {
  @ViewChild('openParenthesisSelect') openParenthesisSelect: any;
  @ViewChild('parameterSelect') parameterSelect: any;
  @ViewChild('operatorSelect') operatorSelect: any;
  @ViewChild('factorSelect') factorSelect: any;
  @ViewChild('logicalOperatorSelect') logicalOperatorSelect: any;
  @ViewChild('closeParenthesisSelect') closeParenthesisSelect: any;
  @Output() onSubmit = new EventEmitter<any>();
  @Output() formClosed = new EventEmitter<any>();

  @Input() formData: ExceptionRecord = {
    exceptionManagementId: null,
      exceptionName: '',
      parameterID: null,
      isTemporary: false,
      scope: [],
      description: '',
      isActive: false,
      startDate: '',
      endDate: '',
      fixedPercentage: 0,
      variationPercentage: 0,
    amountType: '',
    productId: [],
    Products: [],
    expShown: ['']
  

  };
  
  SelectedProducts: [] = [];
  @Input() isEditMode: boolean = false;
  private _snackBar = inject(MatSnackBar);
  exceptionScopes: string[] = ['Limit Amount', 'Product Eligibility'];
  parametersList: Parameters[] = [];
  limitAmountEnabled: boolean = false;
  productEligibilityEnabled: boolean = false;
  parameters: any[] = [];
  isOpenParenthesis: boolean = false;
  isParameterName: boolean = false;
  isOperator: boolean = true;
  isFactor: boolean = true;
  isLogicalOperator: boolean = true;
  isCloseParenthesis: boolean = true;
  isInsertNewRecord: boolean = false;
  selectedParameterId: number = 0;
  isParameterSelected: boolean = false;
  selectedParameter: string = '';
  selectedFactor: string = '';
  selectedOperator: string = '';
  formVisible: boolean = false;
  operatorsAry: { conditionId: number; conditionValue: string }[] = [];
  expression = '';
  expShown = '';
  changePopup: boolean = false;
  expressionForPayload = '';
  exceptionForm!: FormGroup;
  updatedIndexId: number = 0;
  sequence: { type: string; value: any; id?: number }[] = [];
  sequenceExp: { type: string; value: any }[] = [];
  factors: any[] = [];
  factor: any[] = [];
  enteredFactorValue: string = '';
  factorsList: any[] = [];
  openParanthesis: any = ['('];
  closeParanthesis: any = [')'];
  logicalOperator: any = ['And', 'Or'];
  isScopeSelected: boolean = false;
  formDataNew = {
    fullOverride: false,
    scopeCheck: false
  };
  // limitAmountType: 'Fixed' | 'Variation' = 'Fixed';
  
  amountType: string = '';
  percentageValue: number = 0;
  eruleId: number = 0;
  deleteKeyForMultiple: string = '';
  productsList: any[] = [];
  productMap = new Map<number, string>();
  constructor(
    private fb: FormBuilder,
    private ruleService: RulesService,
    private factorService: FactorsService,
    private authService: AuthService,
    private parameterService: ParameterService,
    private validationDialogService: ValidationDialogService,
    private dialog: MatDialog,
    private rolesService: RolesService,
    private productsService: ProductsService,
    private exceptionsService: ExceptionManagementService,
    private utilityService: UtilityService
  ) {
    this.exceptionForm = this.fb.group({
      exceptionManagementId:[],
      exceptionName: ['', Validators.required],
      description: [''],
      scope: [[], [Validators.required, limitRequiresProductValidator]
],
      isTemporary: [false],
      startDate: [{ value: '', disabled: true }, Validators.required],
      endDate: [{ value: '', disabled: true }, Validators.required],
      percentageValue: [0, [Validators.min(0), Validators.max(100), Validators.required]],
      expShown: [''],
      factorName: [{ value: '', disabled: true }, Validators.required],
      productId: [[], Validators.required],
      amountType: ['', Validators.required],
      Products: [[]],
      isActive:[false]
    });
    
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['formData'] && this.formData && this.isEditMode) {
      this.patchFormWithFormData();
      //console.log(this.isEditMode)
    } else  {
      this.changePopup = true;
      this.resetFormData();
      // Clear for add
    }


  }
  //confirmDiscard(): void {
  //  this.changePopup = false;
  //  this.isEditMode = false;
  //  this.resetFormData();
  // // move to add mode
  //}

  //cancelDiscard(): void {
  //  this.changePopup = false; // Stay on edit form
  //  this.isEditMode = false;

  //}

  resetFormData() {
    this.isScopeSelected=false
   this. limitAmountEnabled = false;
   this. productEligibilityEnabled = false;
    this.exceptionForm.reset({
      exceptionManagementId:0,
      exceptionName: '',
      description: '',
      scope: [],
      isTemporary: false,
      startDate: '',
      endDate: '',
      percentageValue: 0,
      amountType: '',
      productId: [],
      expShown: '',
      parameterID: null,
      isActive:false
    });

    this.amountType = '';
 

    // disable start/end date
    this.exceptionForm.get('startDate')?.disable();
    this.exceptionForm.get('endDate')?.disable();

    this.resetExpression(); // if you have custom expression logic
  }
  get scopeControl(): FormControl {
    return this.exceptionForm.get('scope') as FormControl;
  }
  patchFormWithFormData() {
    // Determine amountType based on which percentage is set
    this.amountType = this.formData.fixedPercentage > 0 ? 'Fixed' :
      this.formData.variationPercentage > 0 ? 'Variation' : '';
    // Handle expShown safely (convert array to string if needed)
    const expShownValue = Array.isArray(this.formData.expShown)
      ? this.formData.expShown.join(' ')
      : this.formData.expShown || '';
    // Calculate the correct percentage value
    const percentageValue = this.amountType === 'Fixed' ?
      this.formData.fixedPercentage :
      this.formData.variationPercentage;

    this.exceptionForm.patchValue({
      exceptionManagementId: this.formData.exceptionManagementId,
      exceptionName: this.formData.exceptionName,
      description: this.formData.description,
      scope: this.formData.scope,
      isTemporary: this.formData.isTemporary,
      startDate: this.formData.startDate,
      endDate: this.formData.endDate,
      percentageValue: percentageValue,
      productId: this.formData.productId || [],
      expShown: expShownValue,
      amountType: this.amountType,
      isActive: this.formData.isActive
    })
    this.expressionForPayload = this.formData['expression'] || '';
    if (this.isEditMode && expShownValue) {
      this.parseExistingExpression(expShownValue);
    }
    if (this.formData.isTemporary) {
      this.exceptionForm.get('startDate')?.enable();
      this.exceptionForm.get('endDate')?.enable();
    }

    this.onScopeChange();
  }
  
  ngOnInit() {
    if (!this.formData) {
      this.formData = {
        exceptionName: '',
        parameterID: null,
        isTemporary: false,
        scope: [],
        description: '',
        isActive: false,
        startDate: '',
        endDate: '',
        fixedPercentage: 0,
        variationPercentage: 0,
        amountType: '',
        productId: [],
        Products: [],
        expShown: [''],
        exceptionManagementId: 0,
      };
    } else {
      // Initialize amountType based on which percentage is set
      this.amountType = this.formData.fixedPercentage > 0 ? 'Fixed' :
        this.formData.variationPercentage > 0 ? 'Variation' : '';

      // Calculate the correct percentage value
      const percentageValue = this.amountType === 'Fixed' ?
        this.formData.fixedPercentage :
        this.formData.variationPercentage;

      this.exceptionForm.patchValue({
        exceptionManagementId: this.formData.exceptionManagementId,
        productId: this.formData.productId || [],
        exceptionName: this.formData.exceptionName,
        description: this.formData.description,
        scope: this.formData.scope,
        isTemporary: this.formData.isTemporary,
        startDate: this.formData.startDate,
        endDate: this.formData.endDate,
        amountType: this.amountType,
        percentageValue: percentageValue,
        parameterID: this.formData.parameterID,
        expShown: this.formData.expShown || '',
        isActive: this.formData.isActive
      });

      if (this.formData.isTemporary) {
        this.exceptionForm.get('startDate')?.enable();
        this.exceptionForm.get('endDate')?.enable();
      }
      this.onScopeChange();
      this.fetchAllFactorsList();
      this.fetchConditions()
    }

    forkJoin({
      parameters: this.parameterService.getParameters(),
      factors: this.factorService.getFactorsList(),
      conditions: this.parameterService.getConditions(),
      products: this.productsService.getInfoListName(),
      allFactors: this.factorService.getFactorsList() // For fetchAllFactorsList equivalent
    }).subscribe({
      next: (results) => {
        if (results.parameters.isSuccess) {
          this.parametersList = results.parameters.data;
          this.parameters = results.parameters.data;
        }

        if (results.factors.isSuccess) {
          this.factor = results.factors.data;
        }



        if (results.products.isSuccess) {
          this.productsList = results.products.data;
          results.products.data.forEach((product: { productId: number; productName: string }) => {
            this.productMap.set(product.productId, product.productName);
          });
        }

        if (results.allFactors.isSuccess) {
          this.factorsList = results.allFactors.data;
        }

        if (this.isEditMode && this.formData?.expShown && this.parameters.length > 0) {
          const expShownValue = Array.isArray(this.formData.expShown)
            ? this.formData.expShown.join(' ')
            : this.formData.expShown || '';
          this.parseExistingExpression(expShownValue);
        }

        if (this.isEditMode) {
          this.onScopeChange();
        }
      },
      error: (error) => {
        console.error('Error loading initial data:', error);
        this._snackBar.open('Failed to load required data. Please try again.', 'Close', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 5000,
        });
      },
      complete: () => {
        this.deleteKeyForMultiple = 'eruleId';
      }
    });
  }
  fetchAllFactorsList() {
    this.factorService.getFactorsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factorsList = response.data;
        }
      },
      error: (error) => {
        console.log("rules :", error);
      }
    });
  }

  toggleCheckbox() {
    if (this.formDataNew.fullOverride) {
      this.formDataNew.scopeCheck = false;
    } else if (this.formDataNew.scopeCheck) {
      this.formDataNew.fullOverride = false;
    }
  }
  fetchAllProducts() {
    this.productsService.getInfoListName().subscribe({
      next: (productResponse) => {
        if (productResponse.isSuccess) {
          this.productsList = productResponse.data;
          productResponse.data.forEach((product: { productId: number; productName: string }) => {
            this.productMap.set(product.productId, product.productName);
          });
          //if (this.isEditMode && this.formData?.productId) {
          //  this.exceptionForm.patchValue({
          //    ProductIds: this.formData.productId
          //  });
          //}
        }
      }  })
  


  }
  fetchConditions(): void {
    this.parameterService.getConditions().subscribe({
      next: (response) => {
        this.operatorsAry = response;
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
      },
    });
  }

  updateExpression(): void {
    this.expShown = this.sequence.map(item => item.type === 'parameter' ? item.value : item.value).join(' ');
    if (this.enteredFactorValue) {
      this.expShown += ` ${this.enteredFactorValue}`;
    }
    this.exceptionForm.get('expShown')?.setValue(this.expShown);

    this.expressionForPayload = this.sequence.map(item => item.type === 'parameter' ? item.id : item.value).join(' ');
    if (this.enteredFactorValue) {
      this.expressionForPayload += ` ${this.enteredFactorValue}`;
    }
  }

  fetchFactorsList() {
    this.factorService.getFactorsByParameterId(this.selectedParameterId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factors = response.data;
        }
      },
      error: (error) => {
        console.log("rules :", error);
      }
    });
  }

  fetchFactorsLists() {
    this.factorService.getFactorsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factor = response.data;
        }
      },
      error: (error) => {
        console.log("rules :", error);
      }
    });
  }

  onSelect(type: string, event: any): void {
    const value = event.target.value;
    if (!value) return;

    if (this.sequence.length > 0 && this.sequence[this.sequence.length - 1].type === type) {
      if (type === 'parameter') {
        const selectedParam = this.parameters.find(param => param.parameterName === value);
        this.sequence[this.sequence.length - 1] = { type, value, id: selectedParam?.parameterId };
      } else {
        this.sequence[this.sequence.length - 1].value = value;
      }
    } else {
      if (type === 'parameter') {
        const selectedParam = this.parameters.find(param => param.parameterName === value);
        this.sequence.push({ type, value, id: selectedParam?.parameterId });
      } else {
        this.sequence.push({ type, value });
      }
    }

    if (type === 'parameter') {
      this.selectedParameter = value;
      this.selectedFactor = '';
      this.selectedOperator = '';
      this.isParameterSelected = true;
      const selectedParam = this.parameters.find(param => param.parameterName === value);
      if (selectedParam) {
        this.selectedParameterId = selectedParam.parameterId;
      }
      this.factorService.getFactorsByParameterId(this.selectedParameterId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            if (response.data && response.data.length > 0) {
              this.factors = response.data;
              this.isOperator = false;
              this.isOpenParenthesis = true;
              this.isParameterName = true;
              this.exceptionForm.get('factorName')?.disable();
            } else {
              this.factors = [];
            }
          }
        },
        error: (error) => {
          console.log("rules :", error);
        }
      });
    } else if (type === 'factor') {
      const factor = this.factors.find(f => f.value1 === value || (this.isRangeOperator() && `${f.value1} - ${f.value2}` === value));
      this.selectedFactor = factor ? (this.isRangeOperator() ? `${factor.value1} - ${factor.value2}` : factor.value1) : value;
      this.selectedParameter = '';
      this.selectedOperator = '';
      this.isFactor = true;
      this.isLogicalOperator = false;
      this.isCloseParenthesis = false;
      this.exceptionForm.get('factorName')?.disable();
      if (this.exceptionForm.get('factorName')?.value) {
        const factorValue = this.exceptionForm.get('factorName')?.value;
        this.sequence.push({ type, value: factorValue });
        this.updateExpression();
      }
    } else if (type === 'operator') {
      this.selectedOperator = value;
      this.isFactor = false;
      this.isOperator = true;
      this.exceptionForm.get('factorName')?.enable();
    } else if (type === 'logicalOperator') {
      this.isOpenParenthesis = false;
      this.isParameterName = false;
      this.isLogicalOperator = true;
      this.isCloseParenthesis = true;
      this.exceptionForm.get('factorName')?.disable();
    }
    event.target.value = '';
    this.updateExpression();
  }






//private tokenize(expression: string): Token[] {
//  // Regex splits into quoted strings, AND/OR, comparison ops, parentheses, or words/numbers
//  const regex = /"[^"]+"|\b(?:AND|OR)\b|>=|<=|>|<|=|\(|\)|[\p{L}\p{N}]+(?:[\p{Zs}]+[\p{L}\p{N}]+)*/gu;

//  const tokens: Token[] = [];
//  let match: RegExpExecArray | null;

//  while ((match = regex.exec(expression))) {
//    const value = match[0].trim();

//    if (!value) continue;

//    if (value === "(") {
//      tokens.push({ type: "openParenthesis", value });
//    } else if (value === ")") {
//      tokens.push({ type: "closeParenthesis", value });
//    } else if (["=", ">", "<", ">=", "<="].includes(value)) {
//      tokens.push({ type: "operator", value });
//    } else if (value === "AND" || value === "OR") {
//      tokens.push({ type: "logicalOperator", value });
//    } else if (/^\d+(\.\d+)?$/.test(value)) {
//      tokens.push({ type: "factor", value }); // numeric constant
//    } else {
//      // Heuristic: if it comes before an operator → it's a parameter
//      // if it comes after an operator → it's a factor
//      const prev = tokens[tokens.length - 1];
//      if (prev && prev.type === "operator") {
//        tokens.push({ type: "factor", value });
//      } else {
//        tokens.push({ type: "parameter", value });
//      }
//    }
//  }

//  return tokens;
//  }
  parseExistingExpression(expression: string): void {
    this.sequence = [];
    if (!expression?.trim()) return;

    const tokens: any[] = [];
    const words = expression
      .replace(/\(/g, " ( ")
      .replace(/\)/g, " ) ")
      .split(/\s+/)
      .filter(w => w.trim().length > 0); // remove empties

    let i = 0;
    while (i < words.length) {
      let word = words[i];

      if (word === "(") {
        tokens.push({ type: "openParenthesis", value: "(" });
        i++;
        continue;
      }

      if (word === ")") {
        tokens.push({ type: "closeParenthesis", value: ")" });
        i++;
        continue;
      }
      const threeWord = (words[i] + " " + (words[i + 1] || "") + " " + (words[i + 2] || "")).trim();
      if (/^(Not In List)$/i.test(threeWord)) {
        tokens.push({ type: "operator", value: "Not In List" });
        i += 3;
        continue;
      }

      // ✅ Handle 2-word operator next
      const twoWord = (words[i] + " " + (words[i + 1] || "")).trim();
      if (/^(In List)$/i.test(twoWord)) {
        tokens.push({ type: "operator", value: "In List" });
        i += 2;
        continue;
      }

      // ✅ Handle single-word operators (Range, =, >, <, etc.)
      if (/^(=|>=|<=|>|<|Range)$/i.test(word)) {
        tokens.push({ type: "operator", value: word });
        i++;
        continue;
      }

      if (/^(AND|OR)$/i.test(word)) {
        tokens.push({ type: "logicalOperator", value: word.toUpperCase() });
        i++;
        continue;
      }

      let matchedParam = null;
      let paramWords: string[] = [];
      for (let j = i; j < words.length; j++) {
        paramWords.push(words[j]);
        const joined = paramWords.join(" ").toLowerCase();
        matchedParam = this.parameters.find(
          p => p.parameterName.toLowerCase() === joined
        );
        if (matchedParam) {
          tokens.push({
            type: "parameter",
            value: matchedParam.parameterName,
            id: matchedParam.parameterId
          });
          i = j + 1;
          break;
        }
        if (/^(=|>=|<=|>|<|AND|OR|In List|Range|Not In List|\)|\()$/i.test(words[j])) break;
      }

      if (matchedParam) continue;

      let factorWords: string[] = [];
      while (
        i < words.length &&
        !/^(=|>=|<=|>|<|AND|OR|In List|Range|Not In List|\)|\()$/i.test(words[i])
      ) {
        factorWords.push(words[i]);
        i++;
      }

      if (factorWords.length > 0) {
        const rawFactor = factorWords.join(" ");
        const parts = rawFactor.split(/\b(AND|OR)\b/i).map(p => p.trim()).filter(Boolean);

        for (let pIndex = 0; pIndex < parts.length; pIndex++) {
          const part = parts[pIndex];
          if (/^(AND|OR)$/i.test(part)) {
            tokens.push({ type: "logicalOperator", value: part.toUpperCase() });
          } else {
            tokens.push({ type: "factor", value: part });
          }
        }
      }
    }

    for (let k = 0; k < tokens.length; k++) {
      if (tokens[k].type === "operator") {
        if (tokens[k - 1] && tokens[k - 1].type === "factor") {
       
          const param = this.parameters.find(
            p => p.parameterName.toLowerCase().trim() === tokens[k - 1].value.toLowerCase().trim()
          );
          tokens[k - 1].type = "parameter";
          if (param) tokens[k - 1].id = param.parameterId;
        }

        if (tokens[k + 1] && tokens[k + 1].type === "parameter") {
          tokens[k + 1].type = "factor";
          delete tokens[k + 1].id;
        }
      }
    }

    this.sequence = tokens;
    console.log("Final sequence:", this.sequence);
  }
  isRangeOperator(): boolean {
    return this.selectedOperator === 'Range';
  }

  removeLastItem(): void {
    if (this.sequence.length >= 0) {
      console.log(this.sequence);
      this.sequence.pop();
      this.updateExpression();
      if (this.sequence.length > 0) {
        const lastItem = this.sequence[this.sequence.length - 1];
        if (lastItem.type === 'parameter') {
          this.isParameterName = true;
          this.isOpenParenthesis = true;
          this.isOperator = false;
          this.isFactor = true;
          this.isCloseParenthesis = true;
          this.isLogicalOperator = true;
          this.selectedParameter = lastItem.value;
          this.selectedFactor = '';
          this.selectedOperator = '';
          this.exceptionForm.get('factorName')?.disable();
        } else if (lastItem.type === 'factor') {
          this.isLogicalOperator = false;
          this.isCloseParenthesis = false;
          this.isOpenParenthesis = true;
          this.isOperator = true;
          this.isFactor = true;
          this.isParameterName = true;
          this.selectedFactor = lastItem.value;
          this.selectedParameter = '';
          this.selectedOperator = '';
          this.exceptionForm.get('factorName')?.disable();
        } else if (lastItem.type === 'operator') {
          let lastMatchingParameterId = null;
          for (let i = this.parameters.length - 1; i >= 0; i--) {
            const parameter = this.parameters[i];
            if (this.sequence[this.sequence.length - 2]?.value.includes(parameter.parameterName)) {
              lastMatchingParameterId = parameter.parameterId;
              break;
            }
          }
          if (lastMatchingParameterId) {
            this.factorService.getFactorsByParameterId(lastMatchingParameterId).subscribe({
              next: (response) => {
                if (response.isSuccess) {
                  if (response.data && response.data.length > 0) {
                    this.factors = response.data;
                    this.isParameterName = true;
                  } else {
                    this.factors = [];
                  }
                }
              },
              error: (error) => {
                console.log("rules :", error);
              }
            });
          }
          this.isOperator = true;
          this.isFactor = false;
          this.exceptionForm.get('factorName')?.enable();
          this.isOpenParenthesis = true;
          this.isParameterName = true;
          this.isCloseParenthesis = true;
          this.isLogicalOperator = true;
          this.selectedOperator = lastItem.value;
        } else if (lastItem.type === 'openParenthesis') {
          this.isOpenParenthesis = false;
          this.isParameterName = false;
          this.isFactor = true;
          this.isOperator = true;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
          this.exceptionForm.get('factorName')?.disable();
        } else if (lastItem.type === 'logicalOperator') {
          this.isOpenParenthesis = false;
          this.isParameterName = false;
          this.isFactor = true;
          this.isOperator = true;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
          this.exceptionForm.get('factorName')?.disable();
        }
      } else {
        this.selectedParameter = '';
        this.selectedFactor = '';
        this.selectedOperator = '';
        this.exceptionForm.get('factorName')?.disable();
      }
    }
  }

  onChangePercentage(event: any) {
    const value = event.target.value;
    this.percentageValue = value;
    this.exceptionForm.get('percentageValue')?.setValue(value);
  }

  onChangeAmountType() {
    this.percentageValue = 0;
    this.exceptionForm.get('percentageValue')?.setValue(0);
    this.amountType = this.exceptionForm.get('amountType')?.value;


  }
  submitted: boolean = false;
  async addRecord(event: Event) {

    event.preventDefault();
    this.submitted = true;
  this.exceptionForm.markAllAsTouched();
    const isTemporary = this.exceptionForm.get('isTemporary')?.value;
    const startDate = this.exceptionForm.get('startDate');
    const endDate = this.exceptionForm.get('endDate');

  
    if (isTemporary) {
      startDate?.markAsTouched();
      endDate?.markAsTouched();

      if (!startDate?.value || !endDate?.value) {
        this._snackBar.open('Please select Start Date and End Date.', 'Close', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 4000
        });
        return;
      }
    }
    if (this.exceptionForm.invalid) {
      Object.keys(this.exceptionForm.controls).forEach(controlName => {
        this.exceptionForm.get(controlName)?.markAsTouched();
      });
      this._snackBar.open('Please fill in the required fields.', 'Close', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 5000,
      });
      return;
    }

    // Get the current form values
    const formValues = this.exceptionForm.value;
    var amountType = formValues.amountType;
    const percentageValue = formValues.percentageValue || 0;

    // Set the correct percentage values based on amountType
    var fixedPercentage = amountType === 'Fixed' ? percentageValue : 0;
    var variationPercentage = amountType === 'Variation' ? percentageValue : 0;

    const value = formValues.expShown;
    const expShown = Array.isArray(value) ? value.join(' ') : value || '';

    if (!formValues.scope?.includes('Product Eligibility')) {
      formValues.productId = [];
    }
    if (!formValues.scope?.includes('Limit Amount')) {
      console.log("Limit Amount  not contains")
      fixedPercentage = 0;
      variationPercentage = 0;
      amountType = '';
    }
    const formDataToSubmit = {
      ...formValues,
      expShown: expShown,
      exceptionManagementId: this.formData.exceptionManagementId || 0,
      //isActive: true,
      scope: formValues.scope?.join(',') || '',
      expression: this.expressionForPayload || '',
      fixedPercentage: fixedPercentage,
      variationPercentage: variationPercentage,
      productsList: [],
      amountType: amountType // Make sure to include amountType
    };
    delete formDataToSubmit.parameterID;
    
    const serviceCall = this.isEditMode
      ? this.exceptionsService.updateException(formDataToSubmit)
      : this.exceptionsService.addException(formDataToSubmit);

    serviceCall.subscribe({
      next: (response) => {
        this.fetchParametersList();
        this._snackBar.open(response.message, 'Okay', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.onSubmit.emit();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
    });
  }

  onScopeChange() {
    this.submitted = true;

   var  scopeValues = this.exceptionForm.get('scope')?.value || [];
    const scopeControl = this.exceptionForm.get('scope');
 
    scopeControl?.updateValueAndValidity({ emitEvent: false });

  
    this.limitAmountEnabled = scopeValues.includes('Limit Amount');
    this.productEligibilityEnabled = scopeValues.includes('Product Eligibility');
    this.isScopeSelected = scopeValues.length > 0;
    scopeControl?.markAsTouched();
    scopeControl?.updateValueAndValidity();
    // Handle Limit Amount validations
    const amountType = this.exceptionForm.get('amountType');
    const percentageValue = this.exceptionForm.get('percentageValue');
    if (this.limitAmountEnabled) {
      amountType?.setValidators([Validators.required]);
      percentageValue?.setValidators([Validators.min(0),
        Validators.max(100),
        Validators.required]);
    } else {
      amountType?.clearValidators();
      percentageValue?.clearValidators();
    }
    amountType
    ?.updateValueAndValidity();
    percentageValue?.updateValueAndValidity();

    // Handle Product Eligibility validations
    const productId = this.exceptionForm.get('productId');
    if (this.productEligibilityEnabled) {
      productId?.setValidators([Validators.required]);
    } else {
      productId?.clearValidators();
    }
    productId?.updateValueAndValidity();
  }

  onTemporaryExceptionChange() {
    const isTemporary = this.exceptionForm.get('isTemporary')?.value;

    if (isTemporary) {
      this.exceptionForm.get('startDate')?.enable();
      this.exceptionForm.get('endDate')?.enable();
    } else {
      this.exceptionForm.get('startDate')?.disable();
      this.exceptionForm.get('endDate')?.disable();

      this.exceptionForm.get('startDate')?.setValue(null);
      this.exceptionForm.get('endDate')?.setValue(null);
    }
  }

  fetchParametersList(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.factorService.getParametersList().subscribe({
        next: (response) => {
          if (response && response.data) {
            this.parametersList = response.data;
            this.parameters = response.data;
          } else {
            console.error('Invalid parameters response:', response);
          }
          resolve();
        },
        error: (err) => reject(err),
      });
    });
  }

  cancelEvent() {
    this.isOpenParenthesis = false;
    this.isParameterName = false;
    this.isOperator = true;
    this.isFactor = true;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;
    this.isParameterSelected = false;
    this.selectedParameterId = 0;
    this.formVisible = false;
    this.resetExpression();
    this.isScopeSelected = false;
    this.exceptionForm.reset({
      exceptionName: '',
      description: '',
      scope: [],
      isTemporary: false,
      startDate: '',
      endDate: '',
      percentageValue: 0,
      expShown: '',
      factorName: ''
    });
    this.exceptionForm.get('startDate')?.disable();
    this.exceptionForm.get('endDate')?.disable();
  }

  resetExpression(): void {
    this.isOpenParenthesis = false;
    this.isParameterName = false;
    this.isOperator = true;
    this.isFactor = true;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;
    this.isParameterSelected = false;
    this.selectedParameterId = 0;
    this.exceptionForm.get('factorName')?.disable();
    this.exceptionForm.get('factorName')?.reset();
    this.sequence = [];
    this.exceptionForm.get('expShown')?.setValue('');
    this.expShown = '';
    this.expressionForPayload = '';
    this.selectedParameter = '';
    this.selectedFactor = '';
    this.selectedOperator = '';
    this.openParenthesisSelect.nativeElement.selectedIndex = 0;
    this.parameterSelect.nativeElement.selectedIndex = 0;
    this.operatorSelect.nativeElement.selectedIndex = 0;
    this.factorSelect.nativeElement.selectedIndex = 0;
    this.logicalOperatorSelect.nativeElement.selectedIndex = 0;
    this.closeParenthesisSelect.nativeElement.selectedIndex = 0;
  }

  closeForm() {
    this.formClosed.emit();
    this.formData = {
      exceptionManagementId: 0,
      exceptionName: '',
      parameterID: 0,
      isTemporary: false,
      scope: [],
      description: '',
      isActive: true,
      startDate: '',
      endDate: '',
      fixedPercentage: 0,
      variationPercentage: 0,
      productId: [],
      amountType: '',
      Products: [],
      expShown: [''],
    };
    this.exceptionForm.reset();
    this.isScopeSelected = false;
    this.exceptionForm.get('startDate')?.disable();
    this.exceptionForm.get('endDate')?.disable();
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  openValidatorDialogAdd(expShown: any): Promise<boolean> {
    return new Promise((resolve) => {
      this.isParameterSelected = false;
      this.selectedParameterId = 0;

      const openParenthesisCount = (this.exceptionForm.value.expShown.match(/\(/g) || []).length;
      const closeParenthesisCount = (this.exceptionForm.value.expShown.match(/\)/g) || []).length;

      if (openParenthesisCount !== closeParenthesisCount) {
        const msg = openParenthesisCount > closeParenthesisCount
          ? 'Please close the parentheses first.'
          : 'Missing Opening parentheses.';
        this._snackBar.open(msg, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
        return resolve(false);
      }

      this.sequenceExp = this.sequence.map(item => {
        if (item.type === "parameter") {
          let matchedParam = this.parameters.find(p => p.parameterName === item.value);
          if (matchedParam) {
            return { ...item, value: matchedParam.parameterId };
          }
        }
        if (item.type === "factor") {
          let matchedFactor = this.factors.find(f => f.value1 === item.value || (this.isRangeOperator() && `${f.value1} - ${f.value2}` === item.value));
          if (matchedFactor) {
            return { ...item, value: this.isRangeOperator() ? `${matchedFactor.value1} - ${matchedFactor.value2}` : matchedFactor.value1 };
          }
        }
        return item;
      });

      const expressionExp = this.sequenceExp.map(item => item.value).join(" ");
      if (!this.isInsertNewRecord) {
        expShown = this.exceptionForm.value.expShown;
      }

      this.parameterService.getParameters().subscribe({
        next: (resp) => {
          const dialogRef = this.validationDialogService.openValidationDialog({
            actionType: 'form',
            expshown: expShown,
            parameters: resp.data,
            expression: expressionExp,
            validationType: 'ERule',
            valideeId: this.eruleId
          });

          dialogRef.afterClosed().subscribe((result: boolean) => {
            resolve(result === true);  // ✅ Only continue if Validate button clicked
          });
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          });
          resolve(false);
        }
      });
    });
  }
}
