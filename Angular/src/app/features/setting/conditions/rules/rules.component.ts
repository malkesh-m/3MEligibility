import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { RulesService } from '../../../../core/services/setting/rules.service';
import { TableComponent } from '../../../../core/components/table/table.component';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FactorsService } from '../../../../core/services/setting/factors.service';
import { ParameterService } from '../../../../core/services/setting/parameter.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { DeleteDialogComponent } from '../../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { UtilityService } from '../../../../core/services/utility/utils';
import { AuthService } from '../../../../core/services/auth/auth.service';
//import { ValidatorDialogComponent } from '../../../../core/components/validator-dialog/validator-dialog.component';
import { ValidationDialogService } from '../../../../core/services/setting/validation-dialog.service';
import { RolesService } from '../../../../core/services/setting/role.service';
import { ExceptionManagementService } from '../../../../core/services/setting/exception-management.service';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function optionalValidToValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const validFrom = control.get('validFrom')?.value;
    const validTo = control.get('validTo')?.value;

    // If Valid To is empty or not selected, no error
    if (!validTo) {
      return null;
    }

    const fromDate = new Date(validFrom);
    const toDate = new Date(validTo);

    return fromDate <= toDate ? null : { invalidDateRange: true };
  };
}
export interface RulesRecord {
  eruleName: string;
  eruleDesc: string;
  Expression: string;
  eruleId: string;
  selected: boolean;
  Expshown: string
}

@Component({
  selector: 'app-rules',
  standalone: false,

  templateUrl: './rules.component.html',
  styleUrl: './rules.component.scss'
})
export class RulesComponent {
  @ViewChild('formSection') formSection!: ElementRef;

  @ViewChild('tableChild') tableChild!: TableComponent;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild('openParenthesisSelect') openParenthesisSelect: any;
  @ViewChild('parameterSelect') parameterSelect: any;
  @ViewChild('operatorSelect') operatorSelect: any;
  @ViewChild('factorSelect') factorSelect: any;
  @ViewChild('logicalOperatorSelect') logicalOperatorSelect: any;
  @ViewChild('closeParenthesisSelect') closeParenthesisSelect: any;

  eRulesListAry: any = [];
  rulesColumnsAry = ['select', 'Name', 'Description', 'Expression', 'Version', 'createdByDateTime', 'createdBy', 'updatedByDateTime', 'updatedBy', 'Actions']
  deleteKeyForMultiple: string = '';
  formVisible: boolean = false;
  isInsertNewRecord: boolean = false;
  searchTerm: string = '';
  menuVisible: boolean = false;
  openParanthesis: any = ['(']
  closeParanthesis: any = [')']
  logicalOperator: any = ['And', 'Or']
  expression = '';
  expshown = '';
  expressionForm!: FormGroup;
  updatedIndexId: number = 0;
  private _snackBar = inject(MatSnackBar);
  parameters: any[] = [];
  sequence: { type: string; value: any }[] = [];
  sequenceExp: { type: string; value: any }[] = [];
  factors: any[] = [];
  factor: any[] = [];
  factorsList: any[] = [];
  selectedParameter: string = '';
  selectedFactor: string = '';
  selectedOperator: string = '';
  selectedParameterId: number = 0;
  isParameterSelected: Boolean = false;
  isOpenParenthesis: boolean = false;
  isParameterName: boolean = false;
  isOperator: boolean = true;
  isFactor: boolean = true;
  isLogicalOperator: boolean = true;
  isCloseParenthesis: boolean = true;
  records: RulesRecord[] = [];
  dataSource = new MatTableDataSource<any>([]);
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  checkedSelectedId: string = '';
  paginatedRows: any[] = [];
  selection = new Set<any>();
  operatorsAry: { conditionId: number; conditionValue: string }[] = [];
  enteredFactorValue: string = '';  // Bind to input box for factor
  selectedFile: File | null = null;
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = "Loading data, please wait...";
  loggedInUser: any = null;
  createdBy: string = '';
  eruleId: number = 0;
  selectedRule: any = null;
  IfAddNewVersion: boolean = false;
  IEditRuleVersion: boolean = false;
  editselectedRule: boolean = false;
  EditEruleMaster: boolean = false;
  parentRules: any[] = [];
  exceptions: any[] = [
    { exceptionID: 17, exceptionName: "For Product" },
    { exceptionID: 18, exceptionName: "string" },
    { exceptionID: 19, exceptionName: "gdfg" },
    { exceptionID: 20, exceptionName: "gsdf" },
    { exceptionID: 21, exceptionName: "test" },
    { exceptionID: 22, exceptionName: "test Updated" },
    { exceptionID: 25, exceptionName: "test" },
    { exceptionID: 28, exceptionName: "age exception" },
    { exceptionID: 30, exceptionName: "Corporate Gurantee" },
    { exceptionID: 31, exceptionName: "Age Exception" }
  ];


  constructor(
    private fb: FormBuilder,
    private ruleService: RulesService,
    private factorService: FactorsService,
    private authService: AuthService,
    private parameterService: ParameterService,
    private validationDialogService: ValidationDialogService,
    private dialog: MatDialog,
    private utilityService: UtilityService,
    private rolesService: RolesService,
    private exceptionsService: ExceptionManagementService
  ) {

  }
  ngAfterViewInit(): void {
    // Assign paginator and sort AFTER the view initializes
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }
  ngOnInit(): void {
    this.loadParentRules();

    this.deleteKeyForMultiple = 'eruleId';
    this.refreshRules();    // this.fetchFactorsList();
    this.fetchFactorsLists();
    this.fetchParametersList();
    this.fetchAllFactorsList();
    // this.fetchAllExceptions();
    this.fetchConditions();

    this.expressionForm = this.fb.group({
      name: ['', [Validators.required]],
      factorName: [''],
      description: [''],
      expshown: ['', [Validators.required]],
      eruleMasterId: [],
      isExceptionRule: [false],
      exceptionId: new FormControl({ value: '', disabled: true }),
      parentRuleId: new FormControl({ value: '', disabled: true }),
      validFrom: ['', Validators.required],
      validTo: [],
      isActive: [false]
    }, { validators: optionalValidToValidator() });

    this.expressionForm.get('isExceptionRule')!.valueChanges.subscribe(isExceptionRule => {
      const exceptionControl = this.expressionForm.get('exceptionId');
      //const parentRuleControl = this.expressionForm.get('parentRuleId');

      if (isExceptionRule) {
        exceptionControl?.enable();
        // parentRuleControl?.enable();

        exceptionControl?.setValidators(Validators.required)
        // parentRuleControl?.setValidators(Validators.required)
      } else {
        exceptionControl?.setValue('')
        // parentRuleControl?.setValue('')
        exceptionControl?.disable();
        // parentRuleControl?.disable();
        exceptionControl?.clearValidators()
        // parentRuleControl?.clearValidators()
      }

      exceptionControl?.updateValueAndValidity();
      // parentRuleControl?.updateValueAndValidity();
    });
  }

  hasPermission(roleId: string): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  isRangeOperator(): boolean {
    // Check if current operator is "Range" (case insensitive)
    return this.selectedOperator?.toLowerCase() === 'range';
  }

  // Helper method to get display value for factors
  getFactorDisplayValue(factor: any): string {
    if (factor.value2 && factor.value2 !== factor.value1) {
      return `${factor.value1}-${factor.value2}`;
    }
    return factor.value1;
  }

  // Helper method to parse factor value (handles ranges)
  parseFactorValue(value: string): { value1: string, value2: string | null, isRange: boolean } {
    if (value.includes('-')) {
      const parts = value.split('-');
      return {
        value1: parts[0]?.trim(),
        value2: parts[1]?.trim(),
        isRange: true
      };
    }
    return {
      value1: value,
      value2: null,
      isRange: false
    };
  }

  // findMatchingFactors(expression: string, parameters: any[]): string[] {
  //   // Extract all numbers from the expression
  //   const extractedNumbers = expression.match(/\d+/g)?.map(Number) || [];

  //   // Find matching factor names
  //   const matchingFactors = parameters
  //     .filter(parameters =>
  //       extractedNumbers.includes(Number(parameters.parameterName)))
  //     .map(parameters => parameters.parameterId);

