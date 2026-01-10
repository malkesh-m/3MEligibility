import { ChangeDetectorRef, Component, EventEmitter, inject, Input, Output, SimpleChanges, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductsService } from '../../../../core/services/setting/products.service';
import { TableComponent } from '../../../../core/components/table/table.component';
import { FactorsService } from '../../../../core/services/setting/factors.service';
import { EntityService } from '../../../../core/services/setting/entity.service';
import { DeleteDialogComponent } from '../../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { UtilityService } from '../../../../core/services/utility/utils';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { ExceptionManagementService } from '../../../../core/services/setting/exception-management.service';

@Component({
  selector: 'app-product-list',
  standalone: false,
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss'
})
export class ProductListComponent {
  @ViewChild('tableChild') tableChild!: TableComponent;
  @Input() columnsDisplayAry: any = [];
  @Input() headerDisplayAry: any = [];
  @Input() listAry: any = [];
  @Input() currentTab: string = '';
  @Input() categoryList: any = [];
  @Input() productList: any = [];

  @Output() updateTableRows = new EventEmitter<any>()
  @Output() addFormData = new EventEmitter<any>()
  @Input() editRoleId: number = 0;
  @Input() deleteRoleId: number = 0;
  ProductListDetails: any = [];
  parametersList: any = [];
  parametersValueList: any = [];
  rows: any = [];
  formData!: FormGroup;
  formVisible: boolean = false;
  updatedIndexId: number = 0;
  deleteKeyForMultiple: string = '';
  isInsertNewRecord: boolean = false;
  requiredAry = [{ 'label': 'Yes', value: true }, { 'label': 'No', value: false }]
  private _snackBar = inject(MatSnackBar);
  selectedRows: Set<number> = new Set();
  entitiesList: { entityId: number; entityName: string }[] = [];
  imagePreview: string = '';
  loggedInUser: any = null;
  isExceptionRule: boolean = false;
  isDataReady: boolean = false;
  isLoading: boolean = false;
  private editingItem: any = null;
  private formBackup: any = null;
  isEditing = false;
  private isInitialEdit: boolean = true;


  // exceptions : any[]= [];
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

  constructor(private fb: FormBuilder,private exceptionsService: ExceptionManagementService,private authService: AuthService, private cdrf: ChangeDetectorRef, private productService: ProductsService, private factorsService: FactorsService, private entityService: EntityService, private dialog: MatDialog, private utilityService: UtilityService) { }

  ngOnInit() {
    if(this.currentTab == 'Info') {
      // this.fetchAllExceptions();
    }
    if (this.currentTab == 'Details') {
      this.fetchProductList();
    }
  }
  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      const file = input.files[0];
      const mimeType = file.type;
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = () => {
          const base64String = reader.result as string;
          const base64WithoutPrefix = base64String.split(',')[1];
          this.formData.patchValue({
            productImage: base64WithoutPrefix,
            mimeType: mimeType
          });
          this.imagePreview = base64String;
        };

