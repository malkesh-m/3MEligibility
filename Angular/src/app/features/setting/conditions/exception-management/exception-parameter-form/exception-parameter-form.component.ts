import { MatTableDataSource } from "@angular/material/table";
import { TableComponent } from "../../../../../core/components/table/table.component";
import { Component, inject, ViewChild } from "@angular/core";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { FormBuilder, FormControl, FormGroup, Validators } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { RulesService } from "../../../../../core/services/setting/rules.service";
import { FactorsService } from "../../../../../core/services/setting/factors.service";
import { AuthService } from "../../../../../core/services/auth/auth.service";
import { ParameterService } from "../../../../../core/services/setting/parameter.service";
import { ValidationDialogService } from "../../../../../core/services/setting/validation-dialog.service";
import { MatDialog } from "@angular/material/dialog";
import { UtilityService } from "../../../../../core/services/utility/utils";
import { RolesService } from "../../../../../core/services/setting/role.service";
import { ExceptionManagementService } from "../../../../../core/services/setting/exception-management.service";
import { MatCheckboxChange } from "@angular/material/checkbox";
import { DeleteDialogComponent } from "../../../../../core/components/delete-dialog/delete-dialog.component";

export interface RulesRecord {
  eruleName: string;
  eruleDesc: string;
  Expression: string;
  eruleId: string;
  selected: boolean;
  Expshown: string
}

@Component({
  selector: 'app-exception-parameter-form',
  standalone: false,

  templateUrl: './exception-parameter-form.component.html',
  styleUrl: './exception-parameter-form.component.scss'
})
export class ExceptionParameterFormComponent {
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
  rulesColumnsAry = ['select', 'Name', 'Description', 'Expression','createdBy','updatedBy', 'Actions']
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
  createdBy:string = '';
  eruleId: number = 0;
  // exceptions: any[] = [];
  parentRules: any[] = [];
  //exceptions: any[] = [
  //  { exceptionID: 17, exceptionName: "For Product" },
  //  { exceptionID: 18, exceptionName: "string" },
  //  { exceptionID: 19, exceptionName: "gdfg" },
  //  { exceptionID: 20, exceptionName: "gsdf" },
  //  { exceptionID: 21, exceptionName: "test" },
  //  { exceptionID: 22, exceptionName: "test Updated" },
  //  { exceptionID: 25, exceptionName: "test" },
  //  { exceptionID: 28, exceptionName: "age exception" },
  //  { exceptionID: 30, exceptionName: "Corporate Gurantee" },
  //  { exceptionID: 31, exceptionName: "Age Exception" }
  //];

  
  constructor(
    private fb: FormBuilder,
    private ruleService: RulesService,
    private factorService: FactorsService,
    private authService: AuthService,
    private parameterService: ParameterService,
    private validationDialogService: ValidationDialogService,
    private dialog: MatDialog,
    private utilityService: UtilityService,
    private rolesService:RolesService,
    private exceptionsService: ExceptionManagementService
  ) {
    
  }