  //   return matchingFactors;
  // }

  getParameterIds(expression: string, parameterList: { parameterId: number; parameterName: string }[]): number[] {
    const matchedIds: number[] = [];

    for (const param of parameterList) {
      const regex = new RegExp(`\\b${param.parameterName}\\b`, 'g');
      if (regex.test(expression)) {
        matchedIds.push(param.parameterId);
      }
    }

    return matchedIds;
  }

  toggleColumn(column: string, afterColumn: string) {
    const index = this.rulesColumnsAry.indexOf(column);

    if (index > -1) {
      // Remove column if already visible
      this.rulesColumnsAry.splice(index, 1);
    } else {
      // Find the index of the afterColumn and insert right after it
      const afterIndex = this.rulesColumnsAry.indexOf(afterColumn);
      if (afterIndex !== -1) {
        this.rulesColumnsAry.splice(afterIndex + 1, 0, column);
      } else {
        this.rulesColumnsAry.push(column); // Default push if not found
      }
    }

    this.rulesColumnsAry = [...this.rulesColumnsAry]; // Ensure reactivity
  }
  addNewVersion(rule: any) {
    this.IfAddNewVersion = true;
    this.IEditRuleVersion = false;
    this.formVisible = true;

    const latestVersion = rule.versions && rule.versions.length > 0 ? rule.versions[0] : null;

    // Patch base rule information (from EruleMaster)
    this.expressionForm.patchValue({
      eruleId: 0, // Always 0 when adding new version
      name: rule.eruleName || '',
      eruleMasterId: Number(rule.eruleMasterId),
      description: rule.description || '',
      validFrom: latestVersion?.validFrom ? this.formatDateForInputUTC(latestVersion.validFrom) : null,
      validTo: latestVersion?.validTo ? this.formatDateForInputUTC(latestVersion.validTo) : null,
      expshown: latestVersion?.expShown || '',
      factorName: latestVersion?.factorName || '',
      expression: ''
    });

    // Scroll to the form
    setTimeout(() => {
      this.formSection?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'center' });

      const expShown = latestVersion?.expShown || '';
      this.sequence = this.parseExpressionToSequence(expShown);

      // --- Handle parameter matching
      let lastMatchingParameterId = null;
      for (let i = this.parameters.length - 1; i >= 0; i--) {
        const parameter = this.parameters[i];
        if (expShown.includes(parameter.parameterName)) {
          lastMatchingParameterId = parameter.parameterId;
          break;
        }
      }

      if (lastMatchingParameterId) {
        this.factorService.getFactorsByParameterId(lastMatchingParameterId).subscribe({
          next: (response) => {
            if (response.isSuccess) {
              this.factors = response.data || [];
              this.isParameterName = true;

              // Build expression with actual values
              const expressionExp = this.sequence.map(item => {
                if (item.type === "parameter") {
                  const matchedParam = this.parameters.find(p => p.parameterName === item.value);
                  return matchedParam ? matchedParam.parameterId : item.value;
                } else {
                  return item.value; // Use actual factor values directly
                }
              }).join(" ");

              this.expressionForm.patchValue({
                expression: expressionExp,
                expshown: expShown
              });

              const lastType = this.sequence[this.sequence.length - 1]?.type;
              this.updateControlStatesBasedOnLastItem(lastType);
            } else {
              this.factors = [];
            }
          },
          error: (err) => {
            console.error("Error fetching factors:", err);
            this.factors = [];
          }
        });
      } else {
        this.factors = [];
        this.sequence = this.parseExpressionToSequence(expShown);

        // Build expression with actual values
        const expressionExp = this.sequence.map(item => {
          if (item.type === "parameter") {
            const matchedParam = this.parameters.find(p => p.parameterName === item.value);
            return matchedParam ? matchedParam.parameterId : item.value;
          } else {
            return item.value; // Use actual factor values directly
          }
        }).join(" ");

        this.expressionForm.patchValue({
          expression: expressionExp,
          expshown: expShown
        });
      }

      if (!latestVersion) {
        this.resetExpression();
      }
    }, 0);
  }
  updateControlStatesBasedOnLastItem(lasteditvaluee: string | undefined) {
    if (lasteditvaluee === 'factor') {
      this.isLogicalOperator = false;
      this.isCloseParenthesis = false;
      this.isOpenParenthesis = true;
      this.isParameterName = true;
      this.isOperator = true;
      this.isFactor = true;
    } else if (lasteditvaluee === 'operator') {
      this.isFactor = false;
      this.isOperator = true;
      this.isOpenParenthesis = true;
      this.isCloseParenthesis = true;
      this.isParameterName = true;
      this.isLogicalOperator = true;
    } else if (lasteditvaluee === 'logicalOperator') {
      this.isFactor = true;
      this.isOperator = true;
      this.isOpenParenthesis = false;
      this.isCloseParenthesis = true;
      this.isParameterName = false;
      this.isLogicalOperator = true;
    } else if (lasteditvaluee === 'closeParenthesis') {
      this.isFactor = true;
      this.isOperator = true;
      this.isOpenParenthesis = true;
      this.isCloseParenthesis = false;
      this.isParameterName = true;
      this.isLogicalOperator = false;
    }
  }

  fetchConditions(): void {
    this.parameterService.getConditions().subscribe({
      next: (response) => {
        this.operatorsAry = response;
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }

  toggleSelection(rowIndex: number, rowItem: any) {
    if (this.selectedRows.has(rowIndex)) {
      this.selectedRows.delete(rowIndex);
      this.selectedRowsItem.delete(rowItem[this.checkedSelectedId])
    } else {
      this.selectedRows.add(rowIndex);
      this.selectedRowsItem.add(rowItem[this.checkedSelectedId])
    }
  }
  toggleSelectAll(event: MatCheckboxChange) {
    const checked = event.checked;

    // Get current page rows
    const currentPageData = this.dataSource._pageData(this.dataSource.filteredData);

    if (checked) {
      currentPageData.forEach((row, index) => {
        this.selectedRows.add(index); // optional, for tracking index
        this.selectedRowsItem.add(row[this.checkedSelectedId]);
      });
    } else {
      currentPageData.forEach((row) => {
        this.selectedRowsItem.delete(row[this.checkedSelectedId]); // delete the actual ID
      });

      // Optionally, clear selected row indices for current page
      currentPageData.forEach((_, index) => this.selectedRows.delete(index));
    }
  }


  fetchAllFactorsList() {
    this.factorService.getFactorsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factorsList = response.data;
        }
      }, error: (error) => {
        this._snackBar.open(error.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log("rules :", error)
      }
    })
  }
  private updateStatesAfterLogicalOperator(): void {
    // After logical operator, you can start a new condition
    this.isOpenParenthesis = false; // Can add open parenthesis for grouping
    this.isParameterName = false;   // Can add parameter to start new condition
    this.isOperator = true;         // Operators are available
    this.isFactor = true;           // Factors are available
    this.isLogicalOperator = true;  // Can add more logical operators
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
  }
  onSelect(type: string, data: any): void {
    const value = data.target.value;
    if (!value) return;

    // Handle close parenthesis specifically first
    if (type === 'closeParenthesis') {
      this.sequence.push({ type, value });
      this.updateExpression();
      this.updateStatesAfterCloseParenthesis();
      data.target.value = '';
      return;
    }

    // Handle logical operator
    if (type === 'logicalOperator') {
      this.sequence.push({ type, value });
      this.updateExpression();
      this.updateStatesAfterLogicalOperator();
      data.target.value = '';
      return;
    }

    // Handle open parenthesis
    if (type === 'openParenthesis') {
      this.sequence.push({ type, value });
      this.updateExpression();
      this.updateStatesAfterOpenParenthesis();
      data.target.value = '';
      return;
    }

    // For other types, update or add to sequence
    if (this.sequence.length > 0 && this.sequence[this.sequence.length - 1].type === type) {
      this.sequence[this.sequence.length - 1].value = value;
    } else {
      this.sequence.push({ type, value });
    }

    // Handle specific types
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
              this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
              this.isLogicalOperator = true; // Keep logical operators enabled
            } else {
              this.factors = [];
            }
          }
        },
        error: (error) => {
          console.log("rules :", error)
        }
      });
    } else if (type === 'factor') {
      const selectedFactor = this.factors.find(f => f.factorId == value);
      let selectedValue = value;

      if (selectedFactor) {
        if (selectedFactor.value2 && selectedFactor.value2 !== selectedFactor.value1) {
          selectedValue = `${selectedFactor.value1}-${selectedFactor.value2}`;
        } else {
          selectedValue = selectedFactor.value1;
        }
      }

      this.selectedFactor = selectedValue;
      this.selectedParameter = '';
      this.selectedOperator = '';
      this.isFactor = true;
      this.isLogicalOperator = false; // Disable logical operators immediately after factor
      this.isCloseParenthesis = this.shouldEnableCloseParenthesis();

      // Update the sequence with the actual value
      const lastItem = this.sequence[this.sequence.length - 1];
      if (lastItem && lastItem.type === 'factor') {
        lastItem.value = selectedValue;
      }

      this.expressionForm.controls['factorName'].disable();
      this.updateExpression();
    } else if (type === 'operator') {
      this.selectedOperator = value;
      this.isFactor = false;
      this.isOperator = true;
      this.expressionForm.controls['factorName'].enable();
      this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
      this.isLogicalOperator = true; // Enable logical operators after operator
    }

    data.target.value = '';
    this.updateExpression();
  }
  //fetchAllErules() {
  //  this.eRulesListAry = [];
  //  this.ruleService.getRulesList().subscribe({
  //    next: (response) => {
  //      if (response.isSuccess) {
  //        console.log(response)
  //        // Sorting the response data based on lastModifiedDateTime in descending order
  //        const sortedData = response.data.sort((a: any, b: any) =>
  //          new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
  //        );
  //        console.log(sortedData)
  //        console.log("sortedData")
  //        this.eRulesListAry = [...sortedData]

  //        //Parent rules are rules that are NOT exception rules
  //        this.parentRules = this.eRulesListAry.filter((rule: any) => !rule.isException)

  //        this.dataSource.data = sortedData.map((res: any) => ({
  //          Name: res.eruleName,
  //          Description: res.description,
  //          Expshown: res.expShown,
  //          Version: res.version,
  //          Expression: res.expression,
  //          eruleId: res.eruleId,
  //          createdBy: res.createdBy,
  //          createdByDateTime: res.createdByDateTime,
  //          updatedBy: res.updatedBy,
  //          updatedByDateTime: res.updatedByDateTime,
  //          isExceptionRule: res.isException,
  //          parentEruleId: Number(res.parentEruleId),
  //          exceptionId: Number(res.exceptionId),
  //          IsActive: res.isActive
  //        }))

  //        this.dataSource.paginator = this.paginator;
  //        this.dataSource.sort = this.sort;
  //        console.log(this.dataSource.data)
  //        console.log('this.dataSource.data')


  //      }
  //    }, error: (error) => {
  //      this.eRulesListAry = [];
  //      console.log("rules :", error)
  //    }
  //  })
  //}
  //fetchallrules new logic

  //fetchAllErules() {
  //  this.ruleService.getRulesList().subscribe({
  //    next: (response) => {
  //      if (response.isSuccess) {
  //        const allRules = response.data;
  //        console.log(response.data)



  //        const grouped = allRules.reduce((acc: any, rule: any) => {
  //          if (!acc[rule.eruleName]) acc[rule.eruleName] = [];
  //          acc[rule.eruleName].push(rule);
  //          return acc;
  //        }, {});

  //        // Only include if there's at least one active version
  //        this.parentRules = Object.entries(grouped).map(([eruleName, versions]: any) => {
  //          // Sort versions by date (latest first)
  //          const sortedVersions = versions.sort((a: any, b: any) =>
  //            (b.version) - (a.version)
  //          );

  //          // Use the latest version for the description
  //          const latest = sortedVersions[0];

  //          return {

  //            eruleName,
  //            description: latest.description,
  //            versions: sortedVersions,
  //            eruleMasterId: latest.eruleMasterId
  //      };

  //          });

  //        this.dataSource.data = this.parentRules;
  //        console.log(this.dataSource.data)
  //        console.log("rsponse.data")
  //      }
  //    },
  //    error: (err) => console.error("Failed to load rules:", err)
  //  });
  //}
  //newlogic 2


  fetchAllErules() {
    this.ruleService.getRulesList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          const allRules = response.data;

          // Group by rule name
          const grouped = allRules.reduce((acc: any, rule: any) => {
            if (!acc[rule.eruleName]) acc[rule.eruleName] = [];
            acc[rule.eruleName].push(rule);
            return acc;
          }, {});

          this.parentRules = Object.entries(grouped).map(([eruleName, versions]: any) => {
            const sortedVersions = versions
              .filter((v: any) => v.version !== null)
              .sort((a: any, b: any) => b.version - a.version);

            const latest = sortedVersions.length > 0 ? sortedVersions[0] : versions[0];

            return {
              eruleName,
              description: latest.description || '',
              versions: sortedVersions,
              eruleMasterId: latest.eruleMasterId,
              isActive: latest.isActive
            };
          });

          // Assign to dataSource and bind paginator & sort
          this.dataSource = new MatTableDataSource(this.parentRules);
          this.dataSource.paginator = this.paginator;
          this.dataSource.sort = this.sort;
        }
      },
      error: (err) => {
        console.error("Failed to load rules:", err)

        this._snackBar.open(err.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });      }
    });
  }
  fetchParametersList() {
    this.parameterService.getParameters().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.parameters = response.data;
        }
      }, error: (error) => {
        this._snackBar.open(error.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log("rules :", error)
      }
    })
  }

  fetchFactorsList() {
    this.factorService.getFactorsByParameterId(this.selectedParameterId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factors = response.data;
        }
      }, error: (error) => {
        this._snackBar.open(error.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log("rules :", error)
      }
    })
  }

  fetchFactorsLists() {
    this.factorService.getFactorsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factor = response.data;
        }
      }, error: (error) => {
        this._snackBar.open(error.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log("rules :", error)
      }
    })
  }

  fetchAllExceptions() {
    this.exceptionsService.getExceptionList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.exceptions = response.data.map((exc: any) => ({ ...exc, exceptionID: Number(exc.exceptionID) }));
        }
      }, error: (error) => {
        this._snackBar.open(error.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log("rules :", error)
      }
    })
  }
  editRuleMaster(payload: any) {
    this.IfAddNewVersion = false;
    this.IEditRuleVersion = false;
    this.EditEruleMaster = true

    this.formVisible = true;
    this.expressionForm.patchValue({

      name: payload.eruleName,
      description: payload.description,
      eruleMasterId: payload.eruleMasterId,
      isActive: payload.isActive

    })
    setTimeout(() => {
      this.formSection?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 0);
  }
  UpdateEruleMaster(payload: any) {
    const nameControl = this.expressionForm.get('name');
    if (nameControl?.invalid) {
      nameControl.markAsTouched();
      return;
    }
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    payload.eruleName = payload.name.trim()
    payload.updatedBy = this.loggedInUser.user.userName;

    payload.EruleId = payload.eruleMasterId
    this.ruleService.editNewERuleMaster(payload).subscribe({
      next: (response: { message: string; }) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.refreshRules();

        this.cancelEvent();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }
  AddNewEruleMaster(payload: any) {
    const nameControl = this.expressionForm.get('name');
    if (nameControl?.invalid) {
      nameControl.markAsTouched();
      return;
    }
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
 
    payload.eruleName = payload.name.trim()

    this.ruleService.addNewEruleMaster(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.refreshRules();
        this.cancelEvent();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  addNewRule(payload: any) {


    if (this.expressionForm.invalid) {
      this.expressionForm.markAllAsTouched(); // Shows all validation errors
      return;
    }
    this.IfAddNewVersion = false
    const formValue = this.expressionForm.value;
    payload.eruleMasterId = formValue.eruleMasterId
 

   payload.validFrom = formValue.validFrom ? this.toUTCDateString(formValue.validFrom) : null;
payload.validTo   = formValue.validTo   ? this.toUTCDateString(formValue.validTo)   : null;
    this.ruleService.addNewRule(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.refreshRules();
        this.cancelEvent();

      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  updateRule(payload: any) {
    if (this.expressionForm.invalid) {
      this.expressionForm.markAllAsTouched(); // Shows all validation errors
      return;
    }

    //payload.expression = this.expressionForm.value.expshown;
    payload.validFrom = this.expressionForm.value.validFrom? this.toUTCDateString(this.expressionForm.value.validFrom): null;
    payload.validTo = this.expressionForm.value.validTo? this.toUTCDateString(this.expressionForm.value.validTo): null;
    this.ruleService.updateExistingRule(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.refreshRules();
        this.cancelEvent();
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }

    })
  }

  deleteRule(id: number) {
    this.ruleService.deleteSingleRule(id).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.refreshRules();
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }
  deleteEruleMaster(Erule: any) {

    const confirmDelete = window.confirm(
      `Are you sure you want to delete the Rule: "${Erule.eruleName}"?`
    );
    if (confirmDelete) {

      this.ruleService.deleteEruleMaster(Erule.eruleMasterId).subscribe({
        next: (response) => {
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.refreshRules();
          this.selectedRules.clear();
          this.cancelEvent();
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.selectedRules.clear();
        }
      })
    }
  }
  selectedRules = new Set<any>();

  // Toggle rule selection
  toggleRuleSelection(rule: any): void {
    if (this.selectedRules.has(rule)) {
      this.selectedRules.delete(rule);
    } else {
      this.selectedRules.add(rule);
    }
  }

  // Check if rule is selected
  isRuleSelected(rule: any): boolean {
    return this.selectedRules.has(rule);
  }

  // Select all rules
  selectAllRules(): void {
    this.dataSource.data.forEach(rule => this.selectedRules.add(rule));
  }

  // Clear all selections
  clearAllSelections(): void {
    this.selectedRules.clear();
  }
  deleteCheckedMultipleRules() {
    if (this.selectedRules.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const selectedIds = Array.from(this.selectedRules).map(r => r.eruleMasterId); // adjust property

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.ruleService.deleteMultipleRules(selectedIds).subscribe({
          next: (response) => {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.refreshRules();
            this.selectedRules.clear(); // clear selection after delete
          },
          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      }
    });
  }


  //applyFilter(event: Event): void {
  //  console.log("applyfilter")
  //  const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
  //  this.dataSource.filter = filterValue;
  //  if (this.dataSource.paginator) {
  //    this.dataSource.paginator.firstPage();
  //  }
  //}
  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();

    const filteredRules = this.parentRules.filter(rule => {
      const matchesName = rule.eruleName?.toLowerCase().includes(filterValue);
      const matchesDescription = rule.description?.toLowerCase().includes(filterValue);
      const isActiveText = rule.isActive ? 'active' : 'inactive';
      const matchesIsActive = isActiveText.includes(filterValue.toLowerCase());

      const matchesVersion = rule.versions?.some((v: any) =>
        v.version?.toString().includes(filterValue) ||
        v.expShown?.toLowerCase().includes(filterValue) ||
        v.createdBy?.toLowerCase().includes(filterValue) ||
        v.updatedBy?.toLowerCase().includes(filterValue)
      );
      return matchesName || matchesDescription || matchesVersion || matchesIsActive;
    });

    this.paginatedRules = filteredRules;
    this.dataSource.data=filteredRules

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  formatDateForInput(date: string | Date): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = ('0' + (d.getMonth() + 1)).slice(-2);
    const day = ('0' + d.getDate()).slice(-2);
    return `${year}-${month}-${day}`;
  }
  //editRecord(event: any) {
  //  console.log(event)
  //  console.log('event')
  //  this.IfAddNewVersion = true;
  //  this.IEditRuleVersion = true;
  //  this.isInsertNewRecord = false;
  //  this.formVisible = true;
  //  this.expressionForm.patchValue({
  //    name: event.eruleName,
  //    description: event.description,
  //    factorName: '',
  //    expshown: event.expShown,
  //    expression:event.expression,
  //    isExceptionRule: event.isExceptionRule,
  //    exceptionId: event.exceptionId,
  //    parentRuleId: event.parentEruleId,
  //    validFrom: event.validFrom ? this.formatDateForInput(event.validFrom) : null,
  //    validTo: event.validTo ? this.formatDateForInput(event.validTo) : null

  //  })
  //  this.eruleId = event.eruleId;
  //  this.createdBy = event.createdBy;
  //  this.updatedIndexId = event.eruleId;
  //  let lastMatchingParameterId = null;

  //  for (let i = this.parameters.length - 1; i >= 0; i--) {
  //    const parameter = this.parameters[i];
  //    if (event.Expshown.includes(parameter.parameterName)) {
  //      lastMatchingParameterId = parameter.parameterId;
  //      break; // Exit the loop as soon as we find the last match
  //    }
  //  }

  //  if (lastMatchingParameterId) {
  //    this.factorService.getFactorsByParameterId(lastMatchingParameterId).subscribe({
  //      next: (response) => {
  //        if (response.isSuccess) {
  //          if (response.data && response.data.length > 0) {
  //            this.factors = response.data;
  //            this.isParameterName = true;
  //          } else {
  //            this.factors = [];

  //          }
  //        }
  //      }, error: (error) => {
  //        console.log("rules :", error)
  //      }
  //    })
  //  }
  //  this.sequence = this.parseExpressionToSequence(event.expshown);
  //  let lasteditvaluee = this.sequence[this.sequence.length - 1].type;
  //  if (lasteditvaluee === 'factor') {
  //    this.isLogicalOperator = false;
  //    this.isCloseParenthesis = false;
  //    this.isOpenParenthesis = true;
  //    this.isParameterName = true;
  //    this.isOperator = true;
  //    this.isFactor = true;
  //  } else if (lasteditvaluee === 'operator') {
  //    this.isFactor = false;
  //    this.isOperator = true;
  //    this.isOpenParenthesis = true;
  //    this.isCloseParenthesis = true;;
  //    this.isParameterName = true;
  //    this.isLogicalOperator = true;
  //  } else if (lasteditvaluee === 'logicalOperator') {
  //    this.isFactor = true;
  //    this.isOperator = true;
  //    this.isOpenParenthesis = false;
  //    this.isCloseParenthesis = true;;
  //    this.isParameterName = false;
  //    this.isLogicalOperator = true;
  //  } else if (lasteditvaluee === 'closeParenthesis') {
  //    this.isFactor = true;
  //    this.isOperator = true;
  //    this.isOpenParenthesis = true;
  //    this.isCloseParenthesis = false;;
  //    this.isParameterName = true;
  //    this.isLogicalOperator = false;

  //  }

  //}
  //new editrecord logic
  editRecord(event: any) {
    this.IfAddNewVersion = true;
    this.IEditRuleVersion = true;
    this.isInsertNewRecord = false;
    this.formVisible = true;

    // Patch form
    this.expressionForm.patchValue({
      name: event.eruleName,
      description: event.description,
      factorName: '',
      expshown: event.expShown,
      expression: event.expression,
      isActive: event.isActive,
      isExceptionRule: event.isExceptionRule,
      exceptionId: event.exceptionId,
      parentRuleId: event.parentEruleId,
      validFrom: event.validFrom ? this.formatDateForInputUTC(event.validFrom) : null,
      validTo: event.validTo ? this.formatDateForInputUTC(event.validTo) : null,
    });

    this.eruleId = event.eruleId;
    this.createdBy = event.createdBy;
    this.updatedIndexId = event.eruleId;

    let lastMatchingParameterId = null;
    for (let i = this.parameters.length - 1; i >= 0; i--) {
      const parameter = this.parameters[i];
      if (event.expShown.includes(parameter.parameterName)) {
        lastMatchingParameterId = parameter.parameterId;
        break;
      }
    }

    if (lastMatchingParameterId) {
      this.factorService.getFactorsByParameterId(lastMatchingParameterId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.factors = response.data || [];
            this.isParameterName = true;

            this.sequence = this.parseExpressionToSequence(event.expShown);

            // Build expression with actual values
            const expressionExp = this.sequence.map(item => {
              if (item.type === "parameter") {
                const matchedParam = this.parameters.find(p => p.parameterName === item.value);
                return matchedParam ? matchedParam.parameterId : item.value;
              } else {
                return item.value; // Use actual factor values directly
              }
            }).join(" ");

            this.expressionForm.patchValue({ expression: expressionExp });

            const lasteditvaluee = this.sequence[this.sequence.length - 1]?.type;
            if (lasteditvaluee === 'factor') {
              this.isLogicalOperator = false;
              this.isCloseParenthesis = false;
              this.isOpenParenthesis = true;
              this.isParameterName = true;
              this.isOperator = true;
              this.isFactor = true;
            } else if (lasteditvaluee === 'operator') {
              this.isFactor = false;
              this.isOperator = true;
              this.isOpenParenthesis = true;
              this.isCloseParenthesis = true;
              this.isParameterName = true;
              this.isLogicalOperator = true;
            } else if (lasteditvaluee === 'logicalOperator') {
              this.isFactor = true;
              this.isOperator = true;
              this.isOpenParenthesis = false;
              this.isCloseParenthesis = true;
              this.isParameterName = false;
              this.isLogicalOperator = true;
            } else if (lasteditvaluee === 'closeParenthesis') {
              this.isFactor = true;
              this.isOperator = true;
              this.isOpenParenthesis = true;
              this.isCloseParenthesis = false;
              this.isParameterName = true;
              this.isLogicalOperator = false;
            }
          } else {
            this.factors = [];
          }
        },
        error: (error) => {
          console.log("rules :", error);
        }
      });
    } else {
      this.factors = [];
      this.sequence = this.parseExpressionToSequence(event.expShown);

      // Build expression with actual values
      const expressionExp = this.sequence.map(item => {
        if (item.type === "parameter") {
          const matchedParam = this.parameters.find(p => p.parameterName === item.value);
          return matchedParam ? matchedParam.parameterId : item.value;
        } else {
          return item.value; // Use actual factor values directly
        }
      }).join(" ");

      this.expressionForm.patchValue({ expression: expressionExp });
    }

    setTimeout(() => {
      this.formSection?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 0);
  }

  deleteRecord(event: any) {
    if (confirm(`Are you sure you want to delete Rule: "${event.version}"?`)) {
      this.ruleService.deleteSingleRule(event.eruleId).subscribe({
        next: (response) => {
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.refreshRules();
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
private toUTCDateString(dateStr: string | null): string | null {
  if (!dateStr) return null;
  const [year, month, day] = dateStr.split('-').map(Number);
  const utcDate = new Date(Date.UTC(year, month - 1, day)); // construct in UTC
  return utcDate.toISOString(); // always "YYYY-MM-DDT00:00:00.000Z"
}
private formatDateForInputUTC(date: string | Date): string {
  const d = new Date(date);
  const year = d.getUTCFullYear();
  const month = ('0' + (d.getUTCMonth() + 1)).slice(-2);
  const day = ('0' + d.getUTCDate()).slice(-2);
  return `${year}-${month}-${day}`;
}
  tableRowAction(event: { action: string; data: any }): void {
    if (event.action === 'edit') {
      this.isInsertNewRecord = false;
      this.formVisible = true;
      console.log(event.data)
      this.expressionForm.setValue({
        name: event.data.Name,
        description: event.data.Description,
        expshown: event.data.Expshown,
      })
      this.updatedIndexId = event.data.eruleId;
      let lastMatchingParameterId = null;

      for (let i = this.parameters.length - 1; i >= 0; i--) {
        const parameter = this.parameters[i];
        if (event.data.expshown.includes(parameter.parameterName)) {
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
              this.refreshRules();
            }
          }, error: (error) => {
            this._snackBar.open(error.message, 'Close', {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
            });
            console.log("rules :", error)
          }
        })
      }
      this.sequence = this.parseExpressionToSequence(event.data.expShown);
      let lasteditvaluee = this.sequence[this.sequence.length - 1].type;
      if (lasteditvaluee === 'factor') {
        this.isLogicalOperator = false;
        this.isCloseParenthesis = false;
        this.isOpenParenthesis = true;
        this.isParameterName = true;
        this.isOperator = true;
        this.isFactor = true;
      } else if (lasteditvaluee === 'operator') {
        this.isFactor = false;
        this.isOperator = true;
        this.isOpenParenthesis = true;
        this.isCloseParenthesis = true;;
        this.isParameterName = true;
        this.isLogicalOperator = true;
      } else if (lasteditvaluee === 'logicalOperator') {
        this.isFactor = true;
        this.isOperator = true;
        this.isOpenParenthesis = false;
        this.isCloseParenthesis = true;;
        this.isParameterName = false;
        this.isLogicalOperator = true;
      } else if (lasteditvaluee === 'closeParenthesis') {
        this.isFactor = true;
        this.isOperator = true;
        this.isOpenParenthesis = true;
        this.isCloseParenthesis = false;;
        this.isParameterName = true;
        this.isLogicalOperator = false;
      }
    } else {
      this.deleteRule(event.data.eruleId)
    }
  }

  insertNewRecord() {
    this.EditEruleMaster = false;
    this.IfAddNewVersion = false
    this.IEditRuleVersion = false;
    this.formVisible = true;
    this.menuVisible = !this.menuVisible;
    this.isInsertNewRecord = true;
    this.expressionForm.controls['factorName'].reset();
    this.expressionForm.controls['factorName'].disable();
    this.expressionForm.reset();
  }

  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
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
  }

  copyRecord(record: any) {
    this.formVisible = true;
    this.menuVisible = !this.menuVisible;
    // Set the form to the selected record's data
    this.expressionForm.patchValue({
      name: record.Name + 1,
      description: record.Description,
      expshown: record.Expshown
    });

    this.sequence = this.parseExpressionToSequence(record.Expshown);
    let lasteditvaluee = this.sequence[this.sequence.length - 1].type;
    if (lasteditvaluee === 'factor') {
      this.isLogicalOperator = false;
      this.isCloseParenthesis = false;
      this.isOpenParenthesis = true;
      this.isParameterName = true;
      this.isOperator = true;
      this.isFactor = true;
    } else if (lasteditvaluee === 'operator') {
      this.isFactor = false;
      this.isOperator = true;
      this.isOpenParenthesis = true;
      this.isCloseParenthesis = true;;
      this.isParameterName = true;
      this.isLogicalOperator = true;
    } else if (lasteditvaluee === 'logicalOperator') {
      this.isFactor = true;
      this.isOperator = true;
      this.isOpenParenthesis = false;
      this.isCloseParenthesis = true;;
      this.isParameterName = false;
      this.isLogicalOperator = true;
    } else if (lasteditvaluee === 'closeParenthesis') {
      this.isFactor = true;
      this.isOperator = true;
      this.isOpenParenthesis = true;
      this.isCloseParenthesis = false;;
      this.isParameterName = true;
      this.isLogicalOperator = false;
    }

    // Update the form for new record insertion (reset state)
    this.isInsertNewRecord = true; // set flag for new record
    this.updatedIndexId = 0; // reset any previous record index
  }

  //submit() {


  //  this.authService.currentUser$.subscribe((user) => {
  //    this.loggedInUser = user;
  //  });

  //  if (!this.expressionForm.valid)
  //    this.expressionForm.markAllAsTouched();

  //  this.isParameterSelected = false;
  //  this.selectedParameterId = 0;

  //  const openParenthesisCount = (this.expressionForm.value.expshown.match(/\(/g) || []).length;
  //  const closeParenthesisCount = (this.expressionForm.value.expshown.match(/\)/g) || []).length;

  //  if (openParenthesisCount !== closeParenthesisCount) {
  //    if (openParenthesisCount > closeParenthesisCount) {
  //      this._snackBar.open('Please close the parentheses first.', 'Okay', {
  //        horizontalPosition: 'right',
  //        verticalPosition: 'top', duration: 3000
  //      });
  //    } else if (closeParenthesisCount > openParenthesisCount) {
  //      this._snackBar.open('Missing Opening parentheses.', 'Okay', {
  //        horizontalPosition: 'right',
  //        verticalPosition: 'top', duration: 3000
  //      });
  //    }
  //    return;
  //  }
  //  console.log(this.sequence)
  //  console.log("this.sequenceExp")
  //  this.sequence = this.sequence.map(item => {
  //    if (item.type === 'parameter') {
  //      const matched = this.parameters.find(p => p.parameterName === item.value);
  //      if (matched) item.value = matched.parameterName;
  //    } else if (item.type === 'factor') {
  //      const matched = this.factors.find(p =>
  //        p.value1 === item.value || (p.value1 + '-' + p.value2) === item.value
  //      );
  //      if (matched) {
  //        item.value = this.isRangeOperator()
  //          ? (matched.value1 + '-' + matched.value2)
  //          : matched.value1;
  //      }
  //    }
  //    return item;
  //  });
  //  this.sequenceExp = this.sequence.map(item => {
  //    if (item.type === "parameter") {
  //      let matchedParam = this.parameters.find(p => p.parameterName === item.value);
  //      if (matchedParam) {
  //        return { ...item, value: matchedParam.parameterId };
  //      }
  //    }
  //    if (item.type === "factor") {
  //      let matchedFactor = this.factors.find(p => p.value1 === item.value || (p.value1 + '-' + p.value2) === item.value);
  //      if (matchedFactor) {
  //        return { ...item, value: matchedFactor.factorId };
  //      }
  //    }
  //    return item;
  //  });
  //  const expressionExp = this.sequenceExp.map(item => item.value).join(" ");


  //  if (this.expressionForm.valid) {
  //    var paylaod = {
  //      "eruleMasterId": Number(this.expressionForm.value.eruleMasterId),
  //      "eruleId": this.isInsertNewRecord ? 0 : this.updatedIndexId,
  //      "eruleName": this.expressionForm.value.name,
  //      "description": this.expressionForm.value.description,
  //      "expression": expressionExp,
  //      "expShown": this.expressionForm.value.expshown,
  //      "entityId": null,
  //      "isActive": this.expressionForm.value.isActive,
  //      "isException": this.expressionForm.value.isExceptionRule || false,
  //      "exceptionId": this.expressionForm.value.exceptionId,
  //      "parentEruleId": this.expressionForm.value.parentRuleId
  //    }
  //    console.log("Payload: ", this.EditEruleMaster)
  //    if (this.isInsertNewRecord || this.IfAddNewVersion && !this.IEditRuleVersion) {
  //      paylaod.eruleId = 0

  //      this.addNewRule(paylaod)
  //    }
  //    else {

  //      console.log(paylaod)

  //      console.log("editpayload")
  //      this.updateRule(paylaod);
  //    }
  //  } else {
  //    this.expressionForm.markAllAsTouched();
  //  }
  //}
  refreshRules() {
    this.ruleService.getRulesList().subscribe({
      next: (response: any) => {
        if (!response.isSuccess) return;

        const allRules = response.data;

        // Group by rule name
        const grouped = allRules.reduce((acc: any, rule: any) => {
          if (!acc[rule.eruleName]) acc[rule.eruleName] = [];
          acc[rule.eruleName].push(rule);
          return acc;
        }, {});

        this.parentRules = Object.entries(grouped).map(([eruleName, versions]: any) => {
          const sortedVersions = versions
            .filter((v: any) => v.version !== null)
            .sort((a: any, b: any) => b.version - a.version);

          const latest = sortedVersions[0] || versions[0];

          return {
            eruleName,
            description: latest.description || '',
            versions: sortedVersions,
            eruleMasterId: latest.eruleMasterId,
            isActive: latest.isActive
          };
        });

        // Update pagination after rules are ready
        if (this.paginator) {
          this.paginator.length = this.parentRules.length;
          this.paginator.pageIndex = 0;
          this.updatePaginatedRules();
        }
      },
      error: (err) => {
        this._snackBar.open(err.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.error('Failed to refresh rules:', err)
      }
    });
  }

  submit() {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });

    if (!this.expressionForm.valid) {
      this.expressionForm.markAllAsTouched();
      return;
    }

    this.isParameterSelected = false;
    this.selectedParameterId = 0;

    const openParenthesisCount = (this.expressionForm.value.expshown.match(/\(/g) || []).length;
    const closeParenthesisCount = (this.expressionForm.value.expshown.match(/\)/g) || []).length;

    if (openParenthesisCount !== closeParenthesisCount) {
      const message = openParenthesisCount > closeParenthesisCount
        ? 'Please close the parentheses first.'
        : 'Missing Opening parentheses.';
      this._snackBar.open(message, 'Okay', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 3000
      });
      return;
    }

    // Use the same sequence for both display and expression (values instead of IDs)
    const expressionExp = this.sequence.map(item => {
      if (item.type === 'parameter') {
        // For parameters, we still need IDs in the backend expression
        const matchedParam = this.parameters.find(p => p.parameterName === item.value);
        return matchedParam ? matchedParam.parameterId : item.value;
      } else if (item.type === 'factor') {
        // For factors, use the actual value (like "20-30") in the expression
        return item.value;
      } else {
        return item.value;
      }
    }).join(" ");

    const expShown = this.sequence.map(item => item.value).join(" ");

    const payload = {
      eruleMasterId: Number(this.expressionForm.value.eruleMasterId),
      eruleId: this.isInsertNewRecord ? 0 : this.updatedIndexId,
      eruleName: this.expressionForm.value.name,
      description: this.expressionForm.value.description,
      expression: expressionExp, // This now contains actual values like "20-30"
      expShown: expShown, // Display expression
      entityId: null,
      isActive: this.expressionForm.value.isActive,
      isException: this.expressionForm.value.isExceptionRule || false,
      exceptionId: this.expressionForm.value.exceptionId,
      parentEruleId: this.expressionForm.value.parentRuleId
    };

    console.log('Submission Payload:', payload);
    console.log('Expression with values:', expressionExp);
    console.log('Display expression:', expShown);

    if (this.isInsertNewRecord || (this.IfAddNewVersion && !this.IEditRuleVersion)) {
      payload.eruleId = 0;
      this.addNewRule(payload);
    } else {
      this.updateRule(payload);
    }

  }
  updateExpression(): void {
    this.expshown = this.sequence.map(item => item.value).join(' ');
    if (this.enteredFactorValue) {
      this.expshown += ` ${this.enteredFactorValue}`;  // Append the factor value if entered
    }
    this.expressionForm.get('expshown')?.setValue(this.expshown);
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
    this.expressionForm.controls['factorName'].disable();
    this.expressionForm.controls['factorName'].reset();
    this.sequence = [];
    this.expressionForm.get('expshown')?.setValue('');
    this.expshown = '';
    this.selectedParameter = '';
    this.selectedFactor = '';
    this.selectedOperator = '';

    // Reset all select elements to the first option (Select)
    if (this.openParenthesisSelect) {
      this.openParenthesisSelect.nativeElement.selectedIndex = 0;
    }
    if (this.parameterSelect) {
      this.parameterSelect.nativeElement.selectedIndex = 0;
    }
    if (this.operatorSelect) {
      this.operatorSelect.nativeElement.selectedIndex = 0;
    }
    if (this.factorSelect) {
      this.factorSelect.nativeElement.selectedIndex = 0;
    }
    if (this.logicalOperatorSelect) {
      this.logicalOperatorSelect.nativeElement.selectedIndex = 0;
    }
    if (this.closeParenthesisSelect) {
      this.closeParenthesisSelect.nativeElement.selectedIndex = 0;
    }
  }
  private refreshFactors(): void {
    // Find the last parameter in the current sequence
    const lastParameterItem = [...this.sequence].reverse().find(item => item.type === 'parameter');

    if (!lastParameterItem) {
      this.factors = [];
      this.selectedParameterId = 0;
      return;
    }

    // Use case-insensitive matching like in getLastParameterInSequence()
    const sequenceValue = lastParameterItem.value.toLowerCase().replace(/\s+/g, ' ').trim();
    const param = this.parameters.find(p =>
      p.parameterName.toLowerCase().replace(/\s+/g, ' ').trim() === sequenceValue
    );

    if (!param) {
      this.factors = [];
      this.selectedParameterId = 0;
      return;
    }

    this.selectedParameterId = param.parameterId;

    this.factorService.getFactorsByParameterId(param.parameterId).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factors = response.data || [];
        } else {
          this.factors = [];
        }
      },
      error: () => {
        this.factors = [];
      }
    });
  }

  removeLastItem(): void {
    if (this.sequence.length > 0) {
      this.sequence.pop();
      this.updateExpression();

      if (this.sequence.length === 0) {
        this.resetToInitialState();
        return;
      }

      this.updateStatesBasedOnLastItem();
      this.updateFactorsAfterRemoval(); // always refresh factors based on last parameter
      this.refreshFactorsBasedOnLastParameter();
    }
  }
  private refreshFactorsBasedOnLastParameter(): void {
    const lastParam = this.getLastParameterInSequence();

    if (lastParam) {
      this.selectedParameterId = lastParam.parameterId;
      this.factorService.getFactorsByParameterId(lastParam.parameterId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.factors = response.data || [];
          } else {
            this.factors = [];
          }
        },
        error: () => {
          this.factors = [];
        }
      });
    } else {
      this.factors = [];
      this.selectedParameterId = 0;
    }
  }

  private updateStatesAfterCloseParenthesis(): void {
    const openCount = (this.expshown.match(/\(/g) || []).length;
    const closeCount = (this.expshown.match(/\)/g) || []).length;

    this.isOpenParenthesis = openCount > closeCount;
    this.isParameterName = true;
    this.isOperator = true;
    this.isFactor = true;
    this.isLogicalOperator = false;
    this.isCloseParenthesis = openCount > closeCount;
  }

  private updateStatesAfterOpenParenthesis(): void {
    this.isOpenParenthesis = true;
    this.isParameterName = false;
    this.isOperator = true;
    this.isFactor = true;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
  }

  private resetToInitialState(): void {
    this.isOpenParenthesis = false;
    this.isParameterName = false;
    this.isOperator = true;
    this.isFactor = true;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;
    this.isParameterSelected = false;
    this.selectedParameterId = 0;
    this.selectedParameter = '';
    this.selectedFactor = '';
    this.selectedOperator = '';
    this.expressionForm.controls['factorName'].disable();
    this.expressionForm.controls['factorName'].reset();
  }
  private getLastParameterInSequence(): any {
    for (let i = this.sequence.length - 1; i >= 0; i--) {
      if (this.sequence[i].type === 'parameter') {
        // Normalize both the sequence value and parameter names for comparison
        const sequenceValue = this.sequence[i].value.toLowerCase().replace(/\s+/g, ' ').trim();

        return this.parameters.find(p =>
          p.parameterName.toLowerCase().replace(/\s+/g, ' ').trim() === sequenceValue
        );
      }
    }
    return null;
  }

  private updateFactorsAfterRemoval(): void {
    const lastParam = this.getLastParameterInSequence();
    if (lastParam) {
      this.selectedParameterId = lastParam.parameterId;
      this.factorService.getFactorsByParameterId(lastParam.parameterId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.factors = response.data || [];
          }
        },
        error: () => {
          this.factors = [];
        }
      });
    } else {
      this.factors = [];
    }
  }

  private updateStatesBasedOnLastItem(): void {
    const lastItem = this.sequence[this.sequence.length - 1];

    switch (lastItem.type) {
      case 'parameter':
        this.handleParameterState();
        break;
      case 'factor':
        this.handleFactorState();
        break;
      case 'operator':
        this.handleOperatorState();
        break;
      case 'openParenthesis':
        this.handleOpenParenthesisState();
        break;
      case 'closeParenthesis':
        this.handleCloseParenthesisState();
        break;
      case 'logicalOperator':
        this.handleLogicalOperatorState();
        break;
      default:
        this.resetToInitialState();
    }
  }

  private handleParameterState(): void {
    this.isParameterName = true;
    this.isOpenParenthesis = true;
    this.isOperator = false;
    this.isFactor = true;
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
    this.isLogicalOperator = true;
  }

  private handleFactorState(): void {
    this.isLogicalOperator = false;
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
    this.isOpenParenthesis = true;
    this.isOperator = true;
    this.isFactor = true;
    this.isParameterName = true;
  }

  private handleOperatorState(): void {
    // Use the same method as everywhere else to find the parameter
    const lastParam = this.getLastParameterInSequence();

    if (lastParam) {
      this.selectedParameterId = lastParam.parameterId;
      this.factorService.getFactorsByParameterId(lastParam.parameterId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.factors = response.data || [];
          }
        },
        error: (error) => {
          console.log("Error loading factors in operator state:", error);
          this.factors = [];
        }
      });
    } else {
      this.factors = [];
    }

    this.isOperator = true;
    this.isFactor = false;
    this.expressionForm.controls['factorName'].enable();
    this.isOpenParenthesis = true;
    this.isParameterName = true;
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
    this.isLogicalOperator = true;
  }
  private handleOpenParenthesisState(): void {
    this.isOpenParenthesis = false;
    this.isParameterName = false;
    this.isFactor = true;
    this.isOperator = true;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
  }

  private handleCloseParenthesisState(): void {
    const openCount = (this.expshown.match(/\(/g) || []).length;
    const closeCount = (this.expshown.match(/\)/g) || []).length;

    this.isOpenParenthesis = openCount > closeCount;
    this.isParameterName = true;
    this.isOperator = true;
    this.isFactor = true;
    this.isLogicalOperator = false;
    this.isCloseParenthesis = openCount > closeCount;
  }

  private handleLogicalOperatorState(): void {
    this.isOpenParenthesis = false;
    this.isParameterName = false;
    this.isFactor = true;
    this.isOperator = true;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = this.shouldEnableCloseParenthesis();
  }

  private shouldEnableCloseParenthesis(): boolean {
    const openCount = (this.expshown.match(/\(/g) || []).length;
    const closeCount = (this.expshown.match(/\)/g) || []).length;
    return openCount > closeCount;
  }
  parseExpressionToSequence(expshown: string): { type: string; value: string }[] {
    const sequence: { type: string; value: string }[] = [];

    if (!expshown || !expshown.trim()) return [];

    // Step 1: Collect operators and logical operators
    const operators = this.operatorsAry.map(op => op.conditionValue.trim());
    const logicalOperators = this.logicalOperator.map((op: string) => op.trim());

    // Step 2: Build regex pattern (case-insensitive, handle multi-word)
    const operatorPattern = operators.map(op => op.replace(/\s+/g, '\\s+')).join('|');
    const logicalOperatorPattern = logicalOperators.join('|');
    const regex = new RegExp(`(${logicalOperatorPattern}|${operatorPattern}|\\(|\\)|\\S+)`, 'gi');

    // Step 3: Tokenize
    const tokens = expshown.match(regex);
    if (!tokens) return [];

    let i = 0;
    while (i < tokens.length) {
      let currentToken = tokens[i].trim();
      let nextToken = tokens[i + 1]?.trim() || null;

      // Helper: Determine if a token is boundary
      const isBoundaryToken = (token: string): boolean => {
        if (!token) return false;
        token = token.trim();
        return (
          logicalOperators.some((op: string) => op.toLowerCase() === token.toLowerCase()) ||
          operators.some(op => op.toLowerCase() === token.toLowerCase()) ||
          token === '(' ||
          token === ')'
        );
      };

      // Step 4: Handle boundaries
      if (isBoundaryToken(currentToken)) {
        if (logicalOperators.some((op: string) => op.toLowerCase() === currentToken.toLowerCase())) {
          sequence.push({ type: 'logicalOperator', value: currentToken });
        } else if (operators.some(op => op.toLowerCase() === currentToken.toLowerCase())) {
          sequence.push({ type: 'operator', value: currentToken });
        } else if (currentToken === '(') {
          sequence.push({ type: 'openParenthesis', value: currentToken });
        } else if (currentToken === ')') {
          sequence.push({ type: 'closeParenthesis', value: currentToken });
        }
        i++;
        continue;
      }

      // Step 5: Merge multi-word parameters/factors (until next operator/logical)
      while (nextToken && !isBoundaryToken(nextToken)) {
        currentToken += ` ${nextToken}`;
        i++;
        nextToken = tokens[i + 1]?.trim() || null;
      }

      currentToken = currentToken.trim();

      // Step 6: Check parameter/factor classification
      const { isParameter, isFactor } = this.isParameterOrFactor(currentToken);

      if (isParameter && isFactor) {
        // Disambiguate based on previous token
        const prevValue = sequence[sequence.length - 1]?.value || '';
        const prevIsOperator = operators.some(op => op.toLowerCase() === prevValue.toLowerCase());
        sequence.push({
          type: prevIsOperator ? 'factor' : 'parameter',
          value: currentToken
        });
      } else if (isParameter) {
        sequence.push({ type: 'parameter', value: currentToken });
      } else if (isFactor) {
        sequence.push({ type: 'factor', value: currentToken });
      } else {
        sequence.push({ type: 'unknown', value: currentToken });
      }

      i++;
    }

    return sequence;
  }

  private isParameterOrFactor(token: string): { isParameter: boolean; isFactor: boolean } {
    const isParameter = this.parameters.some(param => param.parameterName === token);
    const isFactor = this.factorsList.some(factor => factor.value1 === token.trim());
    return { isParameter, isFactor };
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  toggleRowSelection(row: any): void {
    if (this.selection.has(row)) {
      this.selection.delete(row);
    } else {
      this.selection.add(row);
    }
  }

  toggleAllSelection(checked: boolean): void {
    if (checked) {
      this.selection = new Set(this.dataSource.data);
    } else {
      this.selection.clear();
    }
  }

  isAllSelected(): boolean {
    return this.selection.size === this.dataSource.data.length;
  }

  isRowSelected(row: any): boolean {
    return this.selection.has(row);
  }

  downloadTemplate() {
    this.isDownloading = true;
    this.message = "Please wait, template is downloading...";
    this.ruleService.downloadTemplate().subscribe((response) => {
      this.isDownloading = false;
      const blob = new Blob([response], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'Rules-Template.xlsm'; // Filename for the download
      a.click();
      window.URL.revokeObjectURL(url);
      this._snackBar.open('Rules Template Download Successfully.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    });
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      this.importRules(this.selectedFile);
    } else {
      this._snackBar.open('Please select a file first.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    }
  }

  importRules(selectedFile: File) {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    this.createdBy = this.loggedInUser.user.userName;
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    this.ruleService.importRule(selectedFile, this.createdBy).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.deleteKeyForMultiple = 'eruleId';
        //this.fetchAllErules();
        this.refreshRules();
        // this.fetchFactorsList();
        this.fetchParametersList();
        this.fetchAllFactorsList();
        this.fetchConditions();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      },
    });
  }

  // exportRules() {
  //   this.ruleService.exportRules().subscribe({
  //     next: (response: Blob) => {
  //       console.log('Blob resp: ', response);
  //       const url = window.URL.createObjectURL(response);
  //       const anchor = document.createElement('a');
  //       anchor.href = url;
  //       anchor.download = 'ERules.xlsx';
  //       document.body.appendChild(anchor);
  //       anchor.click();
  //       document.body.removeChild(anchor);
  //       window.URL.revokeObjectURL(url);
  //       this._snackBar.open('Export ERules Successfully.', 'Okay', {
  //         duration: 2000,
  //         horizontalPosition: 'right',
  //         verticalPosition: 'top',
  //       });
  //     },
  //     error: (error) => {
  //       this._snackBar.open(error.message, 'Okay', {
  //         duration: 2000,
  //         horizontalPosition: 'right',
  //         verticalPosition: 'top',
  //       });
  //     }
  //   });
  // }

  exportRules() {
    const selectedIds = Array.from(this.selection).map(row => row.eruleId);
    this.ruleService.exportRules(selectedIds).subscribe({
      next: (response: Blob) => {
        console.log('Blob resp: ', response);
        const url = window.URL.createObjectURL(response);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'ERules.xlsx';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(url);
        this._snackBar.open('Export ERules Successfully.', 'Okay', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      }
    });
  }

  private getFullRuleFromECardExpression(eCardExpression: string): { expressionWithPIDs: string, expressionWithPNames: string } {
    let expressionWithPNames = eCardExpression;
    let expressionWithPIDs = eCardExpression;

    this.ruleService.getRulesList().subscribe({
      next: (response) => {
        const rules = response.data;

        rules.forEach((rule: any) => {
          expressionWithPNames = expressionWithPNames.replaceAll(String(rule.eruleId), rule.expShown);
          expressionWithPIDs = expressionWithPIDs.replaceAll(String(rule.eruleId), rule.expression);
        });

        return { expressionWithPIDs, expressionWithPNames };
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
      }
    });

    return { expressionWithPIDs, expressionWithPNames };
  }

  openValidatorDialog(entity: any) {
    this.parameterService.getParameters().subscribe({
      next: (resp) => {
        this.validationDialogService.openValidationDialog({
          actionType: 'exits',
          expshown: entity.Expshown,
          parameters: resp.data,
          expression: entity.expression,
          validationType: 'ERule',
          valideeId: entity.eruleId
        })
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 10000
        });
      }
    })
  }

  openValidatorDialogAdd(expshown: any) {
    this.isParameterSelected = false;
    this.selectedParameterId = 0;

    const openParenthesisCount = (this.expressionForm.value.expshown.match(/\(/g) || []).length;
    const closeParenthesisCount = (this.expressionForm.value.expshown.match(/\)/g) || []).length;

    if (openParenthesisCount !== closeParenthesisCount) {
      if (openParenthesisCount > closeParenthesisCount) {
        this._snackBar.open('Please close the parentheses first.', 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      } else if (closeParenthesisCount > openParenthesisCount) {
        this._snackBar.open('Missing Opening parentheses.', 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
      return;
    }

    this.sequenceExp = this.sequence.map(item => {
      if (item.type === "parameter") {
        let matchedParam = this.parameters.find(p => p.parameterName === item.value);
        if (matchedParam) {
          return { ...item, value: matchedParam.parameterId };
        }
      }
      if (item.type === "factor") {
        let matchedParam = this.factor.find(p => p.value1 === item.value);
        if (matchedParam) {
          return { ...item, value: matchedParam.factorId };
        }
      }
      return item;
    });
    const expressionExp = this.sequenceExp.map(item => item.value).join(" ");

    if (!this.isInsertNewRecord) {
      expshown = this.expressionForm.value.expshown;
    }
    this.parameterService.getParameters().subscribe({
      next: (resp) => {
        this.validationDialogService.openValidationDialog({
          actionType: 'form',
          expshown: expshown,
          parameters: resp.data,
          expression: expressionExp,
          validationType: 'ERule',
          valideeId: this.eruleId
        })
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }
  paginatedRules: any[] = [];
  loadParentRules() {
    this.ruleService.getRulesList().subscribe({
      next: (response: any) => {
        if (response.isSuccess) {
          const allRules = response.data;

          // Group by rule name
          const grouped = allRules.reduce((acc: any, rule: any) => {
            if (!acc[rule.eruleName]) acc[rule.eruleName] = [];
            acc[rule.eruleName].push(rule);
            return acc;
          }, {});

          this.parentRules = Object.entries(grouped).map(([eruleName, versions]: any) => {
            const sortedVersions = versions
              .filter((v: any) => v.version !== null)
              .sort((a: any, b: any) => b.version - a.version);

            const latest = sortedVersions[0] || versions[0];

            return {
              eruleName,
              description: latest.description || '',
              versions: sortedVersions,
              eruleMasterId: latest.eruleMasterId,
              isActive: latest.isActive
            };
          });

          // Initialize pagination after fetching data
          setTimeout(() => this.initPagination(), 0);
        }
      }
    });
  }

  initPagination() {
    if (!this.paginator) return;
    this.paginator.length = this.parentRules.length; // total items
    this.paginator.pageIndex = 0;
    this.updatePaginatedRules();

    // subscribe to paginator page changes
    this.paginator.page.subscribe(() => this.updatePaginatedRules());
  }
  updatePaginatedRules() {
    if (!this.paginator) {
      this.paginatedRules = [...this.parentRules]; // fallback if no paginator
      return;
    }
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    this.paginatedRules = this.parentRules.slice(startIndex, endIndex);
  }
}
