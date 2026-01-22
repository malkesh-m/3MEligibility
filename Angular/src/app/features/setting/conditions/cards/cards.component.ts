import { Component, inject, ViewChild } from '@angular/core';
import { CardsService } from '../../../../core/services/setting/cards.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ParameterService } from '../../../../core/services/setting/parameter.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RulesService } from '../../../../core/services/setting/rules.service';
import { MatPaginator } from '@angular/material/paginator';
import { UtilityService } from '../../../../core/services/utility/utils';
import { DeleteDialogComponent } from '../../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { ValidatorDialogComponent } from '../../../../core/components/validator-dialog/validator-dialog.component';
import { ValidationDialogService } from '../../../../core/services/setting/validation-dialog.service';
import { RolesService } from '../../../../core/services/setting/role.service';

@Component({
  selector: 'app-cards',
  standalone: false,
  templateUrl: './cards.component.html',
  styleUrl: './cards.component.scss',
})
export class CardsComponent {
  @ViewChild('tableChild') tableChild: any;
  @ViewChild('paginator') paginator!: MatPaginator;
  // Table Columns and Data
  cardsColumnsAry: string[] = ['Select', 'CardName', 'Description', 'Exp shown', 'Created By', 'Updated By'];
  cardsHeaderAry = ['CardName', 'Description', 'Exp shown','Created By', 'Updated By'];
  eCardsListAry: any[] = [];
  rules: any[] = [];
  parameters: any[] = [];
  filteredCards: any[] = []; // Filtered list displayed in the table

  // UI State
  formVisible: boolean = false;
  isInsertNewRecord: boolean = false;
  menuVisible: boolean = false;
  updatedIndexId: number = 0;
  searchTerm: string = '';
  // Expression Builder State
  sequence: { type: string; value: string }[] = [];
  expression: string = '';
  expshown: string = '';
  // Dropdown Options
  openParanthesis: string[] = ['('];
  closeParanthesis: string[] = [')'];
  logicalOperator: string[] = ['And', 'Or'];

  // Form Group
  expressionForm: FormGroup;

  // Dropdown States
  isRuleDisabled: boolean = false;
  isFunctionDisabled: boolean = true;
  isCloseParenthesisDisabled: boolean = true;
  paginatedCardsListAry: any[] = [];
  deleteKeyForMultiple: string = 'ecardId';

  selectedRows: any = []
  isOpenParenthesis: boolean = false;
  isLogicalOperator: boolean = true;
  isCloseParenthesis: boolean = true
  selectedRule: string = '';
  selectedExpression: string = '';
  selectedRules: { eruleName: string; expShown: string }[] = [];
  selectedFile: File | null = null;
  private _snackBar = inject(MatSnackBar);
  sequenceExp: { type: string; value: any }[] = [];
  combinevalue: string = '';
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = "Loading data, please wait...";
  loggedInUser: any = null;

  constructor(
    private cardService: CardsService,
    private ruleService: RulesService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private utilityService: UtilityService,
    private dialog: MatDialog,
    private authService: AuthService,
    private parameterService: ParameterService,
    private rulesService: RulesService,
    private validationDialogService: ValidationDialogService,
    private rolesService:RolesService
  ) {
    this.expressionForm = this.fb.group({
      CardName: ['', Validators.required],
      Description: ['', Validators.required],
      Expshown: ['', [Validators.required]],
    });
  }

  hasPermission(roleId: number): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  ngOnInit(): void {
    this.fetchCards();
    this.fetchRuleList();
  }

  fetchCards(): void {
    this.eCardsListAry = [];
    this.cardService.getCardsList().subscribe({
      next: (response) => {
        if (response?.data) {
          // Sorting the response data based on lastModifiedDateTime in descending order
          const sortedData = response.data.sort((a: any, b: any) =>
            new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
          );
          this.eCardsListAry = sortedData.map((card: any) => ({
            CardName: card.ecardName,
            Description: card.ecardDesc,
            "Exp shown": card.expshown,
            expression: card.expression,
            ecardId: card.ecardId,
            "Created By": card.createdBy,
            "Created Date": card.createdByDateTime,
            "Updated By": card.updatedBy,
            "Updated Date": card.updatedByDateTime
          }));
          this.filteredCards = this.eCardsListAry; // Initialize filtered list
        }
      },
      error: (error) => console.error('Error fetching cards:', error),
    });
  }