  ngOnInit(): void {
    this.deleteKeyForMultiple = 'eruleId';
    this.fetchAllErules();
    // this.fetchFactorsList();
    this.fetchFactorsLists();
    this.fetchParametersList();
    this.fetchAllFactorsList();
    // this.fetchAllExceptions();
    this.fetchConditions();

    this.expressionForm = this.fb.group  ({
      name: ['', [Validators.required]],
      factorName: [''],
      description: ['', [Validators.required]],
      expshown: ['', [Validators.required]],
      isExceptionRule: [false],
      exceptionId: new FormControl({ value: '', disabled: true }),
      parentRuleId: new FormControl({ value: '', disabled: true }),
    });

    this.expressionForm.get('isExceptionRule')!.valueChanges.subscribe(isExceptionRule => {
      const exceptionControl = this.expressionForm.get('exceptionId');
      const parentRuleControl = this.expressionForm.get('parentRuleId');

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

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  isRangeOperator(): boolean {
    return this.selectedOperator === 'Range';
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
    if (checked) {
      this.selectedRows = new Set(this.paginatedRows.map((_, index) => index));
      this.selectedRowsItem = new Set(this.paginatedRows.map((_, index) => _[this.checkedSelectedId]));
    } else {
      this.selectedRows.clear();
      this.selectedRowsItem.clear();
    }
  }

  fetchAllFactorsList() {
    this.factorService.getFactorsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.factorsList = response.data;
        }
      }, error: (error) => {
        console.log("rules :", error)
      }
    })
  }

  onSelect(type: string, data: any): void {
    var value = data.target.value;
    if (!value) return;

    if (this.sequence.length > 0 && this.sequence[this.sequence.length - 1].type === type) {
      this.sequence[this.sequence.length - 1].value = value;
    } else {
      this.sequence.push({ type, value });
    }

    if (type === 'parameter') {
      this.selectedParameter = value;
      this.selectedFactor = '';
      this.selectedOperator = '';
      this.isParameterSelected = true;
      const selectedParam = this.parameters.find(param => param.parameterName === value);
      if (selectedParam) {
        this.selectedParameterId = selectedParam.parameterId; // Store the corresponding parameterId
      }
      this.factorService.getFactorsByParameterId(this.selectedParameterId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            if (response.data && response.data.length > 0) {
              this.factors = response.data;
              this.isOperator = false;
              this.isOpenParenthesis = true;
              this.isParameterName = true;
            } else {
              this.factors = [];
            }
          }
        }, error: (error) => {
          console.log("rules :", error)
        }
      })
    } else if (type === 'factor') {
      this.selectedFactor = value;
      this.selectedParameter = '';
      this.selectedOperator = '';
      this.isFactor = true;
      this.isLogicalOperator = false;
      this.isCloseParenthesis = false;
      this.expressionForm.controls['factorName'].disable();
      if (this.expressionForm.value.factorName) {
        var value = this.expressionForm.value.factorName;
        this.sequence.push({ type, value });
        this.updateExpression();
      }
    } else if (type === 'operator') {
      this.selectedOperator = value;
      this.isFactor = false;
      this.isOperator = true;
      this.expressionForm.controls['factorName'].enable();
    } else if (type === 'logicalOperator') {
      this.isOpenParenthesis = false;
      this.isParameterName = false;
      this.isLogicalOperator = true;
      this.isCloseParenthesis = true;
    }
    data.target.value = '';
    this.updateExpression();
  }

  fetchAllErules() {
    this.eRulesListAry = [];
    this.ruleService.getRulesList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          // Sorting the response data based on lastModifiedDateTime in descending order
          const sortedData = response.data.sort((a: any, b: any) =>
            new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
          );

          this.eRulesListAry = [...sortedData]

          //Parent rules are rules that are NOT exception rules
          this.parentRules = this.eRulesListAry.filter((rule: any) => !rule.isException)

          this.dataSource.data = sortedData.map((res: any) => ({
            Name: res.eruleName,
            Description: res.eruleDesc,
            Expshown: res.expShown,
            Expression: res.expression,
            eruleId: res.eruleId,
            createdBy: res.createdBy,
            createdByDateTime: res.createdByDateTime,
            updatedBy: res.updatedBy,
            updatedByDateTime: res.updatedByDateTime,
            isExceptionRule: res.isException,
            parentEruleId: Number(res.parentEruleId),
            exceptionId: Number(res.exceptionId)
          }))
          this.dataSource.paginator = this.paginator;
          this.dataSource.sort = this.sort;
        }
      }, error: (error) => {
        this.eRulesListAry = [];
        console.log("rules :", error)
      }
    })
  }

  fetchParametersList() {
    this.parameterService.getParameters().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.parameters = response.data;
        }
      }, error: (error) => {
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
        console.log("rules :", error)
      }
    })
  }

  fetchAllExceptions() {
    this.exceptionsService.getExceptionList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
        //  this.exceptions = response.data.map((exc: any) => ({...exc, exceptionID: Number(exc.exceptionID)}));
        }
      }, error: (error) => {
        console.log("rules :", error)
      }
    })
  }

  addNewRule(payload: any) {
    payload.createdBy = this.loggedInUser.user.userName;
    payload.updatedBy = this.loggedInUser.user.userName;
    this.ruleService.addNewRule(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.fetchAllErules();
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
    payload.createdBy = this.createdBy;
    payload.updatedBy = this.loggedInUser.user.userName;
    this.ruleService.updateExistingRule(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.fetchAllErules();
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
        this.fetchAllErules();
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  deleteCheckedMultipleRules() {
    const selectedIds = Array.from(this.selection).map(row => row.eruleId);
    if (this.selection.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.ruleService.deleteMultipleRules(selectedIds).subscribe({
          next: (response) => {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.fetchAllErules();
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

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.dataSource.filter = filterValue;
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  editRecord(event: any) {
    console.log(event)
    this.isInsertNewRecord = false;
    this.formVisible = true;
    this.expressionForm.setValue({
      name: event.Name,
      description: event.Description,
      factorName: '',
      expshown: event.Expshown,
      isExceptionRule: event.isExceptionRule,
      exceptionId: event.exceptionId,
      parentRuleId: event.parentEruleId
    })
    
    this.eruleId = event.eruleId;
    this.createdBy = event.createdBy;
    this.updatedIndexId = event.eruleId;
    let lastMatchingParameterId = null;

    for (let i = this.parameters.length - 1; i >= 0; i--) {
      const parameter = this.parameters[i];
      if (event.Expshown.includes(parameter.parameterName)) {
        lastMatchingParameterId = parameter.parameterId;
        break; // Exit the loop as soon as we find the last match
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
        }, error: (error) => {
          console.log("rules :", error)
        }
      })
    }
    this.sequence = this.parseExpressionToSequence(event.Expshown);
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

  }

  deleteRecord(event: any) {
    if (confirm(`Are you sure you want to delete Rule: "${event.Name}"?`)) {
      this.ruleService.deleteSingleRule(event.eruleId).subscribe({
        next: (response) => {
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.fetchAllErules();
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
      // Reverse loop through parameters to find the last matching parameterName in the expression
      let lastMatchingParameterId = null;

      for (let i = this.parameters.length - 1; i >= 0; i--) {
        const parameter = this.parameters[i];
        if (event.data.expshown.includes(parameter.parameterName)) {
          lastMatchingParameterId = parameter.parameterId;
          break; // Exit the loop as soon as we find the last match
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
          }, error: (error) => {
            console.log("rules :", error)
          }
        })
      }
      this.sequence = this.parseExpressionToSequence(event.data.Expshown);
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

  submit() {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });

    if (!this.expressionForm.valid)
      this.expressionForm.markAllAsTouched();

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

    if (this.expressionForm.valid) {
      var paylaod = {
        "eruleId": this.isInsertNewRecord ? 0 : this.updatedIndexId,
        "eruleName": this.expressionForm.value.name,
        "eruleDesc": this.expressionForm.value.description,
        "expression": expressionExp,
        "expShown": this.expressionForm.value.expshown,
        "entityId": null,
        "isException": this.expressionForm.value.isExceptionRule || false,
        "exceptionId": this.expressionForm.value.exceptionId,
        "parentEruleId": this.expressionForm.value.parentRuleId
      }
      console.log("Payload: ", paylaod)
      if (this.isInsertNewRecord) {
        this.addNewRule(paylaod)
      } else {
        this.updateRule(paylaod);
      }
    } else {
      this.expressionForm.markAllAsTouched();
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
    this.expressionForm.controls['factorName'].reset();;
    this.sequence = [];
    this.expressionForm.get('expshown')?.setValue('');
    this.expshown = '';
    this.selectedParameter = '';
    this.selectedFactor = '';
    this.selectedOperator = '';
    // Reset all select elements to the first option (Select)
    this.openParenthesisSelect.nativeElement.selectedIndex = 0;
    this.parameterSelect.nativeElement.selectedIndex = 0;
    this.operatorSelect.nativeElement.selectedIndex = 0;
    this.factorSelect.nativeElement.selectedIndex = 0;
    this.logicalOperatorSelect.nativeElement.selectedIndex = 0;
    this.closeParenthesisSelect.nativeElement.selectedIndex = 0;
  }

  removeLastItem(): void {
    if (this.sequence.length > 0) {
      this.sequence.pop();
      this.updateExpression();
      if (this.sequence.length > 0) {
        const lastItem = this.sequence[this.sequence.length - 1];
        const lastTolastItem = this.sequence[this.sequence.length - 2];
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
        } else if (lastItem.type === 'operator') {
          let lastMatchingParameterId = null;
          for (let i = this.parameters.length - 1; i >= 0; i--) {
            const parameter = this.parameters[i];
            if (this.sequence[this.sequence.length - 2].value.includes(parameter.parameterName)) {
              lastMatchingParameterId = parameter.parameterId;
              break; // Exit the loop as soon as we find the last match
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
              }, error: (error) => {
                console.log("rules :", error)
              }
            })
          }
          this.isOperator = true;
          this.isFactor = false;
          // this.expressionForm.controls['factorName'].reset();
          this.expressionForm.controls['factorName'].enable();
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
        } else if (lastItem.type === 'logicalOperator') {
          this.isOpenParenthesis = false;
          this.isParameterName = false;
          this.isFactor = true;
          this.isOperator = true;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
        }
      } else {
        this.selectedParameter = '';
        this.selectedFactor = '';
        this.selectedOperator = '';
      }
    }
  }

  parseExpressionToSequence(expshown: string): { type: string; value: string }[] {
    const sequence: { type: string; value: string }[] = [];

    // Improved tokenization to handle multi-word operators
    const operators = this.operatorsAry.map(op => op.conditionValue); // Get operators
    const logicalOperators = this.logicalOperator; // Your logical operators (AND, OR)

    // Create a regex pattern that matches operators and logical operators, handling multi-word conditions.
    const operatorPattern = operators.join('|').replace(/\s+/g, '\\s+'); // Create a regex for operators
    const logicalOperatorPattern = logicalOperators.join('|'); // Create a regex for logical operators

    // Match parameters (names) and operators (single or multi-word)
    const regex = new RegExp(
      `(${logicalOperatorPattern}|${operatorPattern}|\\(|\\)|\\S+)`, 'g'
    );

    const tokens = expshown.match(regex);

    if (!tokens) {
      return [];
    }

    let i = 0;
    while (i < tokens.length) {
      let currentToken = tokens[i];
      let nextToken = tokens[i + 1] || null;

      // Helper function to check if a token is a logical operator, operator, or parentheses
      const isBoundaryToken = (token: string): boolean => {
        return (
          logicalOperators.includes(token) ||
          operators.includes(token) ||
          token === ")" ||
          token === "("
        );
      };

      // If the current token itself is a boundary token, do not process it for merging
      if (isBoundaryToken(currentToken)) {
        if (logicalOperators.includes(currentToken)) {
          sequence.push({ type: "logicalOperator", value: currentToken });
        } else if (operators.includes(currentToken)) {
          sequence.push({ type: "operator", value: currentToken });
        } else if (currentToken === "(") {
          sequence.push({ type: "openParenthesis", value: currentToken });
        } else if (currentToken === ")") {
          sequence.push({ type: "closeParenthesis", value: currentToken });
        }
        i++;
        continue;
      }

      // Merge tokens until we hit a boundary token
      while (nextToken && !isBoundaryToken(nextToken)) {
        currentToken += ` ${nextToken}`;
        i++; // Move to the next token
        nextToken = tokens[i + 1] || null; // Update next token
      }

      // Check if currentToken (merged or not) exists in parameters or factors
      const { isParameter, isFactor } = this.isParameterOrFactor(currentToken);

      if (isParameter && isFactor) {
        // Check previous token
        const previousToken = sequence[sequence.length - 1]?.value || null;
        const previousIsOperator = previousToken && operators.includes(previousToken);
        if (previousIsOperator) {
          sequence.push({ type: "factor", value: currentToken });
        } else {
          sequence.push({ type: "parameter", value: currentToken });
        }
      } else if (isParameter) {
        sequence.push({ type: "parameter", value: currentToken });
      } else if (isFactor) {
        sequence.push({ type: "factor", value: currentToken });
      } else {
        sequence.push({ type: "unknown", value: currentToken });
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
    this.ruleService.importRule(selectedFile,this.createdBy).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.deleteKeyForMultiple = 'eruleId';
        this.fetchAllErules();
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
          verticalPosition: 'top', duration: 3000
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

    if(!this.isInsertNewRecord){
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
}
