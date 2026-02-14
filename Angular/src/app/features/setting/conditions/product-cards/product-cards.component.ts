import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { ProductCardsService } from '../../../../core/services/setting/product-cards.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ProductsService } from '../../../../core/services/setting/products.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TableComponent } from '../../../../core/components/table/table.component';
import { CardsService } from '../../../../core/services/setting/cards.service';
import { UtilityService } from '../../../../core/services/utility/utils';
import { RulesService } from '../../../../core/services/setting/rules.service';
import { DeleteDialogComponent } from '../../../../core/components/delete-dialog/delete-dialog.component';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { ValidationDialogService } from '../../../../core/services/setting/validation-dialog.service';
import { ParameterService } from '../../../../core/services/setting/parameter.service';
import { PermissionsService } from '../../../../core/services/setting/permission.service';

@Component({
  selector: 'app-product-cards',
  standalone: false,
  templateUrl: './product-cards.component.html',
  styleUrls: ['./product-cards.component.scss'],
})
export class ProductCardsComponent implements OnInit {
  @ViewChild('tableChild') tableChild!: TableComponent;
  eProductCardsListAry: any = [];
  productCardsColumnsAry = ['Select', 'Product Card Name', 'Description', 'Exp shown', 'Product','Created By', 'Updated By', 'Actions'];
  cardsHeaderAry = ['Product Card Name', 'Description', 'Exp shown', 'Product','Created By', 'Updated By'];
  deleteKeyForMultiple: string = 'eproductCardId';
  formVisible: boolean = false;
  isInsertNewRecord: boolean = false;
  searchTerm: string = '';
  menuVisible: boolean = false;
  openParenthesis: any = ['('];
  closeParenthesis: any = [')'];
  function: any = ['And', 'Or'];
  expression = '';
  expshown = '';
  expressionForm: FormGroup;
  updatedIndexId: number = 0;
  private _snackBar = inject(MatSnackBar);
  sequence: { type: string; value: any }[] = [];
  cards: any[] = [];
  productsList: any[] = [];
  productMap = new Map<number, string>();
  @ViewChild('productSelect') productSelect: any;
  @ViewChild('openParenthesisSelect') openParenthesisSelect: any;
  @ViewChild('functionSelect') functionSelect: any;
  @ViewChild('cardsSelect') cardsSelect: any;
  @ViewChild('closeParenthesisSelect') closeParenthesisSelect: any;
  isOpenParenthesis: boolean = false;
  isLogicalOperator: boolean = true;
  isCloseParenthesis: boolean = true
  isCardsDisabled: boolean = false;
  filteredProductCardsList: any = []
  selectedFile: File | null = null;
  selectedCard: { name: string; expshown: string; ruleExpshown:string }[] = [];
  selectedCardName: string = '';
  combinevalue: string = '';
  sequenceExp: { type: string; value: any }[] = [];
  rules: any[] = [];
  selectedRules: { eruleName: string; expShown: string }[] = [];
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = "Loading data, please wait...";
  loggedInUser: any = null;
  createdBy:string = '';
  loading: boolean = false;
  constructor(
    private fb: FormBuilder,
    private productsService: ProductsService,
    private productsCardService: ProductCardsService,
    private cardsService: CardsService,
    private utilityService: UtilityService,
    private ruleService: RulesService,
    private parameterService: ParameterService,
    private validationDialogService: ValidationDialogService,
    private dialog: MatDialog,
    private authService: AuthService,
    private PermissionsService:PermissionsService
  ) {
    this.expressionForm = this.fb.group({
      ProductCardName: ['', [Validators.required]],
      Description: [''],
      //MaximumAmount: ['',[Validators.required]],
      Expshown: ['', [Validators.required]],
      Product: ['', [Validators.required]],
      entityId:['']
    });
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  ngOnInit(): void {
    this.fetchAllProductCards();
    this.fetchCards();
    this.fetchRuleList();
  }

  fetchRuleList(): void {
    this.ruleService.getRulesList().subscribe({
      next: (response) => {
        if (response && response.data) {
          this.rules = response.data;
        }
      },
    });
  }

  fetchAllProductCards() {
    this.isLoading = true;
    this.productsCardService.getProductCardsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          console.log(response.data);

          this.productsService.getInfoListName().subscribe({
            next: (productResponse) => {
              if (productResponse.isSuccess) {
                this.productsList = productResponse.data;

                productResponse.data.forEach((product: { productId: number; productName: string }) => {
                  this.productMap.set(product.productId, product.productName);
                });

                // Sorting the response data
                const sortedData = response.data.sort((a: any, b: any) =>
                  new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
                );

                this.eProductCardsListAry = sortedData.map(
                  (card: { pcardName: any; pcardDesc: any; expshown: any; expression: any; pcardId: any; productId: number; amount: any; createdBy: any; createdByDateTime: any; updatedBy: any; updatedByDateTime: any; entityId: any }) => ({
                    "Product Card Name": card.pcardName,
                    Description: card.pcardDesc,
                    "Exp shown": card.expshown,
                    eproductCardId: card.pcardId,
                    Product: this.productMap.get(card.productId) || 'Unknown',
                    productId: card.productId,
                    "Created By": card.createdBy,
                    "Created Date": card.createdByDateTime,
                    "Updated By": card.updatedBy,
                    "Updated Date": card.updatedByDateTime,
                    "expression": card.expression,
                    "entityId": card.entityId
                  })
                );

                // Initialize filtered list with all data
                this.filteredProductCardsList = [...this.eProductCardsListAry];
                this.isLoading = false;
              }
            },
            error: (error) => {
              console.error('Error fetching product information', error);
              this.isLoading = false;
            },
          });
        }
      },
      error: (error) => {
        console.error('Error fetching product card list', error);
        this.isLoading = false;
      },
    });
  }

  fetchCards(): void {
    this.cardsService.getCardsList().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.cards = response.data.map((card: { ecardId: number; ecardName: string; expshown: string }) => ({
            id: card.ecardId,
            name: card.ecardName.trim(),
            expshown: card.expshown
          }));
        }
      },
      error: (error) => {
        console.error('Error fetching cards list', error);
      },
    });
  }
  private getFullRuleFromECardExpression(expression: string): { expressionWithPIDs: string, expressionWithPNames: string } {
    let expressionWithPNames = expression;
    let expressionWithPIDs = expression;

    this.ruleService.getRulesList().subscribe({
        next: (response) => {
            const rules = response.data;

        console.log(rules)
  

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

  tableRowAction(event: { action: string; data: any }): void {
    if (event.action === 'edit') {
      this.isInsertNewRecord = false;
      this.formVisible = true;
      this.createdBy = event.data.createdBy;
      this.expressionForm.patchValue({
        ProductCardName: event.data["Product Card Name"],
        Description: event.data.Description,
        //MaximumAmount: event.data["Maximum Amount"],
        Expshown: event.data["Exp shown"],
        Product: event.data.productId || 0,
        entityId: event.data.entityId,
      });
      this.selectedCard = [];
      let equivalentExpression = "";
      let ruleExpshownArray: string[] = [];
      let logicalOperator = "";
      this.combinevalue = "";
      this.selectedRules = [];
      const expressionPart = this.extractRuleNamesNew(event.data["Exp shown"]);
      this.sequence = this.parseExpressionToSequence(event.data["Exp shown"]);

      expressionPart.forEach((part: string, index: number) => {
        if (part === 'And' || part === 'Or') {
          logicalOperator = part;
        }
        const selectedCardsObject = this.cards.find(rule => rule.name === part);
        if (selectedCardsObject) {
          let logicalOperator = this.sequence.length > 1 ? this.sequence[this.sequence.length - 2].value : '';
          const expressionParts = this.extractRuleNamesNew(selectedCardsObject.expshown);
  
          if(expressionParts){
            expressionParts.forEach((part: string, index: number) => {
              const selectedRuleObject = this.rules
                .filter(rule => rule.eruleName === part)
                .sort((a, b) => b.version - a.version)[0];

              if (part === 'And' || part === 'Or') {
                if (ruleExpshownArray.length > 0) {
                  ruleExpshownArray.push(part);
                }
              } else {
                if (selectedRuleObject) {
                  const exists = this.selectedRules.find(rule => rule.eruleName === selectedRuleObject.eruleName);
                  if (!exists) {
                    selectedRuleObject.ruleExpshown = selectedRuleObject.expShown; // Store the rule's expression
                    this.selectedRules.push(selectedRuleObject);
                  }
                  ruleExpshownArray.push(selectedRuleObject.expShown);
                }
              }
            });
          }
          
  
          const exists = this.selectedCard.find(rule => rule.name === this.selectedCardName);
          if (!exists) {
            selectedCardsObject.ruleExpshown = ruleExpshownArray.join(" ");
            this.selectedCard.push(selectedCardsObject);
          }
  
          equivalentExpression = ruleExpshownArray.join(" ");
          if (this.combinevalue) {
            this.combinevalue += ` ${logicalOperator} { ${equivalentExpression} }`;
          } else {
            this.combinevalue = `{ ${equivalentExpression} }`;
          }
        }
        ruleExpshownArray = [];
      });

      this.updatedIndexId = event.data.eproductCardId;

      let lasteditvalue = this.sequence[this.sequence.length - 1].type;
      if (lasteditvalue === 'openParenthesis') {
        this.isOpenParenthesis = false;
        this.isCardsDisabled = false;
        this.isLogicalOperator = true;
        this.isCloseParenthesis = true;
      } else if (lasteditvalue === 'rule') {
        this.isOpenParenthesis = true;
        this.isCardsDisabled = true;
        this.isLogicalOperator = false;
        this.isCloseParenthesis = false;
      } else if (lasteditvalue === 'logicalOperator') {
        this.isOpenParenthesis = false;
        this.isCardsDisabled = false;
        this.isLogicalOperator = true;
        this.isCloseParenthesis = true;
      } else if (lasteditvalue === 'closeParenthesis') {
        this.isOpenParenthesis = true;
        this.isCardsDisabled = true;
        this.isLogicalOperator = false;
        this.isCloseParenthesis = false;
      }
      this.updateExpression();
    } else if (event.action === 'delete') {
      this.deleteProductCard(event.data.eproductCardId, event.data["Stream Card Name"]);
    }
    else if (event.action === 'validate') {

      const getCardsList = () => {
        return new Promise((resolve, reject) => {
          this.cardsService.getCardsList().subscribe({
            next: (response) => resolve(response.data),
            error: (error) => reject(error)
          });
        });
      };

      const getRulesList = () => {
        return new Promise((resolve, reject) => {
          this.ruleService.getRulesList().subscribe({
            next: (response) => resolve(response.data),
            error: (error) => reject(error)
          });
        });
      };

      const getParameters = () => {
        return new Promise((resolve, reject) => {
          this.parameterService.getParameters().subscribe({
            next: (response) => resolve(response.data),
            error: (error) => reject(error)
          });
        });
      };

      const processValidation = async () => {
        try {
          const eCards: any = await getCardsList();
          const rules: any = await getRulesList();
          const parameters: any = await getParameters();

          let selectedPCardExpression = event.data.expression;

          eCards.forEach((eCard: any) => {
            let eCardExpressionWithPNames = eCard.expression;

            rules.forEach((rule: any) => {
              const ruleWithoutParentheses = rule.expShown.replace(/^\(|\)$/g, '');
              eCardExpressionWithPNames = eCardExpressionWithPNames.replaceAll(String(rule.eruleId), ruleWithoutParentheses);
            });

            selectedPCardExpression = selectedPCardExpression.replaceAll(eCard.ecardId, eCardExpressionWithPNames);
          });

          this.validationDialogService.openValidationDialog({
            actionType: 'exits',
            expshown: selectedPCardExpression,
            expression: '',
            parameters: parameters,
            validationType: 'PCard',
            valideeId: event.data.eproductCardId
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
        result.push(part); // Push the logical operator
      } else {
        // Accumulate multi-word rule names
        tempRule += (tempRule ? " " : "") + part;
      }
    }

    // Add any remaining rule name at the end
    if (tempRule) {
      result.push(tempRule.trim());
    }

    return result;
  }

  extractRuleNames(expression: string): string[] {
    return expression.replace(/[()]/g, '').split(/\s+/);
  }

  insertNewRecord() {
    this.menuVisible = !this.menuVisible;
    this.isInsertNewRecord = true;
    this.combinevalue = '';
    this.isOpenParenthesis = false;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true
    this.isCardsDisabled = false;
    this.selectedCard = [];
    this.expressionForm.reset();
    this.sequence = [];
    this.expression = '';
    this.expressionForm.reset({ Expression: '' });
    this.formVisible = true;

  }

  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  cancelEvent() {
    this.formVisible = false;
    this.resetExpression();
  }

  submit() {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    if (this.expressionForm.valid) {
      this.sequenceExp = this.sequence.map(item => {
        if (item.type === "cards") {
          let matchedParam = this.cards.find(p => p.name === item.value);
          if (matchedParam) {
            return { ...item, value: matchedParam.id };
          }
        }
        return item; // Return unchanged if not a parameter
      });
      const expressionExp = this.sequenceExp.map(item => item.value).join(" ");
      const payload = {
        pcardId: this.isInsertNewRecord ? 0 : this.updatedIndexId,
        pcardName: this.expressionForm.value.ProductCardName,
        pcardDesc: this.expressionForm.value.Description,
        amount : this.expressionForm.value.MaximumAmount,
        expression: expressionExp,
        entityId: null,
        productId: +this.expressionForm.get('Product')?.value || 0,
        expshown: this.expressionForm.value.Expshown,
        pstatus: 'Active',
      };

      if (this.isInsertNewRecord) {
        this.addNewProductCard(payload);
      } else {
        this.updateProductCard(payload);
      }
    } else {
      this.expressionForm.markAllAsTouched();
    }
  }

  validateValueInput(event: KeyboardEvent | ClipboardEvent): void {
    let isValidInput = false;
    if (event instanceof KeyboardEvent || event instanceof ClipboardEvent) {
      const pastedText = event instanceof ClipboardEvent ? event.clipboardData?.getData('text') : undefined;
      if (event instanceof KeyboardEvent) {
        const charCode = event.key.charCodeAt(0);
        isValidInput = charCode >= 48 && charCode <= 57; // Check if it's a numeric key
      } else if (event instanceof ClipboardEvent) {
        isValidInput = pastedText ? /^\d+$/.test(pastedText) : false;
      }
    }

    if (!isValidInput) {
      event.preventDefault();
    }
  }

  onPasteValue(event: ClipboardEvent): void {
    this.validateValueInput(event);
  }

  onSelect(type: string, event: any): void {
    const value = event.target.value;
    if (!value) return;
    this.sequence.push({ type, value });

    // Logic to enable/disable fields
    if (type === 'cards') {
      this.isCardsDisabled = true;
      this.isLogicalOperator = false;
      this.isCloseParenthesis = false;
      this.isOpenParenthesis = true;
      this.selectedCardName = event.target.value;
      let equivalentExpression = "";
      let ruleExpshownArray: string[] = [];

      const selectedCardsObject = this.cards.find(rule => rule.name === this.selectedCardName);
      if (selectedCardsObject) {
        let logicalOperator = this.sequence.length > 1 ? this.sequence[this.sequence.length - 2].value : '';
        const expressionParts = this.extractRuleNamesNew(selectedCardsObject.expshown);

        if (expressionParts) {
          expressionParts.forEach((part: string, index: number) => {
            // CHANGE HERE: Replace .find() with filter and sort to get highest version
            if (part === 'And' || part === 'Or') {
              if (ruleExpshownArray.length > 0) {
                ruleExpshownArray.push(part);
              }
            } else {
              // NEW CODE: Get all matching rules and select the highest version
              const matchingRules = this.rules.filter(rule => rule.eruleName === part);
              if (matchingRules.length > 0) {
                // Sort by version descending and take the first one (highest version)
                const selectedRuleObject = matchingRules.sort((a, b) => b.version - a.version)[0];

                const exists = this.selectedRules.find(rule => rule.eruleName === selectedRuleObject.eruleName);
                if (!exists) {
                  selectedRuleObject.ruleExpshown = selectedRuleObject.expShown; // Store the rule's expression
                  this.selectedRules.push(selectedRuleObject);
                }
                ruleExpshownArray.push(selectedRuleObject.expShown);
              }
            }
          });
        }


        const exists = this.selectedCard.find(rule => rule.name === this.selectedCardName);
        if (!exists) {
          selectedCardsObject.ruleExpshown = ruleExpshownArray.join(" ");
          this.selectedCard.push(selectedCardsObject);
        }

        equivalentExpression = ruleExpshownArray.join(" ");
        if (this.combinevalue) {
          this.combinevalue += ` ${logicalOperator} { ${equivalentExpression} }`;
        } else {
          this.combinevalue = `{ ${equivalentExpression} }`;
        }
      }

    } else if (type === 'function') {
      this.isOpenParenthesis = false;
      this.isCardsDisabled = false;
      this.isLogicalOperator = true;
      this.isCloseParenthesis = true;
    }
    // this.validateDynamicControls(type);
    event.target.value = '';
    this.updateExpression();
  }
  validateDynamicControls(type: string): void {
    const openParenthesisControl = this.expressionForm.get('openParenthesis');
    const cardsControl = this.expressionForm.get('cards');
    const functionControl = this.expressionForm.get('function');
    const closeParenthesisControl = this.expressionForm.get('closeParenthesis');

    if (!cardsControl?.value && !functionControl?.value) {
      openParenthesisControl?.enable();
      cardsControl?.enable();
      functionControl?.disable();
      closeParenthesisControl?.disable();
      return;
    }
    if (type == 'cards') {
      openParenthesisControl?.disable();
      cardsControl?.disable();
      functionControl?.enable();
      closeParenthesisControl?.enable();
      return;
    }
    if (type == 'function') {
      openParenthesisControl?.enable();
      cardsControl?.enable();
      functionControl?.disable();
      closeParenthesisControl?.disable();
      return;
    }
  }

  updateExpression(): void {
    this.expshown = this.sequence.map((item) => item.value).join(' ');
    this.expressionForm.patchValue({ Expshown: this.expshown });
  }

  resetExpression(): void {
    this.sequence = [];
    this.expression = '';
    this.expressionForm.reset({ Expression: '' });
    this.isOpenParenthesis = false;
    this.combinevalue = '';
    this.selectedCard = [];
    this.isCardsDisabled = false;
    this.isLogicalOperator = true;
    this.isCloseParenthesis = true;
    // this.validatesDynamicControls('');
  }

  parseExpressionToSequence(expression: string): { type: string; value: string }[] {
    const sequence: { type: string; value: string }[] = [];
    const tokens = expression.split(/\s+/); // Split expression into tokens

    let i = 0;
    while (i < tokens.length) {
      let currentToken = tokens[i];
      let nextToken = tokens[i + 1] || null;

      // Helper function to check if a token is a logical operator, operator, or parentheses
      const isBoundaryToken = (token: string): boolean => {
        return (
          this.function.includes(token) ||
          token === ")" ||
          token === "("
        );
      };

      if (isBoundaryToken(currentToken)) {
        if (this.function.includes(currentToken)) {
          sequence.push({ type: "function", value: currentToken });
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
      const isRule = this.cards.some(param => param.name === currentToken);
      if (isRule) {
        sequence.push({ type: "cards", value: currentToken });
      }
      i++;
    }

    return sequence;
  }

  toggleFormVisibility() {
    this.formVisible = !this.formVisible;
    this.expressionForm.reset();
  }

  validateFormField(controlName: string) {
    return this.expressionForm.get(controlName)?.invalid && this.expressionForm.get(controlName)?.touched;
  }

  removeLastItem(): void {
    if (this.sequence.length > 0) {
      let removedItem = this.sequence[this.sequence.length - 1];
      const lastItem = this.sequence[this.sequence.length - 2];
      this.sequence.pop();
      this.updateExpression();
      if (removedItem.type === 'cards') {
          this.selectedCard = this.selectedCard.filter(rule => rule.name !== removedItem.value);
          let newEquivalentExpression = "";
          this.selectedCard.forEach((card, index) => {
            const ruleExpshownArray = this.extractRuleNames(card.expshown)
              .map(part => part === 'And' || part === 'Or' ? part : this.rules.find(rule => rule.eruleName === part)?.expShown || part);

            if (index > 0) {
              newEquivalentExpression += ` And { ${ruleExpshownArray.join(' ')} }`;
            } else {
              newEquivalentExpression = `{ ${ruleExpshownArray.join(' ')} }`;
            }
          });

          // Update combinevalue
          this.combinevalue = newEquivalentExpression.trim();
      }


      if (this.sequence.length > 0) {

        if (lastItem.type === 'openParenthesis') {
          this.isOpenParenthesis = false;
          this.isCardsDisabled = false;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
        } else if (lastItem.type === 'cards') {
          this.isOpenParenthesis = true;
          this.isCardsDisabled = true;
          this.isLogicalOperator = false;
          this.isCloseParenthesis = false;
        } else if (lastItem.type === 'function') {
          this.isOpenParenthesis = false;
          this.isCardsDisabled = false;
          this.isLogicalOperator = true;
          this.isCloseParenthesis = true;
        } else if (lastItem.type === 'closeParenthesis') {
          this.isOpenParenthesis = true;
          this.isCardsDisabled = true;
          this.isLogicalOperator = false;
          this.isCloseParenthesis = false;
        }
      }
    }
  }

  addNewProductCard(payload: any) {
    // payload.createdBy = this.loggedInUser.user.userName;
    // payload.updatedBy = this.loggedInUser.user.userName;
    this.productsCardService.addProductCard(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        if (response.isSuccess) { 
        this.fetchAllProductCards();
        this.toggleFormVisibility();
        this.formVisible = false;
      }
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  updateProductCard(payload: any) {
   
    // payload.updatedBy = this.loggedInUser.user.userName;
    this.productsCardService.updateProductCard(payload).subscribe({
      next: (response) => {
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        if (response.isSuccess) {
          this.fetchAllProductCards();
          this.formVisible = false;
        }
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

 deleteProductCard(productCardId: number, productCard: string) {

  const dialogRef = this.dialog.open(DeleteDialogComponent, {
    data: {
      title: 'Confirm',
      message: `Are you sure you want to delete the Product Card: "${productCard}"?`
    }
  });

  dialogRef.afterClosed().subscribe(result => {

    if (!result?.delete) return;

    this.productsCardService.deleteProductCard(productCardId).subscribe({
      next: (response) => {

        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
          duration: 3000
        });

        if (response.isSuccess) {
          this.formVisible = false;
          this.fetchAllProductCards();
        }

      },
      error: (error) => {
        this._snackBar.open(
          error?.error?.message || error.message || 'Something went wrong.',
          'Okay',
          {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          }
        );
      }
    });

  });
}

  deleteCheckedMultipleCards() {
    let listOfSelectedIds = new Set(this.tableChild.selectedRowsItem)
    if (listOfSelectedIds.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);
    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.productsCardService.deleteMultipleProductCard([...listOfSelectedIds]).subscribe({
          next: (response) => {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.fetchAllProductCards();
          },
          error: (error) => {
            this._snackBar.open(error, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top',
              duration: 3000
            });
          }
        })
      }
    });
  }

  exportPCards() {
    let listOfSelectedIds = new Set(this.tableChild.selectedRowsItem)
    this.productsCardService.exportPCards([...listOfSelectedIds]).subscribe({
      next: (response: Blob) => {
        console.log('Blob resp: ', response);
        const url = window.URL.createObjectURL(response);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'ProductCards.xlsx';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      }
    });
  }

  applyFilter() {
    const term = (this.searchTerm || '').toLowerCase().trim();
    console.log('Searching for:', term);

    if (!this.eProductCardsListAry || this.eProductCardsListAry.length === 0) {
      console.log('No data available to filter');
      this.filteredProductCardsList = [];
      return;
    }

    if (!term) {
      this.filteredProductCardsList = [...this.eProductCardsListAry];
      return;
    }

    this.filteredProductCardsList = this.eProductCardsListAry.filter((card: any) => {
      return Object.values(card).some((value: any) => {
        if (value === null || value === undefined) return false;

        const stringValue = value.toString().toLowerCase();
        const containsTerm = stringValue.includes(term);

        return containsTerm;
      });
    });

  }

  downloadTemplate() {
    this.isDownloading = true;
      this.message = "Please wait, template is downloading...";
    this.productsCardService.downloadTemplate().subscribe((response) => {
      this.isDownloading = false;
      const blob = new Blob([response], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'ProductCards-Template.xlsm'; // Filename for the download
      a.click();
      window.URL.revokeObjectURL(url);
      this._snackBar.open('Product Cards Template Download Successfully.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
      });
    });
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      this.importPCard(this.selectedFile);
    } else {
      this._snackBar.open('Please select a file first.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
      });
    }
  }

  importPCard(selectedFile: File) {
  //   this.authService.currentUser$.subscribe((user) => {
  //     this.loggedInUser = user;
  // });
    // this.createdBy = this.loggedInUser.user.userName;
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    this.productsCardService.importPCard(selectedFile).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.fetchAllProductCards();
        this.fetchCards();
        this._snackBar.open(response.message, 'Okay', {
          duration: 4000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(error.message, 'Okay', {
          duration: 4000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
    });
  }

  openValidatorDialog(expshown: any) {
    
    this.sequenceExp = this.sequence.map(item => {
      if (item.type === "cards") {
        let matchedParam = this.cards.find(p => p.name === item.value);
        if (matchedParam) {
          return { ...item, value: matchedParam.id };
        }
      }
      return item; // Return unchanged if not a parameter
    });
    const expressionExp = this.sequenceExp.map(item => item.value).join(" ");

    const getCardsList = () => {
      return new Promise((resolve, reject) => {
        this.cardsService.getCardsList().subscribe({
          next: (response) => resolve(response.data),
          error: (error) => reject(error)
        });
      });
    };
  
    const getRulesList = () => {
      return new Promise((resolve, reject) => {
        this.ruleService.getRulesList().subscribe({
          next: (response) => resolve(response.data),
          error: (error) => reject(error)
        });
      });
    };
  
    const getParameters = () => {
      return new Promise((resolve, reject) => {
        this.parameterService.getParameters().subscribe({
          next: (response) => resolve(response.data),
          error: (error) => reject(error)
   });
      });
    };
  
    const processValidation = async () => {
      try {
        const eCards: any = await getCardsList();
        const rules: any = await getRulesList();
        const parameters: any = await getParameters();
  
        let selectedPCardExpression = expressionExp;
  
        eCards.forEach((eCard: any) => {
          let eCardExpressionWithPNames = eCard.expression;
          rules.forEach((rule: any) => {
            if (rule.expShown != null) {
              const ruleWithoutParentheses = rule.expShown.replace(/^\(|\)$/g, '');
              eCardExpressionWithPNames = eCardExpressionWithPNames.replaceAll(String(rule.eruleId), ruleWithoutParentheses);
            }
});

          selectedPCardExpression = selectedPCardExpression.replaceAll(eCard.ecardId, eCardExpressionWithPNames);
        });
  
        this.validationDialogService.openValidationDialog({
          actionType:'form',
          expshown: selectedPCardExpression,
          expression: expressionExp,
          parameters: parameters,
          validationType: 'PCard',
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



