import { ChangeDetectorRef, Component, inject, ViewChild } from '@angular/core';
import { ProductsService } from '../../../core/services/setting/products.service';
import { ProductListComponent } from './product-list/product-list.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth/auth.service';
import { RolesService } from '../../../core/services/setting/role.service';

interface Product {
  productId: number;
  productName: string;
  categoryId: number;
  entityId: number;
  code: string;
  productImage: string;
  categoryName?: string;  // Optional, will be added after mapping
  description: string;
  narrative: string;
  mimeType: string;
  maxEligibleAmount: number;
}

interface Category {
  categoryId: 0,
  categoryName: string,
  catDescription: string,
  entityId: 0
}

@Component({
  selector: 'app-product',
  standalone: false,

  templateUrl: './product.component.html',
  styleUrl: './product.component.scss'
})
export class ProductComponent {
  @ViewChild('productChild') productListChiild!: ProductListComponent;

  tabsList = [
    {
      id: 1,
      name: 'Category',
      route: '/setting/products/list'
    },
    {
      id: 2,
      name: 'Info',
      route: '/setting/products/info'
    },
    {
      id: 3,
      name: 'Details',
      route: '/setting/products/details'
    }
  ]
  isDataReady: boolean = false;
  searchTerm: string = '';
  categoriesList = [];
  infoList = [];
  detailsList = [];
  activeTab: string = 'Category'
  menuVisible: boolean = false;
  isTableDataUpdated: boolean = false;
  categoryTableColumnAry = ['Select', 'Name', 'Description', 'Created By', 'Updated By', 'Actions'];
  infoTableColumnAry = ['Select', 'Code', 'Category Name', 'Product Name', 'Created By', 'Updated By', 'Actions'];
  detailsTableColumnAry = ['Select', 'Product', 'Parameter', 'Parameter value', 'Display order', 'Created By', 'Updated By', 'Actions'];
  categoryTableHeaderAry = ['Name', 'Description', 'Created By', 'Updated By'];
  infoTableHeaderAry = ['Code', 'Product Name', 'Category Name', 'Created By', 'Updated By'];
  detailsTableHeaderAry = ['Product', 'Parameter', 'Parameter value', 'Display order', 'Created By', 'Updated By'];

  filteredCategoriesList = [];
  filteredInfoList = [];
  filteredDetailsList = [];
  searchTerms: { [key: string]: string } = {
    Category: '',
    Info: '',
    Details: ''
  };
  private _snackBar = inject(MatSnackBar);
  selectedFile: File | null = null;
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = "Loading data, please wait...";
  loggedInUser: any = null;
  createdBy: string = '';
  tabMapping: { [key: string]: number } = {
    'Category': 0,
    'Info': 1,
    'Details': 2
  };

  constructor(private productService: ProductsService, private rolesService: RolesService, private cdr: ChangeDetectorRef, private authService: AuthService) { }

  ngOnInit() {
    if (this.activeTab === 'Category') {
      this.fetchCategoriesList();
    } else if (this.activeTab === 'Info') {
      this.fetchInfoList();
    } else {
      this.fetchDetailsList();
    }
  }

  getAddPermission(): boolean {
    return (this.activeTab === 'Category' && this.hasPermission('Permissions.Category.Create')) ||
      (this.activeTab === 'Info' && this.hasPermission('Permissions.Product.Create')) || (this.activeTab === 'Details' && this.hasPermission('112'));
  }

  getDeletePermission(): boolean {
    return (this.activeTab === 'Category' && this.hasPermission('Permissions.Category.Delete')) ||
      (this.activeTab === 'Info' && this.hasPermission('Permissions.Product.Delete')) || (this.activeTab === 'Details' && this.hasPermission('114'));
  }

  getImportPermission(): boolean {
    return (this.activeTab === 'Category' && this.hasPermission('Permissions.Category.Import')) ||
      (this.activeTab === 'Info' && this.hasPermission('Permissions.Product.Import')) || (this.activeTab === 'Details' && this.hasPermission('115'));
  }

