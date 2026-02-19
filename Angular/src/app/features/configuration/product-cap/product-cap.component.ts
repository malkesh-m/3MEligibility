import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ProductsService } from '../../../core/services/setting/products.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProductCapService } from '../../../core/services/setting/product-cap.service';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
// import { MatTableDataSource } from '@angular/material/table';
// import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-product-cap',
  standalone: false,

  templateUrl: './product-cap.component.html',
  styleUrl: './product-cap.component.scss'
})

export class ProductCapComponent implements OnInit {
  isDataReady: boolean = false;

  selectedProductData: any[] = [];
  isEditMode = false;
  formvisible = false;
  ProductCapForm !: FormGroup;
  private _snackBar = inject(MatSnackBar);
  productsList: any[] = [];
  displayedColumns: string[] = ['ProductName', 'ProductCap', 'MinimumScore', 'MaximumScore', 'actions'];
  showMinMaxError = false;
  isLoading: boolean = false;
  message: string = "Loading data, please wait...";
  isUploading: boolean = false;
  isDownloading: boolean = false;
  constructor(private fb: FormBuilder,
    private productservice: ProductsService,
    private productcapservice: ProductCapService, private PermissionsService: PermissionsService, private dialog: MatDialog, private translate: TranslateService) { }
  isSubmitted = false;
  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
  ngOnInit(): void {
    this.fetchAllProducts();
    this.ProductCapForm = this.fb.group({
      id: [],
      minimumScore: ['', [Validators.required]],
      maximumScore: ['', [Validators.required]],
      productID: ['', [Validators.required]],
      productCapPercentage: ['', [Validators.required, Validators.pattern(/^\d+(\.\d{1,2})?$/)]],
      productName: []
    }, { validators: this.minLessThanOrEqualMaxValidator })
    this.ProductCapForm.get('productID')?.valueChanges.subscribe(productId => {
      this.onProductSelected(+productId);
    });

    this.ProductCapForm.get('minimumScore')?.valueChanges.subscribe(() => this.validateMinMax());
    this.ProductCapForm.get('maximumScore')?.valueChanges.subscribe(() => this.validateMinMax());
  }

  minLessThanOrEqualMaxValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const form = control as FormGroup;
    const min = form.get('minimumScore')?.value;
    const max = form.get('maximumScore')?.value;

    if (min != null && max != null && +max < +min) {
      return { maxLessThanMin: true };
    }

    return null;
  };

  onProductSelected(productId: number) {
    this.formvisible = false;
    const selectedProduct = this.productsList?.find(p => p.productId === +productId);


    const productName = selectedProduct?.productName;

    this.productcapservice.getProductList(productId).subscribe({
      next: (response) => {

        if (response.isSuccess && Array.isArray(response.data)) {
          this.selectedProductData = response.data.map((item: {
            productName: any;
            productCapPercentage: any; minimumScore: any; maximumScore: any; id: any
          }) => ({
            id: item.id,
            productName: productName,
            productCapPercentage: item.productCapPercentage || 0,
            minimumScore: item.minimumScore || 0,
            maximumScore: item.maximumScore || 0
          }));

        } else {
          this.selectedProductData = [];
        }
      },
      error: (error) => {
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        console.error("Error fetching product cap data", error);
        this.selectedProductData = [];
      }
    });
  }
  validateMinMax() {
    if (!this.formvisible || !this.isSubmitted) return;
    const min = +this.ProductCapForm.get('minimumScore')?.value;
    const max = +this.ProductCapForm.get('maximumScore')?.value;

    this.showMinMaxError = !isNaN(min) && !isNaN(max) && min > max;
  }



  fetchAllProducts() {
    this.isLoading = true;
    this.productservice.getInfoListName().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.productsList = response.data;
          this.isDataReady = true;
        }
        this.isLoading = false;
      }, error: (error) => {

        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        console.log("Products :", error)
        this.isLoading = false;
      }

    })

  }

  addProductCap() {
    this.isEditMode = false;
    this.ProductCapForm.patchValue({
      minimumScore: '',
      maximumScore: '',
      productCapPercentage: ''
    })
    this.showMinMaxError = false;
    this.formvisible = true;
    this.isSubmitted = false;
  }

  onSubmit() {
    this.isSubmitted = true;
    const min = +this.ProductCapForm.get('minimumScore')?.value;
    const max = +this.ProductCapForm.get('maximumScore')?.value;

    // Custom validation check
    if (!isNaN(min) && !isNaN(max) && min > max) {
      this.showMinMaxError = true;
      return; // ⛔ Stop submission
    }

    this.showMinMaxError = false;
    if (this.ProductCapForm.invalid) {
      this.ProductCapForm.markAllAsTouched(); // Show all validation messages
      return;
    }
    if (this.ProductCapForm.invalid) return;

    const payload = this.ProductCapForm.value;

    if (this.isEditMode) {

      this.productcapservice.updateProductCap(payload).subscribe({
        next: (Response) => {
          this.formvisible = false;
          const selectedProductId = this.ProductCapForm.get('productID')?.value;
          if (selectedProductId) {

            this.onProductSelected(selectedProductId);
            this.ProductCapForm.patchValue({
              productID: selectedProductId
            })
          }
          this._snackBar.open(Response.message, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });

        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          console.log("", error);
        }
      });
    } else {

      payload.id = 0;


      this.productcapservice.addProductCap(payload).subscribe({
        next: (Response) => {
          this.formvisible = false;
          const selectedProductId = this.ProductCapForm.get('productID')?.value;
          if (selectedProductId) {
            this.onProductSelected(selectedProductId);
            this.ProductCapForm.patchValue({
              productID: selectedProductId
            })
          }
          this._snackBar.open(Response.message, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
          console.log("", error);
        }
      }
      );

    }
  }

  editProductCap(item: any) {
    this.isSubmitted = false;
    this.formvisible = true;
    this.isEditMode = true;
    this.ProductCapForm.patchValue(item);
  }
  deleteProductCap(element: any): void {

    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: this.translate.instant('Confirm'),
        message: this.translate.instant('Are you sure you want to delete the Product Cap for "{{name}}"?', { name: element.productName })
      }
    });

    dialogRef.afterClosed().subscribe((result: { delete: any; }) => {

      if (!result?.delete) return;

      this.productcapservice.deleteProductCap(element.id).subscribe({
        next: (response) => {

          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          });

          if (response.isSuccess) {
            const selectedProductId = this.ProductCapForm.get('productID')?.value;

            if (selectedProductId) {
              this.onProductSelected(selectedProductId);
            }
          }

        },
        error: (error) => {

          const message =
            error?.error?.message ||
            error?.message ||
            'Something went wrong.';

          this._snackBar.open(message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          });

          console.error('Delete Product Cap Error:', error);
        }
      });

    });
  }

  resetForm() {
    this.isEditMode = false;
    this.formvisible = false;
  }
}