        reader.readAsDataURL(file);
      } else {
        alert('Please select a valid image file.');
      }
    }
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  fetchEntitiesList() {
    this.entityService.getEntitiesList().subscribe({
      next: (response) => {
        this.entitiesList = response.data.map((entity: any) => ({
          entityId: entity.entityId,
          entityName: entity.entityName,
        }));
      },
      error: (err) => {
        console.error('Error fetching entities list:', err);
      }
    });
  }
  fetchProductList() {
    this.productService.getInfoListName().subscribe({
      next: (response) => {
        this.ProductListDetails = response.data;
        console.log(response.data)
        console.log("response.datafetchProductList")

      },
      error: (err) => {
        console.error('Error fetching entities list:', err);
      }
    });
  }

  addNewRecord() {
    this.imagePreview = '';
    this.formVisible = true;
    this.isInsertNewRecord = true;
    this.isEditing = false;
    this.editingItem = null;
    this.formData.reset();
  }

  tableRowAction(event: { action: string; data: any }): void {
    if (event.action === 'edit') {
      this.editingItem = event.data;
      this.isEditing = true;
      this.isInsertNewRecord = false;

      if (this.isInitialEdit) {
        this.formBackup = { ...this.formData.value };
        this.isInitialEdit = false;
      }

      if (this.currentTab === 'Category') {
        this.updatedIndexId = event.data.CategoryId;
        this.formData.setValue({
          categoryName: event.data.Name,
          catDescription: event.data.Description,
          entityId: event.data.entityId || ''
        });
      } else if (this.currentTab === 'Info') {
        this.updatedIndexId = event.data.ProductId;
        const hasException = !!event.data.ExceptionId;
        const base64String = `data:${event.data.MimeType};base64,${event.data.ProductImage}`;
        this.formData.patchValue({
          code: event.data.Code,
          productName: event.data["Stream Name"],
          productCategory: event.data.Category,
          productImage: event.data.ProductImage,
          narrative: event.data.Narrative,
          description: event.data.Description,
          entityId: event.data.EntityId || "",
          mimeType: event.data.MimeType,
          isExceptionRule: hasException,
          exceptionId: hasException ? event.data.ExceptionId : null,
          maxEligibleAmount: event.data['Stream Cap'] ?? ''
        });

        if (hasException) {
          this.formData.get('exceptionId')?.enable();
          this.formData.get('exceptionId')?.setValidators(Validators.required);
        } else {
          this.formData.get('exceptionId')?.disable();
          this.formData.get('exceptionId')?.clearValidators();
        }
        this.formData.get('exceptionId')?.updateValueAndValidity();
        this.imagePreview = base64String;
      } else {
        this.onSelectProductItem({ target: { value: event.data.ProductId } });
        setTimeout(() => {
          this.onSelectParameterItem({ target: { value: event.data.ParameterId } });
          this.formData.patchValue({
            product: event.data.ProductId,
            parameter: event.data.ParameterId,
            parameterValue: event.data['Parameter value'],
            displayOrder: event.data['Display order'],
            isrequired: event.data.IsRequired,
            entityId: event.data.EntityId || "",
          });
        }, 500);
      }
      this.formVisible = true;
    } else if (event.action === 'delete') {
      if (this.currentTab === 'Category') {
        this.deleteCategoryWithId(event.data.CategoryId, event.data.Name);
      } else if (this.currentTab === 'Info') {
        this.deleteInfoWithId(event.data.ProductId, event.data["Stream Name"]);
      } else {
        this.deleteProductDetailWithId(event.data.ProductId, event.data.ParameterId, event.data.Product);
      }
    }
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['currentTab']) {
      this.initForm();
      this.isInitialEdit = true;
    }

    if (changes['listAry'] && this.listAry?.length > 0) {
      this.updateRows();
    }
    this.cdrf.detectChanges();
  } s() {
    if (this.currentTab === 'Category') {
      this.deleteKeyForMultiple = 'CategoryId';
      this.formData = this.fb.group({
        categoryName: ['', Validators.required],
        catDescription: ['', Validators.required],
        entityId: ['']
      });
    } else if (this.currentTab === 'Info') {
      this.deleteKeyForMultiple = 'ProductId';
      if (!this.formData) {
        this.formData = this.fb.group({
          code: ['', Validators.required],
          productName: ['', Validators.required],
          productCategory: ['', Validators.required],
          productImage: [''],
          narrative: [''],
          description: ['', [Validators.maxLength(50)]],
          entityId: [''],
          mimeType: [''],
          exceptionId: new FormControl({ value: '', disabled: true }),
          isExceptionRule: [false],
          maxEligibleAmount: ['', [Validators.required, Validators.pattern('^[0-9]+$')]]
        });
      }
    } else {
      this.deleteKeyForMultiple = 'ProductId';
      this.formData = this.fb.group({
        product: [null, Validators.required],
        parameter: [null, Validators.required],
        parameterValue: [null],
        displayOrder: ['', [Validators.required]],
        isrequired: [null, Validators.required],
        entityId: ['']
      });
    }

    if (this.listAry?.length > 0) {
      if (this.currentTab === 'Category') {
        this.rows = this.listAry.map((res: any) => ({
          "Name": res.categoryName,
          "Description": res.catDescription,
          "CategoryId": res.categoryId,
          "entityId": res.entityId,
          "Created By": res.createdBy,
          "Created Date": res.createdByDateTime,
          "Updated By": res.updatedBy,
          "Updated Date": res.updatedByDateTime
        }));
      } else if (this.currentTab === 'Info') {
        this.isDataReady = false;
        this.rows = this.listAry.map((res: any) => ({
          "Stream Name": res.productName,
          "Code": res.code,
          "Category": res.categoryId,
          "Category Name": res.categoryName,
          "ProductImage": res.productImage,
          "Narrative": res.narrative,
          "Description": res.description,
          "ProductId": res.productId,
          "EntityId": res.entityId,
          "MimeType": res.mimeType,
          "Created By": res.createdBy,
          "Created Date": res.createdByDateTime,
          "Updated By": res.updatedBy,
          "Updated Date": res.updatedByDateTime,
          "ExceptionId": res?.exceptionId,
          "Stream Cap": res.maxEligibleAmount,
        }));
        this.isDataReady = true;
      } else {
        this.rows = this.listAry.map((res: any) => ({
          "Product": res.productName,
          "ProductId": res.productId,
          "ParameterId": res.parameterId,
          "Parameter": res.parameterName,
          "Parameter value": res.paramValue,
          "Display order": res.displayOrder,
          "IsRequired": res.isRequired,
          "EntityId": res.entityId,
          "Created By": res.createdBy,
          "Created Date": res.createdByDateTime,
          "Updated By": res.updatedBy,
          "Updated Date": res.updatedByDateTime
        }));
      }
    }

    this.cdrf.detectChanges();
  }

  loadTabData(searchTerm: string, action?: string) {
    if (this.tableChild) {
      if (action && action == 'delete') {
        this.rows = this.listAry
      }
      this.tableChild.applyFilter(searchTerm);

    }
  }

  /**
   * Category tab crud events
   */
  addCategoryDetails(payload: { categoryName: string, catDescription: string, categoryId: number, entityId: number, createdBy: string, updatedBy: string }) {
    payload.createdBy = this.loggedInUser.user.userName;
    payload.updatedBy = this.loggedInUser.user.userName;
    payload.categoryId = 0;

    this.productService.addCategories(payload).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.cancelEvent();
          this.updateTableRows.emit({ event: 'CategoryList', action: '' })
        }
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  updateCategoryDetails(payload: { categoryName: string, catDescription: string, categoryId: number, entityId: number, updatedBy: string }) {
    payload.updatedBy = this.loggedInUser.user.userName;
    this.productService.updateCategoriesDetails(payload).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.cancelEvent();
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });

          this.updateTableRows.emit({ event: 'CategoryList', action: '' })
        }
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  deleteCategoryWithId(id: number, categoryName: string) {
    const confirmDelete = window.confirm(
      `Are you sure you want to delete the category: "${categoryName}"? and all its related data?`
    );

    if (confirmDelete) {
      this.productService.deleteCategoryWithId(id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            console.log("response ", response)
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });

            this.updateTableRows.emit({ event: 'CategoryList', action: 'delete' })
          }
          if (!response.isSuccess) {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      })
    }
  }

  deleteCategoryWithMultiple(payload: any) {
    if (this.tableChild.selectedRows.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.productService.deleteMultipleCategories(payload).subscribe({
          next: (response: any) => {
            if (response.isSuccess) {
              this._snackBar.open(response.message, 'Okay', {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
              this.updateTableRows.emit({ event: 'CategoryList', action: 'delete' })
            }
          },
          error: (error) => {
            this._snackBar.open(error.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      }
    });
  }

  /**
   * Info tab crud events
   */
  addInfoDetails(payload: any) {
    console.log(payload)
    console.log("payload")

    payload.createdBy = this.loggedInUser.user.userName;
    payload.updatedBy = this.loggedInUser.user.userName;
    this.productService.addCategoriesInfo(payload).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.cancelEvent();
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.updateTableRows.emit({ event: 'InfoList', action: '' })
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

  updateInfoDetails(payload: any) {
    console.log(payload)
    console.log("payload")
    payload.updatedBy = this.loggedInUser.user.userName;
    this.productService.updateCategoriesInfo(payload).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.cancelEvent();
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.updateTableRows.emit({ event: 'InfoList', action: '' })
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

  deleteInfoWithId(id: number, productName: string) {
    const confirmDelete = window.confirm(
      `Are you sure you want to delete the Stream: "${productName}"?`
    );

    if (confirmDelete) {
      this.productService.deleteCategoryInfoWithId(id).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.updateTableRows.emit({ event: 'InfoList', action: 'delete' })
          }
        },
        error: (error) => {
          console.log("error ", error)
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      })
    }
  }

  deleteMultipleInfoItem(payload: any) {
    if (this.tableChild.selectedRows.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.productService.deleteMultipleInfoItems(payload).subscribe({
          next: (response: any) => {
            if (response.isSuccess) {
              this._snackBar.open(response.message, 'Okay', {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
              this.updateTableRows.emit({ event: 'InfoList', action: 'delete' })
            }
          },
          error: (error) => {
            this._snackBar.open(error.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        })
      }
    });
  }

  /**
   * Detail tab crud events
   */

  addProductDetails(payload: any) {
    payload.createdBy = this.loggedInUser.user.userName;
    payload.updatedBy = this.loggedInUser.user.userName;
    this.productService.addProductDetails(payload).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.cancelEvent();
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.updateTableRows.emit({ event: 'DetailList', action: '' })
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

  updateProductDetails(payload: any) {
    payload.updatedBy = this.loggedInUser.user.userName;
    this.productService.updateProductDetails(payload).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.cancelEvent();
          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          this.updateTableRows.emit({ event: 'DetailList', action: '' })
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

  deleteProductDetailWithId(productId: number, paramId: number, product: string) {
    const confirmDelete = window.confirm(
      `Are you sure you want to delete the product: "${product}"?`
    );

    if (confirmDelete) {
      this.productService.deleteProductDetailWithId(productId, paramId).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
            this.updateTableRows.emit({ event: 'DetailList', action: 'delete' })
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
  }

  deleteMultipleDetails(payload: any) {
    if (this.tableChild.selectedRows.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        this.productService.deleteMultipleDetails(payload).subscribe({
          next: (response: any) => {
            if (response.isSuccess) {
              this.updateTableRows.emit({ event: 'DetailList', action: 'delete' })
            }
            this._snackBar.open(response.message, 'Okay', {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
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

  fetchParameterListOnProduct(productId: number) {
    this.factorsService.getParametersList().subscribe({
      next: (response) => {
        if (response.data && response.data.length > 0) {
          this.parametersList = response.data
          if (this.formData.value.parameter) {
            this.formData.get('parameter')?.setValue(this.formData.value.parameter);
          }
        } else {
          this.parametersList = [];
          this.formData.get('parameter')?.setValue(null);
        }
        this.cdrf.detectChanges();
      },
      error: (error) => {
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        setTimeout(() => {
          this.parametersList = [];
          this.formData.get('parameter')?.setValue(null);
          this.cdrf.detectChanges();
        }, 500);
      }
    })
  }

  fetchParameterValueListOnParameter(parameterId: number) {
    this.productService.getParameterValueByParamterId(parameterId).subscribe({
      next: (response) => {
        this.parametersValueList = response.data
      },
      error: (error) => {
        console.log("error ", error)
        this._snackBar.open(error, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }
  cancelEvent(): void {
    if (this.isEditing) {
      this.formData.patchValue(this.formBackup);
      this.formVisible = false; 
    } else {
      this.formData.reset();
      this.formVisible = false;
    }
    this.isInsertNewRecord = !this.isEditing;
  }

  onSelectProductItem(value: any) {
    this.bindEntityId(value, 'Details');
    this.fetchParameterListOnProduct(value.target.value)
  }

  onSelectParameterItem(value: any) {
    this.fetchParameterValueListOnParameter(value.target.value);
  }

  deleteMultiple() {
    let listOfSelectedIds = new Set(this.tableChild.selectedRowsItem);
    if (this.currentTab === 'Category') {
      this.deleteCategoryWithMultiple([...listOfSelectedIds]);
    } else if (this.currentTab === 'Info') {
      this.deleteMultipleInfoItem([...listOfSelectedIds]);
    } else {
      // let productIds = [...new Set(this.tableChild.productselectedRowsItem)];
      // let parameterIds = [...new Set(this.tableChild.paramselectedRowsItem)];
      let productIds: number[] = [];
      let parameterIds: number[] = [];
      this.tableChild.productparamSelectedItem.forEach(record => {
        productIds.push(record.productId);
        parameterIds.push(record.parameterId);
      });
      console.log("this.rows = ", this.rows);
      console.log("productIds = ", productIds);
      console.log("parameterIds = ", parameterIds);
      this.deleteMultipleDetails({ productIds, parameterIds });
    }
  }

  formRecord() {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    if (this.formData.valid) {
      if (this.isInsertNewRecord) {
        this.addCategoryDetails({ ...this.formData.value, categoryId: this.updatedIndexId })

      } else {
        this.updateCategoryDetails({ ...this.formData.value, categoryId: this.updatedIndexId })
      }
    } else {
      this.formData.markAllAsTouched();
    }
  }

  private updateRows(): void {
    if (this.currentTab === 'Category') {
      this.rows = this.listAry.map((res: any) => ({
        "Name": res.categoryName,
        "Description": res.catDescription,
        "CategoryId": res.categoryId,
        "entityId": res.entityId,
        "Created By": res.createdBy,
        "Created Date": res.createdByDateTime,
        "Updated By": res.updatedBy,
        "Updated Date": res.updatedByDateTime
      }));
    } else if (this.currentTab === 'Info') {
      this.isDataReady = false;
      this.rows = this.listAry.map((res: any) => ({
        "Stream Name": res.productName,
        "Code": res.code,
        "Category": res.categoryId,
        "Product Name": res.categoryName,
        "ProductImage": res.productImage,
        "Narrative": res.narrative,
        "Description": res.description,
        "ProductId": res.productId,
        "EntityId": res.entityId,
        "MimeType": res.mimeType,
        "Created By": res.createdBy,
        "Created Date": res.createdByDateTime,
        "Updated By": res.updatedBy,
        "Updated Date": res.updatedByDateTime,
        "ExceptionId": res?.exceptionId,
        "Stream Cap": res.maxEligibleAmount,
      }));
      this.isDataReady = true;
    } else {
      this.rows = this.listAry.map((res: any) => ({
        "Stream": res.productName,
        "ProductId": res.productId,
        "ParameterId": res.parameterId,
        "Parameter": res.parameterName,
        "Parameter value": res.paramValue,
        "Display order": res.displayOrder,
        "IsRequired": res.isRequired,
        "EntityId": res.entityId,
        "Created By": res.createdBy,
        "Created Date": res.createdByDateTime,
        "Updated By": res.updatedBy,
        "Updated Date": res.updatedByDateTime
      }));
    }
  }
  private initForm(): void {
    if (this.currentTab === 'Category') {
      this.deleteKeyForMultiple = 'CategoryId';
      this.formData = this.fb.group({
        categoryName: ['', Validators.required],
        catDescription: ['', Validators.required],
        entityId: ['']
      });
    } else if (this.currentTab === 'Info') {
      this.deleteKeyForMultiple = 'ProductId';
      this.formData = this.fb.group({
        code: ['', Validators.required],
        productName: ['', Validators.required],
        productCategory: ['', Validators.required],
        productImage: [''],
        narrative: [''],
        description: [''],
        entityId: [''],
        mimeType: [''],
        exceptionId: new FormControl({ value: '', disabled: true }),
        isExceptionRule: [false],
        maxEligibleAmount: ['']
      });
    } else {
      this.deleteKeyForMultiple = 'ProductId';
      this.formData = this.fb.group({
        product: [null, Validators.required],
        parameter: [null, Validators.required],
        parameterValue: [null],
        displayOrder: ['', [Validators.required]],
        isrequired: [null, Validators.required],
        entityId: ['']
      });
    }
  }

  onSubmit() {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    if (this.formData.valid) {
      if (this.isInsertNewRecord) {
        this.addInfoDetails({ ...this.formData.value, productId: 0, categoryId: parseInt(this.formData.value.productCategory) })
      } else {
        this.updateInfoDetails({ ...this.formData.value, productId: this.updatedIndexId, categoryId: parseInt(this.formData.value.productCategory) })
      }
      this.imagePreview = '';
    } else {
      this.formData.markAllAsTouched();
    }
  }

  bindEntityId(value: any, tab: string) {
    if (tab === 'Details') {
      const productId = parseInt(value.target.value, 10);
      const matchingProduct = this.productList.find(
        (x: { productId: number }) => x.productId === productId
      );
      if (matchingProduct) {
        this.formData.patchValue({
          entityId: matchingProduct.entityId
        });
      }
    } else {
      const categoryId = parseInt(value.target.value, 10);
      const matchingCategory = this.categoryList.find(
        (x: { categoryId: number }) => x.categoryId === categoryId
      );
      if (matchingCategory) {
        this.formData.patchValue({
          entityId: matchingCategory.entityId
        });
      }
    }
  }
  detailsFormSubmit(): void {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
      if (this.formData.valid) {
        let payload = {
          entityId: this.formData.value.entityId,
          displayOrder: parseInt(this.formData.value.displayOrder),
          isRequired: this.formData.value.isrequired === true,
          parameterId: parseInt(this.formData.value.parameter),
          paramValue: this.formData.value.parameterValue,
          productId: parseInt(this.formData.value.product),
        };

        if (this.isEditing) {
          this.updateProductDetails(payload);
        } else {
          this.addProductDetails(payload);
        }
      } else {
        this.formData.markAllAsTouched();
      }
    });
  }

  exportProducts() {
    if (this.currentTab === 'Category') {
      let listOfSelectedIds = new Set(this.tableChild.selectedRowsItem);
      this.productService.exportCategories([...listOfSelectedIds]).subscribe({
        next: (response: Blob) => {
          console.log('Blob resp: ', response);
          const url = window.URL.createObjectURL(response);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = 'Category.xlsx';
          document.body.appendChild(anchor);
          anchor.click();
          document.body.removeChild(anchor);
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      });
    }
    if (this.currentTab === 'Info') {
      let listOfSelectedIds = new Set(this.tableChild.selectedRowsItem);
      this.productService.exportInfoList([...listOfSelectedIds]).subscribe({
        next: (response: Blob) => {
          console.log('Blob resp: ', response);
          const url = window.URL.createObjectURL(response);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = 'Info.xlsx';
          document.body.appendChild(anchor);
          anchor.click();
          document.body.removeChild(anchor);
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      });
    }
    if (this.currentTab === 'Details') {
      let listOfSelectedIds = new Set(this.tableChild.selectedRowsItem);
      this.productService.exportDetails([...listOfSelectedIds]).subscribe({
        next: (response: Blob) => {
          console.log('Blob resp: ', response);
          const url = window.URL.createObjectURL(response);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = 'Details.xlsx';
          document.body.appendChild(anchor);
          anchor.click();
          document.body.removeChild(anchor);
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        }
      });
    }
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

  toggleException() {
    // this.isExceptionRule = event;
    const exceptionControl = this.formData?.get('exceptionId');
    if (this.formData?.get('isExceptionRule')?.value) {
      exceptionControl?.enable();
      exceptionControl?.setValidators(Validators.required)
    }
    else {
      exceptionControl?.setValue(null)
      exceptionControl?.disable();
      exceptionControl?.clearValidators()
    }
    exceptionControl?.updateValueAndValidity();
  }
}