  getExportPermission(): boolean {
    return (this.activeTab === 'Category' && this.hasPermission('Permissions.Category.Export')) ||
      (this.activeTab === 'Info' && this.hasPermission('Permissions.Product.Export')) || (this.activeTab === 'Details' && this.hasPermission('116'));
  }

  hasPermission(roleId: string): boolean {
    return this.rolesService.hasPermission(roleId);
  }

  insertNewRecord() {
    if (this.productListChiild && typeof this.productListChiild.addNewRecord === 'function') {
      this.isDataReady = true;
      this.productListChiild.addNewRecord();
    } else {

    } this.menuVisible = false;
    this.isDataReady = true;

  }

  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }


  tabChanged(event: any) {
    this.activeTab = Object.keys(this.tabMapping).find(key => this.tabMapping[key] === event.index) || 'Category';
    this.searchTerm = this.searchTerms[this.activeTab] || ''; // Load the stored search term
    if (this.activeTab === 'Category') {
      this.fetchCategoriesList();
    } else if (this.activeTab === 'Info') {
      this.fetchInfoList();
    } else {
      this.fetchInfoList();
      this.fetchDetailsList();
    }
    this.applyFilter(); // Apply the filter for the active tab
  }

  getTabIndex(): number {
    return this.tabMapping[this.activeTab] || 0;
  }

  switchTab(tabName: string): void {
    this.activeTab = tabName;
    this.searchTerm = this.searchTerms[this.activeTab] || ''; // Load the stored search term
    if (this.activeTab === 'Category') {
      this.fetchCategoriesList();
    } else if (this.activeTab === 'Info') {


      this.fetchInfoList();


    } else {
      this.fetchInfoList();
      this.fetchDetailsList();
    }
    this.applyFilter();
  }

  fetchCategoriesList(action?: string) {
    this.productService.getCategoriesList().subscribe({
      next: (response) => {
        this.categoriesList = response.data;
        this.applyFilter(action);
      },
      error: (error) => {
        this.categoriesList = [];
        this.applyFilter(action);
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
    this.isTableDataUpdated = false;
  }

  fetchInfoList(action?: string) {
    this.productService.getInfoList().subscribe({
      next: (response) => {
        this.infoList = response.data.map((product: Product) => {
          const category = this.categoriesList.find((category: Category) => category.categoryId === product.categoryId);
          if (category) {
            product.categoryName = category['categoryName'];
          }
          return product;
        });
        this.applyFilter(action);

      },
      error: (error) => {
        this.infoList = [];
        this.applyFilter(action);
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  fetchDetailsList(action?: string) {
    this.productService.getDetailsList().subscribe({
      next: (response) => {
        this.detailsList = response.data;
        this.applyFilter(action);
      },
      error: (error) => {
        this.detailsList = [];
        this.applyFilter(action);
        this._snackBar.open(error.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
      }
    })
  }

  categoryTableDataUpdate(event: any) {


    if (event.event === 'CategoryList') {
      this.fetchCategoriesList(event.action)
      this.categoriesList = [];
    } else if (event.event === 'InfoList') {
      this.fetchInfoList(event.action)
      this.infoList = []
    } else {
      this.fetchDetailsList(event.action);
      this.detailsList = [];

    }
    // this.isTableDataUpdated = true;

  }

  applyFilter(action?: string) {
    const searchTerm = this.searchTerms[this.activeTab]?.toLowerCase() || '';
    this.cdr.markForCheck();
    if (this.productListChiild) {
      this.productListChiild.loadTabData(searchTerm, action);
    }
  }

  // exportProducts() {
  //   if (this.activeTab === 'Category') {
  //     this.productService.exportCategories().subscribe({
  //       next: (response: Blob) => {
  //         console.log('Blob resp: ', response);
  //         const url = window.URL.createObjectURL(response);
  //         const anchor = document.createElement('a');
  //         anchor.href = url;
  //         anchor.download = 'Category.xlsx';
  //         document.body.appendChild(anchor);
  //         anchor.click();
  //         document.body.removeChild(anchor);
  //         window.URL.revokeObjectURL(url);
  //       },
  //       error: (error) => {
  //         this._snackBar.open(error.message, 'Okay', {
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top', duration: 3000
  //         });
  //       }
  //     });
  //   }
  //   if (this.activeTab === 'Info') {
  //     this.productService.exportInfoList().subscribe({
  //       next: (response: Blob) => {
  //         console.log('Blob resp: ', response);
  //         const url = window.URL.createObjectURL(response);
  //         const anchor = document.createElement('a');
  //         anchor.href = url;
  //         anchor.download = 'Info.xlsx';
  //         document.body.appendChild(anchor);
  //         anchor.click();
  //         document.body.removeChild(anchor);
  //         window.URL.revokeObjectURL(url);
  //       },
  //       error: (error) => {
  //         this._snackBar.open(error.message, 'Okay', {
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top', duration: 3000
  //         });
  //       }
  //     });
  //   }
  //   if (this.activeTab === 'Details') {
  //     this.productService.exportDetails().subscribe({
  //       next: (response: Blob) => {
  //         console.log('Blob resp: ', response);
  //         const url = window.URL.createObjectURL(response);
  //         const anchor = document.createElement('a');
  //         anchor.href = url;
  //         anchor.download = 'Details.xlsx';
  //         document.body.appendChild(anchor);
  //         anchor.click();
  //         document.body.removeChild(anchor);
  //         window.URL.revokeObjectURL(url);
  //       },
  //       error: (error) => {
  //         this._snackBar.open(error.message, 'Okay', {
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top', duration: 3000
  //         });
  //       }
  //     });
  //   }
  // }

  downloadTemplate() {
    this.isDownloading = true;
    this.message = "Please wait, template is downloading...";
    if (this.activeTab === 'Category') {
      this.productService.DownloadCategoryTemplate().subscribe((response) => {
        this.isDownloading = false;
        const blob = new Blob([response], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Category-Template.xlsx'; // Filename for the download
        a.click();
        window.URL.revokeObjectURL(url);
        this._snackBar.open('Category Template Download Successfully.', 'Okay', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
      });
    }
    else if (this.activeTab === 'Info') {
      this.productService.DownloadInfoTemplate().subscribe((response) => {
        this.isDownloading = false;
        const blob = new Blob([response], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Info-Template.xlsx'; // Filename for the download
        a.click();
        window.URL.revokeObjectURL(url);
        this._snackBar.open('Info Template Download Successfully.', 'Okay', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
      });
    }
    else if (this.activeTab === 'Details') {
      this.productService.DownloadDetailsTemplate().subscribe((response) => {
        this.isDownloading = false;
        const blob = new Blob([response], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Details-Template.xlsx'; // Filename for the download
        a.click();
        window.URL.revokeObjectURL(url);
        this._snackBar.open('Details Template Download Successfully.', 'Okay', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
      });
    }
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      this.importProduct(this.selectedFile);
    } else {
      this._snackBar.open('Please select a file first.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    }
  }

  importProduct(selectedFile: File) {
    //   this.authService.currentUser$.subscribe((user) => {
    //     this.loggedInUser = user;
    // });
    // this.createdBy = this.loggedInUser.user.userName;
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    if (this.activeTab === 'Category') {
      this.productService.importCategory(selectedFile).subscribe({
        next: (response) => {
          this.isUploading = false;
          this.fetchCategoriesList();
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
    else if (this.activeTab === 'Info') {
      this.productService.importInfo(selectedFile, this.createdBy).subscribe({
        next: (response) => {
          this.isUploading = false;
          this.fetchInfoList();
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
    else if (this.activeTab === 'Details') {
      this.productService.importDetails(selectedFile, this.createdBy).subscribe({
        next: (response) => {
          this.isUploading = false;
          this.fetchDetailsList();
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
  }
}