  toggleAll(event: any): void {
    const checked = event.target.checked;
    this.paginatedCardsListAry.forEach((card) => {
      card.selected = checked;
      if (checked) this.selectedRows.add(card.ecardId);
      else this.selectedRows.delete(card.ecardId);
    });
  }

  toggleRowSelection(card: any): void {
    card.selected = !card.selected;
    if (card.selected) this.selectedRows.add(card.ecardId);
    else this.selectedRows.delete(card.ecardId);
  }

  deleteCheckedMultipleCards() {
    let listOfSelectedIds = this.tableChild.selectedRowsItem.size;
    if (listOfSelectedIds.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }
    const dialogRef = this.dialog.open(DeleteDialogComponent);
    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.cardService.deleteMultipleCards([...this.tableChild.selectedRowsItem]).subscribe({
          next: (response) => {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.fetchCards();
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
  fetchRuleList(): void {
    this.ruleService.getRulesList().subscribe({
      next: (response) => {

        if (!response?.data?.length) return;

        // STEP 1: Only Active rules
        const activeRules = response.data.filter((r: any) => r.isActive === true);

        // STEP 2: Group by rule master (eruleMasterId)
        const grouped = activeRules.reduce((acc: any, rule: any) => {
          const key = rule.eruleMasterId;

          if (!acc[key]) {
            acc[key] = [];
          }

          acc[key].push(rule);
          return acc;
        }, {});

        // STEP 3: From each group pick highest version
        this.rules = (Object.values(grouped) as any[][]).map(
          (ruleGroup: any[]) =>
            ruleGroup.reduce((max, curr) =>
              curr.version > max.version ? curr : max
            )
        );

      },

      error: (err) => console.error("Error fetching rules:", err)
    });
  }


  submitEditForm(): void {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });

    if (this.expressionForm.invalid) {
      this.snackBar.open('Please fill out all required fields!', 'Close', { duration: 2000 });
      this.expressionForm.markAllAsTouched();
      return;
    }

    this.sequenceExp = this.sequence.map(item => {
      if (item.type === "rule") {
        let matchedParam = this.rules.find(p => p.eruleName === item.value);
        if (matchedParam) {
          return { ...item, value: matchedParam.eruleId };
        }
      }
      return item; // Return unchanged if not a parameter
    });
    const expressionExp = this.sequenceExp.map(item => item.value).join(" ");

    const payload = {
      ecardId: this.isInsertNewRecord ? 0 : this.updatedIndexId,
      ecardName: this.expressionForm.value.CardName,
      ecardDesc: this.expressionForm.value.Description,
      expression: expressionExp,
      expshown: this.expressionForm.value.Expshown,
      entityId: null,
    };
    if (this.isInsertNewRecord) {
      this.addCard(payload);
    } else {
      this.updateCard(payload);
    }
  }
  
  addCard(payload: any) {
    // payload.createdBy = this.loggedInUser.user.userName;
    // payload.updatedBy = this.loggedInUser.user.userName;
    payload.ecardName = payload.ecardName.trim();
    this.cardService.addCard(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.fetchCards();
        this.fetchRuleList();
        this.closeForm();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  updateCard(payload: any) {
    payload.ecardName = payload.ecardName.trim();

    // payload.updatedBy = this.loggedInUser.user.userName;
    this.cardService.updateCard(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.fetchCards();
        this.fetchRuleList();
        this.closeForm();
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  // Handle Dropdown Selection Logic
  onSelect(type: string, event: any): void {
    const value = event.target.value;
    if (!value) return;
    this.sequence.push({ type, value });

    // Logic to enable/disable fields
    if (type === 'rule') {
      this.isRuleDisabled = true;
      this.isLogicalOperator = false;
      this.isCloseParenthesis = false;
      this.isOpenParenthesis = true;
      this.selectedRule = event.target.value;

      // Find the selected rule object
      const selectedRuleObject = this.rules.find(rule => rule.eruleName === this.selectedRule);
      if (selectedRuleObject) {
        this.selectedExpression = selectedRuleObject.expShown;
        let logicalOperator = this.sequence.length > 1 ? this.sequence[this.sequence.length - 2].value : '';
        // Add to selected rules if not already present
        const exists = this.selectedRules.find(rule => rule.eruleName === this.selectedRule);
        if (!exists) {
          this.selectedRules.push(selectedRuleObject);
          if (this.combinevalue) {
            this.combinevalue += ` ${logicalOperator} ${selectedRuleObject.expShown}`;
        } else {
            this.combinevalue = selectedRuleObject.expShown;
        }
        }
      }
    } else if (type === 'logicalOperator') {
      this.isOpenParenthesis = false;
      this.isRuleDisabled = false;
      this.isLogicalOperator = true;
      this.isCloseParenthesis = true;
    }
    event.target.value = '';
    this.updateExpression();
  }

  removeRule(ruleName: string): void {
    this.selectedRules = this.selectedRules.filter(rule => rule.eruleName !== ruleName);
  }


  // removeFirstAndLastParentheses(str: string): string {
  //   const firstOpenParenIndex = str.indexOf('(');
  //   const lastCloseParenIndex = str.lastIndexOf(')');

  //   if (firstOpenParenIndex === -1 || lastCloseParenIndex === -1 || firstOpenParenIndex > lastCloseParenIndex) {
  //       // If there is no '(' or no ')' or '(' comes after ')', return the original string
  //       return str;
  //   }

  //   // Remove the first '('
  //   str = str.slice(0, firstOpenParenIndex) + str.slice(firstOpenParenIndex + 1);

  //   // Adjust the lastCloseParenIndex because the string length has been reduced by 1
  //   const adjustedLastCloseParenIndex = lastCloseParenIndex - 1;

  //   // Remove the last ')'
  //   str = str.slice(0, adjustedLastCloseParenIndex) + str.slice(adjustedLastCloseParenIndex + 1);

  //   return str;
  // }


  tableRowAction(event: { action: string; data: any }): void {
    if (event.action === 'edit') {
      this.isInsertNewRecord = false;
      this.formVisible = true;
      this.updatedIndexId = event.data.ecardId;
      this.selectedRules = [];
      this.combinevalue = "";

      const dbExpshown = event.data['Exp shown'];
      const dbExpression = event.data.expression;

      this.expressionForm.patchValue({
        CardName: event.data.CardName,
        Description: event.data.Description,
        Expshown: dbExpshown
      });

      this.expshown = dbExpshown;
      this.expression = dbExpression;
      this.sequence = this.parseExpressionToSequence(dbExpression);

      const dbExpressionParts = dbExpression.split(/\s+/);
      let tempCombineValueWithExpShown = '';

      dbExpressionParts.forEach((part: string) => {
        const upperPart = part.toUpperCase();
        if (['(', ')', 'AND', 'OR'].includes(upperPart)) {
          tempCombineValueWithExpShown += ` ${part}`;
        } else {
          const rule = this.rules.find(r => r.eruleId.toString() === part.trim());
          if (rule) {
            tempCombineValueWithExpShown += ` ${rule.expShown}`;
            this.selectedRules.push(rule);
          } else {
            tempCombineValueWithExpShown += ` ${part}`; // fallback
          }
        }
      });

      this.combinevalue = tempCombineValueWithExpShown.trim();

      const lastEditValue = this.sequence[this.sequence.length - 1]?.type;
      this.updateUIState(lastEditValue);
    
    }
    else if (event.action === 'delete') {
      this.deleteCard(event.data.ecardId, event.data.CardName);
    }
    else if (event.action === 'validate') {
      this.validateCardExpression(event.data);
    }
  }

  private validateCardExpression(cardData: any): void {
    const cardExpression = cardData.expression;
    let expressionWithPNames = cardExpression;

    const getRulesList = (): Promise<any[]> => {
      return new Promise((resolve, reject) => {
        this.rulesService.getRulesList().subscribe({
          next: (response) => resolve(response.data as any[]),
          error: (error) => reject(error)
        });
      });
    };

    const getParameters = (): Promise<any[]> => {
      return new Promise((resolve, reject) => {
        this.parameterService.getParameters().subscribe({
          next: (response) => resolve(response.data as any[]),
          error: (error) => reject(error)
        });
      });
    };

    const processValidation = async () => {
      try {
        const rules = await getRulesList();
        const parameters = await getParameters();
        rules.forEach((rule) => {
          if (rule.expShown) {
            const ruleWithoutParentheses = rule.expShown.replace(/^\(|\)$/g, '');
            expressionWithPNames = expressionWithPNames.replaceAll(
              String(rule.eruleId),
              ruleWithoutParentheses
            );
          }
        });

        this.validationDialogService.openValidationDialog({
          actionType: 'exits',
          expshown: expressionWithPNames,
          parameters: parameters,
          expression: '',
          validationType: 'ECard',
          valideeId: cardData.ecardId
        });
      } catch (error: any) {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
      }
    };

    processValidation();
  }

  private updateUIState(lastItemType: string | undefined): void {
    if (!lastItemType) return;

    this.isOpenParenthesis = false;
    this.isRuleDisabled = false;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;

    switch (lastItemType) {
      case 'openParenthesis':
        this.isOpenParenthesis = false;
        this.isRuleDisabled = false;
        this.isLogicalOperator = true;
        this.isCloseParenthesis = true;
        break;
      case 'rule':
        this.isOpenParenthesis = true;
        this.isRuleDisabled = true;
        this.isLogicalOperator = false;
        this.isCloseParenthesis = false;
        break;
      case 'logicalOperator':
        this.isOpenParenthesis = false;
        this.isRuleDisabled = false;
        this.isLogicalOperator = true;
        this.isCloseParenthesis = true;
        break;
      case 'closeParenthesis':
        this.isOpenParenthesis = true;
        this.isRuleDisabled = true;
        this.isLogicalOperator = false;
        this.isCloseParenthesis = false;
        break;
    }
  }
  extractRuleNamesNew(expression: string): string[] {
    let parts = expression.replace(/[()]/g, '').split(/\s+/);
    let result: string[] = [];
    let tempRule = "";

    for (let i = 0; i < parts.length; i++) {
      let part = parts[i];

      // Check if the current part is a logical operator
      if (part.toLowerCase() === "and" || part.toLowerCase() === "or") {
        if (tempRule) {
          result.push(tempRule.trim()); // Push the accumulated rule
          tempRule = "";
        }
        result.push(part); 
      } else {
       
        tempRule += (tempRule ? " " : "") + part;
      }
    }

    
    if (tempRule) {
      result.push(tempRule.trim());
    }

    return result;
  }

  updateExpression(): void {
    this.expshown = this.sequence.map((item) => {
      if (item.type === 'rule') {
        const rule = this.rules.find(r => r.eruleId.toString() === item.value.toString() || r.eruleName === item.value);
        return rule ? ` ${rule.eruleName} ` : `( Unknown )`;
      } else if (item.type === 'logicalOperator') {
        return item.value.toUpperCase();
      }
      return item.value;
    }).join(' ').replace(/\s+/g, ' ').trim();

    this.expressionForm.patchValue({ Expshown: this.expshown });
  }

  // Reset Expression and States
  resetExpression(): void {
    this.sequence = [];
    this.expression = '';
    this.expshown = '';
    this.selectedRules = [];
    this.combinevalue = '';
    this.expressionForm.patchValue({ Expshown: '' });
    this.isOpenParenthesis = false;
    this.isRuleDisabled = false;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;
  }

  removeLastItem(): void {
    if (this.sequence.length > 0) {
      const lastItems = this.sequence[this.sequence.length - 1];
      if (lastItems?.type === 'rule') {
        this.selectedRules = this.selectedRules.filter(rule => rule.eruleName !== lastItems.value);
        // Update combinevalue (Equivalent table)
        const lastExpressionIndex = this.combinevalue.lastIndexOf(this.rules.find(rule => rule.eruleName === lastItems.value)?.expShown || '');
        if (lastExpressionIndex !== -1) {
          this.combinevalue = this.combinevalue.substring(0, lastExpressionIndex).trim();
        }

        // Remove trailing logical operator if needed
        if (this.combinevalue.endsWith("And") || this.combinevalue.endsWith("Or")) {
          this.combinevalue = this.combinevalue.substring(0, this.combinevalue.lastIndexOf(" ")).trim();
        }
      }
      this.sequence.pop();
      this.updateExpression();
      if (this.sequence.length > 0) {
        const lastItem = this.sequence[this.sequence.length - 1];
        if (lastItem.type === 'openParenthesis') {
          this.isOpenParenthesis = false;
          this.isRuleDisabled = false;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
        } else if (lastItem.type === 'rule') {
          this.isOpenParenthesis = true;
          this.isRuleDisabled = true;
          this.isLogicalOperator = false;
          this.isCloseParenthesis = false;
        } else if (lastItem.type === 'logicalOperator') {
          this.isOpenParenthesis = false;
          this.isRuleDisabled = false;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
        } else if (lastItem.type === 'closeParenthesis') {
          this.isOpenParenthesis = true;
          this.isRuleDisabled = true;
          this.isLogicalOperator = false;
          this.isCloseParenthesis = false;
        }


      }
    }
  }

  insertNewRecord(): void {
    this.menuVisible = false;
    this.formVisible = true;
    this.isInsertNewRecord = true;
    this.expressionForm.reset();
    this.resetExpression();
  }

  closeForm(): void {
    this.formVisible = false;
    this.isOpenParenthesis = false;
    this.isRuleDisabled = false;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;
    this.resetExpression();
  }

  toggleMenu(): void {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  applyFilter(): void {
    const term = this.searchTerm.toLowerCase();
    this.filteredCards = this.eCardsListAry.filter((card) => {
      return Object.values(card)
        .some((value: any) => value.toString().toLowerCase().includes(term));
    });
  }

  deleteCard(id: number,cardName:string) {
    const confirmDelete = window.confirm(
      `Are you sure you want to delete the card: "${cardName}"?`
    );
    if (confirmDelete) {
      this.cardService.deleteCard(id).subscribe({
        next: (response) => {
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.fetchCards();
          this.fetchRuleList();
          this.closeForm();
  
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
    
  //parseExpressionToSequence(expression: string): { type: string; value: string }[] {
  //  const sequence: { type: string; value: string }[] = [];
  //  const tokens = expression.split(/\s+/); // Split expression into tokens

  //  let i = 0;
  //  while (i < tokens.length) {
  //    let currentToken = tokens[i];
  //    let nextToken = tokens[i + 1] || null;

  //    // Helper function to check if a token is a logical operator, operator, or parentheses
  //    const isBoundaryToken = (token: string): boolean => {
  //      return (
  //        this.logicalOperator.includes(token) ||
  //        token === ")" ||
  //        token === "("
  //      );
  //    };

  //    if (isBoundaryToken(currentToken)) {
  //      if (this.logicalOperator.includes(currentToken)) {
  //        sequence.push({ type: "logicalOperator", value: currentToken });
  //      } else if (currentToken === "(") {
  //        sequence.push({ type: "openParenthesis", value: currentToken });
  //      } else if (currentToken === ")") {
  //        sequence.push({ type: "closeParenthesis", value: currentToken });
  //      }
  //      i++;
  //      continue;
  //    }

  //    // Merge tokens until we hit a boundary token
  //    while (nextToken && !isBoundaryToken(nextToken)) {
  //      currentToken += ` ${nextToken}`;
  //      i++; // Move to the next token
  //      nextToken = tokens[i + 1] || null; // Update next token
  //    }

  //    // Check if currentToken (merged or not) exists in parameters or factors
  //    const isRule = this.rules.some(param => param.eruleName === currentToken);
  //    if (isRule) {
  //      sequence.push({ type: "rule", value: currentToken });
  //    }
  //    i++;
  //  }

  //  return sequence;
  //}

  parseExpressionToSequence(expression: string): { type: string; value: string }[] {
    const sequence: { type: string; value: string }[] = [];
    const tokens = expression.split(/\s+/);

    let i = 0;
    while (i < tokens.length) {
      let currentToken = tokens[i];
      let nextToken = tokens[i + 1] || null;

      const isBoundaryToken = (token: string): boolean => {
        return (
          this.logicalOperator.includes(token.toUpperCase()) ||
          token === ")" ||
          token === "("
        );
      };

      if (isBoundaryToken(currentToken)) {
        if (this.logicalOperator.includes(currentToken.toUpperCase())) {
          sequence.push({ type: "logicalOperator", value: currentToken.toUpperCase() });
        } else if (currentToken === "(") {
          sequence.push({ type: "openParenthesis", value: currentToken });
        } else if (currentToken === ")") {
          sequence.push({ type: "closeParenthesis", value: currentToken });
        }
        i++;
        continue;
      }
      if (currentToken === "And" || currentToken === "Or") {
        sequence.push({ type: 'logicalOperator', value: currentToken });

      }
      // Check if token is a rule ID
      const matchedRule = this.rules.find(rule => rule.eruleId.toString() === currentToken);
      if (matchedRule) {
        sequence.push({ type: "rule", value: matchedRule.eruleName });
      } else {
        console.warn('Unmatched token in expression sequence:', currentToken);
      }
      i++;
    }

    return sequence;
  }


  downloadTemplate() {
    this.isDownloading = true;
    this.message = "Please wait, template is downloading...";
    this.cardService.downloadTemplate().subscribe((response) => {
      this.isDownloading = false;
      const blob = new Blob([response], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'ECards-Template.xlsm'; // Filename for the download
      a.click();
      window.URL.revokeObjectURL(url);
      this._snackBar.open('Cards Template Download Successfully.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
      });
    });
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      this.importCard(this.selectedFile);
    } else {
      this._snackBar.open('Please select a file first.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
      });
    }
  }

  importCard(selectedFile: File) {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
  });
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    this.cardService.importCard(selectedFile,this.loggedInUser.user.userName).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.fetchCards();
        this.fetchRuleList();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
    });
  }

  // exportCards() {
  //   this.cardService.exportCards().subscribe({
  //     next: (response: Blob) => {
  //       console.log('Blob resp: ', response);
  //       const url = window.URL.createObjectURL(response);
  //       const anchor = document.createElement('a');
  //       anchor.href = url;
  //       anchor.download = 'ECards.xlsx';
  //       document.body.appendChild(anchor);
  //       anchor.click();
  //       document.body.removeChild(anchor);
  //       window.URL.revokeObjectURL(url);
  //       this._snackBar.open('Export ECards Successfully.', 'Okay', {
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

  exportCards() {
    this.cardService.exportCards([...this.tableChild.selectedRowsItem]).subscribe({
      next: (response: Blob) => {
        console.log('Blob resp: ', response);
        const url = window.URL.createObjectURL(response);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'ECards.xlsx';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(url);
        this._snackBar.open('Export ECards Successfully.', 'Okay', {
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

  getParameters = (): Promise<any[]> => {
    return new Promise((resolve, reject) => {
      this.parameterService.getParameters().subscribe({
        next: (response) => resolve(response.data as any[]),
        error: (error) => reject(error)
      });
    });
  };

  openValidatorDialog(expshown: any) {
    //this.updateExpression();


    if (this.expressionForm.invalid) {
      this.snackBar.open('Please fill out all required fields!', 'Close', { duration: 2000 });
      this.expressionForm.markAllAsTouched();
      return;
    }

    this.sequenceExp = this.sequence.map(item => {
      if (item.type === "rule") {
        let matchedParam = this.rules.find(p => p.eruleName === item.value);
        if (matchedParam) {
          return { ...item, value: matchedParam.eruleId };
        }
      }
      return item; // Return unchanged if not a parameter
    });
    const expressionExp = this.sequenceExp.map(item => item.value).join(" ");

    const cardExpression = expressionExp; // Example ( 1234 AND ( 123 or 321 ))
    let expressionWithPNames = cardExpression;

    const getRulesList = (): Promise<any[]> => {
      return new Promise((resolve, reject) => {
        this.rulesService.getRulesList().subscribe({
          next: (response) => resolve(response.data as any[]),
          error: (error) => reject(error)
        });
      });
    };
  
    const getParameters = (): Promise<any[]> => {
      return new Promise((resolve, reject) => {
        this.parameterService.getParameters().subscribe({
          next: (response) => resolve(response.data as any[]),
          error: (error) => reject(error)
        });
      });
    };

    const processValidation = async () => {
      try {
        const rules = await getRulesList();
  
        //rules.forEach((rule) => {
        //  const ruleWithoutParentheses = rule.expShown.replace(/^\(|\)$/g, '');
        //  expressionWithPNames = expressionWithPNames.replaceAll(String(rule.eruleId), ruleWithoutParentheses);
        //});
        rules.forEach((rule) => {
          if (rule.expShown) {
            const ruleWithoutParentheses = rule.expShown.replace(/^\(|\)$/g, '');
            expressionWithPNames = expressionWithPNames.replaceAll(String(rule.eruleId), ruleWithoutParentheses);
          }
        });
  
        const parameters = await getParameters();
  
        this.validationDialogService.openValidationDialog({
          actionType:'form',
          expshown: expressionWithPNames,
          parameters: parameters,
          expression: expressionExp,
          validationType: 'ECard',
          valideeId: 0
        });
      } catch (error: any) {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });
      }
    };
  
    processValidation();
  }
}
