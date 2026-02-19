import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductsService } from '../../../core/services/setting/products.service';
import { ProductCapAmountService } from '../../../core/services/setting/product-cap-amount.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { MatDialog } from '@angular/material/dialog';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-product-cap-amount',
  standalone: false,

  templateUrl: './product-cap-amount.component.html',
  styleUrl: './product-cap-amount.component.scss'
})
export class ProductCapAmountComponent {
  ProductCapForm !: FormGroup;
  isEditMode: boolean = false;
  productsList: any[] = [];
  displayedColumns: string[] = ['productName', 'age', 'salary', 'amount', 'actions'];
  selectedProductData: any[] = [];
  private _snackBar = inject(MatSnackBar);
  formvisible: boolean = false;
  isLoading: boolean = false;
  message: string = "Loading data, please wait...";
  constructor(private fb: FormBuilder, private productservice: ProductsService, private productAmountService: ProductCapAmountService, private PermissionsService: PermissionsService, private dialog: MatDialog, private translate: TranslateService) { }
  ngOnInit(): void {
    this.ProductCapForm = this.fb.group({
      id: [],
      productId: ['', [Validators.required]],
      //activity: ['', [Validators.required]],
      maxCapPerStream: [null],
      age: ['', [Validators.required]],
      salary: ['', [Validators.required]],
      amount: ['', [Validators.required, Validators.pattern(/^\d+(\.\d+)?$/)]],

    },
    )
    this.fetchAllProducts()
    this.ProductCapForm.get('productId')?.valueChanges.subscribe(productId => {
      this.onProductSelected(+productId);
    });
  }
  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
  onSubmit() {
    //this.isSubmitted = true;
    //const min = +this.ProductCapForm.get('minimumScore')?.value;
    //const max = +this.ProductCapForm.get('maximumScore')?.value;

    // Custom validation check
    //if (!isNaN(min) && !isNaN(max) && min > max) {
    //  this.showMinMaxError = true;
    //  return; // ⛔ Stop submission
    //}

    if (this.ProductCapForm.invalid) {
      this.ProductCapForm.markAllAsTouched(); // Show all validation messages
      return;
    }
    if (this.ProductCapForm.invalid) return;

    const payload = this.ProductCapForm.value;

    if (this.isEditMode) {

      this.productAmountService.updateProductCap(payload).subscribe({
        next: (Response) => {
          this.formvisible = false;
          const selectedProductId = this.ProductCapForm.get('productId')?.value;
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
        error: (error: any) => {
          this._snackBar.open(error.message, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          console.log("", error);
        }
      });
    } else {

      payload.id = 0;
      this.productAmountService.addProductCapAmount(payload).subscribe({
        next: (Response) => {
          this.formvisible = false;
          const selectedProductId = this.ProductCapForm.get('productId')?.value;
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
          this._snackBar.open(error.message, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          console.log("", error);
        }
      }
      );

    }
  }
  fetchAllProducts() {
    this.isLoading = true;

    this.productservice.getInfoListName().subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.productsList = response.data;
          this.isLoading = false;
        }
      }, error: (error) => {
        this._snackBar.open(error.message, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        console.log("Products :", error)
      }

    })

  }
  onProductSelected(productId: number) {
    //this.formvisible = false;
    const selectedProduct = this.productsList?.find(p => p.productId === +productId);


    const productName = selectedProduct?.productName;

    this.productAmountService.getProducCapAmountList(productId).subscribe({
      next: (response) => {

        if (response.isSuccess && Array.isArray(response.data)) {
          this.selectedProductData = response.data.map((item: {
            productName: any;
            activity: any; maxCapPerStream: any; age: any; id: any; salary: any; amount: any;
          }) => ({
            id: item.id,
            productName: productName,
            //activity: item.activity ||'',
            maxCapPerStream: item.maxCapPerStream || null,
            age: item.age || 0,
            salary: item.salary || 0,
            amount: item.amount || 0

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
  editProductCapAmount(item: any) {
    //this.isSubmitted = false;
    this.formvisible = true;
    this.isEditMode = true;
    this.ProductCapForm.patchValue(item);
  }
  deleteProductCapAmount(element: any): void {

    const dialogRef = this.dialog.open(DeleteDialogComponent, {
      data: {
        title: this.translate.instant('Confirm'),
        message: this.translate.instant('Are you sure you want to delete the Product Cap Amount for "{{name}}"?', { name: element.productName })
      }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (!result?.delete) return;

      this.productAmountService.deleteProductCap(element.id).subscribe({
        next: (response) => {

          this._snackBar.open(response.message, 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top',
            duration: 3000
          });

          if (response.isSuccess) {

            const selectedProductId =
              this.ProductCapForm.get('productId')?.value;

            if (selectedProductId) {
              this.onProductSelected(selectedProductId);
            }

            this.formvisible = false;
          }

        },
        error: (error) => {

          const message =
            error?.error?.message ||
            error?.message ||
            'Something went wrong.';

          this._snackBar.open(message, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top'
          });

          console.error('Delete Product Cap Amount Error:', error);
        }
      });

    });
  }
  resetForm() {
    //this.ProductCapForm.reset();
    this.formvisible = false
  }
  addProductCap() {
    this.isEditMode = false;
    this.ProductCapForm.patchValue({
      //activity:'',
      maxCapPerStream: null,
      age: '',
      salary: '',
      amount: '',
    })
    this.ProductCapForm.markAsUntouched();

    this.formvisible = true;
  }
}




