import { Component, OnInit, ViewChild, AfterViewInit, inject, Injectable, ChangeDetectorRef } from '@angular/core';
import { EntityService } from '../../../core/services/setting/entity.service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { MatSnackBar } from "@angular/material/snack-bar";
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { NgForm } from '@angular/forms';
import { UtilityService } from '../../../core/services/utility/utils';
import { AuthService } from '../../../core/services/auth/auth.service';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { environment } from '../../../../environments/environment';

export interface EntityRecord {
  entityName: string;
  code: string;
  countryId: number | null;
  cityId: number | null;
  isparent: boolean;
  entityAddress: string;
  selected: boolean;
  entitylocation?: string;
  entityId?: number;
  parentEnitityId: number | null;
  createdBy: string;
  updatedBy: string;
//  BaseCurrencyId: number | null;
}

export interface Country {
  id: number;
  name: string;
}

export interface City {
  id: number;
  name: string;
  countryId: number;
}
//export interface Currency {
//  currencyId: number;
//  currencyName: string;

//}

export interface EntityOption {
  entityId: number;
  entityName: string;
}

@Component({
  selector: 'app-entity',
  standalone: false,
  templateUrl: './entity.component.html',
  styleUrl: './entity.component.scss'
})

@Injectable({
  providedIn: 'root',
})

export class EntityComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['select', 'code', 'entityName', 'countryId', 'cityId', 'entityAddress', 'isparent', 'parentEnitityId', 'createdBy', 'updatedBy', 'actions'];
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  private _snackBar = inject(MatSnackBar);
  records: EntityRecord[] = [];
  dataSource = new MatTableDataSource<EntityRecord>(this.records); // Initialize empty data source
  formVisible = false;
  menuVisible = false;
  isEditMode = false;

  formData: EntityRecord = {
    entityName: '',
    code: '',
    entityAddress: '',
    countryId: null,
    cityId: null,
    entitylocation: '',
    isparent: false,
    selected: false,
    parentEnitityId: null,
    createdBy: '',
    updatedBy: '',
  //  BaseCurrencyId: null
  };
  //Currencies: Currency[] = [];
  searchTerm: string = '';
  countries: Country[] = [];
  cities: City[] = [];
  filteredCities: City[] = [];
  entitiesOptions: EntityOption[] = [];
  selectedRows: Set<number> = new Set();
  selectedRowsItem: Set<number> = new Set();
  pageSize: number = 5;
  selectedFile: File | null = null;
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = "Loading data, please wait...";
  loggedInUser: any = null;
  createdBy: string = '';
  showMaxLengthWarning: boolean = false;
  showMaxLengthEntityCode: boolean = false;


  constructor(
    private entityService: EntityService,
    private dialog: MatDialog,
    private utilityService: UtilityService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private PermissionsService: PermissionsService
  ) { }

  async ngOnInit() {
    await Promise.all([this.fetchCountries(), this.fetchCities()]);
    //this.getCurrencyList();
    // this.fetchEntitiesList(); // Fetch entities options
    console.log(environment.production)
    console.log("environment.production")

  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
  }
  onEntityNameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    if (value.length > 50) {
      this.showMaxLengthWarning = true;
      // Truncate to 50 chars
      this.formData.entityName = value.slice(0, 50);
    } else {
      this.showMaxLengthWarning = false;
      this.formData.entityName = value;
    }
  }
  onEntityCodeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    if (value.length > 10) {
      this.showMaxLengthEntityCode = true;
      // Truncate to 50 chars
      this.formData.entityName = value.slice(0, 10);
    } else {
      this.showMaxLengthEntityCode = false;
      this.formData.code = value;
    }
  }

  toggleColumn(column: string, afterColumn: string) {
    const index = this.displayedColumns.indexOf(column);

    if (index > -1) {
      // Remove column if already visible
      this.displayedColumns.splice(index, 1);
    } else {
      // Find the index of the afterColumn and insert right after it
      const afterIndex = this.displayedColumns.indexOf(afterColumn);
      if (afterIndex !== -1) {
        this.displayedColumns.splice(afterIndex + 1, 0, column);
      } else {
        this.displayedColumns.push(column); // Default push if not found
      }
    }

    this.displayedColumns = [...this.displayedColumns]; // Ensure reactivity
  }

  ngAfterViewInit() {
    // Bind paginator and sort after the view is initialized
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  toggleMenu() {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu() {
    this.menuVisible = false;
  }

  // @HostListener('document:click', ['$event.target'])
  // onClickOutside(targetElement: HTMLElement) {
  //   const dropdown = document.querySelector('.dropdown-toggle');
  //   const menu = document.querySelector('.dropdown-menu');

  //   if (!dropdown?.contains(targetElement) && !menu?.contains(targetElement)) {
  //     this.closeMenu();
  //   }
  // }


  /**
 * Function: fetchEntitiesList
 * Description: Fetches the list of entities from the server.
 * @returns {Promise<Entity[]>} A promise that resolves with the list of entities.
 */
  fetchEntitiesList() {
    this.entityService.getEntitiesList().subscribe({
      next: (response) => {
        // Sorting the response data based on lastModifiedDateTime in descending order
        const sortedData = response.data.sort((a: any, b: any) =>
          new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
        );
        this.dataSource.data = sortedData.map((item: any) => ({
          countryName: this.countries.find(c => c.id === item.countryId)?.name || 'Unknown',
          cityName: this.cities.find(c => c.id === item.cityId)?.name || 'Unknown',
          entityId: item.entityId,
          entityName: item.entityName,
          code: item.code,
          countryId: item.countryId,
          cityId: item.cityId,
          isparent: item.isparent,
          entityAddress: item.entityAddress,
          selected: false,
          entitylocation: item.entitylocation,
          parentEnitityId: item.parentEnitityId,
          parentEnitityName: item.parentEnitityId && response.data.find((c: any) => c.entityId === item.parentEnitityId)?.entityName || '',
          createdBy: item.createdBy,
          createdByDateTime: item.createdByDateTime,
          updatedBy: item.updatedBy,
          updatedByDateTime: item.updatedByDateTime,
          BaseCurrencyId: item.baseCurrencyId,
        }));

        this.entitiesOptions = response.data.filter((entity: EntityRecord) => !entity.isparent).map((item: any) => ({
          entityId: item.entityId,
          entityName: item.entityName,
          baseCurrencyId: item.baseCurrencyId
        }));
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

  /**
   * Function: fetchCountries
   * Description: Fetches the list of Countries from the server.
   * @returns {Promise<Country[]>} A promise that resolves with the list of Countries.
   */
  fetchCountries(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.entityService.getCountriesList().subscribe({
        next: (countries) => {
          this.countries = countries.data.map((country: any) => ({
            id: country.countryId,
            name: country.countryName,
          }));
          resolve();
        },
        error: (err) => reject(err),
      });
    });
  }

  /**
   * Function: fetchCities
   * Description: Fetches the list of Cities from the server.
   * @returns {Promise<Cities[]>} A promise that resolves with the list of Cities.
   */
  fetchCities(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.entityService.getCitiesList().subscribe({
        next: (cities) => {
          this.cities = cities.data.map((city: any) => ({
            id: city.cityId,
            name: city.cityName,
            countryId: city.countryId,
          }));
          resolve();
        },
        error: (err) => reject(err),
      });
    });
  }
  //getCurrencyList() {
  //  return new Promise((resolve, reject) => {
  //    this.entityService.getCurrencyList().subscribe({
  //      next: (Currencies) => {
  //        console.log(Currencies.data)
  //        console.log("Currencies.data")

  //        this.Currencies = Currencies.data.map((Currency: any) => ({
  //          currencyId: Currency.currencyId,
  //          currencyName: Currency.currencyName,

  //        }));
  //      },
  //      error: (err) => reject(err),
  //    });
  //  });
  //}


  //filter the citites based on the selected country
  filterCities() {
    this.filteredCities = this.cities.filter(city => city.countryId === Number(this.formData.countryId));
  }

  // Get current page data
  getCurrentPageData(): any[] {
    if (!this.paginator) return []; // Return empty array if paginator is undefined
    const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
    const endIndex = startIndex + this.paginator.pageSize;
    return this.dataSource.filteredData.slice(startIndex, endIndex);
  }

  // this function triggers when we search for an entity thorugh different columns
  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.dataSource.filter = filterValue;
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  /**
 * Function: addRecord
 * Description: add or Updates entity on the server.
 * @param {NgForm} form - The form object with updated details.
 * @returns {Promise<void>} A promise that resolves once the entity is updated.
 */
  addRecord(form: NgForm) {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    if (this.formData.entityName?.length > 50) {
      this.showMaxLengthWarning = true;
      return; // 🚫 prevent form submission
    }
    this.showMaxLengthWarning = false;

    if (this.formData.isparent) {
      const parentEntity = this.dataSource.data.find(
        (item) => item.entityId === this.formData.parentEnitityId

      );
      //console.log('Parent baseCurrencyId:', parentEntity);

      //if (parentEntity && parentEntity.BaseCurrencyId) {

      //  this.formData.BaseCurrencyId = parentEntity.BaseCurrencyId;

      //  console.log('Parent baseCurrencyId:', parentEntity.BaseCurrencyId);
      //} else {

      //  console.warn('Parent entity not found or baseCurrencyId is missing');
      //}
    } else {
      this.formData.parentEnitityId = 0;
    }
    const isParentRequiredAndMissing =
      this.formData.isparent && !this.formData.parentEnitityId;
    if (form.invalid || isParentRequiredAndMissing) {
      // Mark all controls as touched to display validation messages
      Object.keys(form.controls).forEach((controlName) => {
        form.controls[controlName].markAsTouched();
      });

      this._snackBar.open('Please fill in the required fields.', 'Close', {
        horizontalPosition: 'right',
        verticalPosition: 'top',
        duration: 5000, // Auto-close after 5 seconds
      });

      return; // Prevent form submission
    }
    const requestBody = {
      ...this.formData,
    };

    if (this.isEditMode) {
      // Call update API
      //requestBody.createdBy = this.formData.createdBy;
      //requestBody.updatedBy = this.loggedInUser.user.userName;

      this.entityService.updateEntitiesDetails(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            console.log('Entity updated successfully:', response.message);
            this.fetchEntitiesList(); // Refresh the table
            this.closeForm(); // Close the form
            this._snackBar.open(response.message, 'Okay', {
              duration: 2000,
              horizontalPosition: 'center',
              verticalPosition: 'top',
            });
          } else {
            console.error('Error updating entity:', response.message);
            this._snackBar.open(response.message, 'Okay', {
              duration: 2000,
              horizontalPosition: 'center',
              verticalPosition: 'top',
            });
          }
        },
        error: (error) => {
          this._snackBar.open(error.message, 'Okay', {
            duration: 2000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
        },
      });
    } else {
      // Add new record
      //requestBody.createdBy = this.loggedInUser.user.userName;
      //requestBody.updatedBy = this.loggedInUser.user.userName;
      this.entityService.addEntities(requestBody).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            console.log('Entity added successfully:', response.message);
            this.fetchEntitiesList(); // Refresh the table after adding
            this.closeForm(); // Close the form
            this._snackBar.open(response.message, 'Okay', {
              duration: 2000,
              horizontalPosition: 'center',
              verticalPosition: 'top',
            });
          } else {
            console.error('Error adding entity:', response.message);
            this._snackBar.open(response.message, 'Okay', {
              duration: 2000,
              horizontalPosition: 'center',
              verticalPosition: 'top',
            });
          }
        },
        error: (error) => {
          const message = error?.message === 'undefined' ? 'Something went wrong' : error?.message;
          this._snackBar.open(message, 'Okay', {
            duration: 2000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
          });
        },
      });
    }
  }


  get isChildEntityDDEnabled(): boolean {
    return this.formData.isparent;
  }

  handleIsParentIdChange(): void {
    if (!this.formData.isparent) {
      // Clear the values if isParent is unchecked
      this.formData.parentEnitityId = null;
    }
  }

  // Toggle Single Row Selection
  toggleSelection(event: MatCheckboxChange, entityId: number) {
    if (event.checked) {
      this.selectedRows.add(entityId);
    } else {
      this.selectedRows.delete(entityId);
    }
  }

  // Toggle Select All Rows on Current Page
  toggleSelectAll(event: MatCheckboxChange) {
    const currentPageData = this.getCurrentPageData(); // Paginated rows
    if (event.checked) {
      currentPageData.forEach((row: any) => this.selectedRows.add(row.entityId));
    } else {
      currentPageData.forEach((row: any) => this.selectedRows.delete(row.entityId));
    }
  }

  // Check if All Rows on Current Page are Selected
  isAllPageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.every((row: any) => this.selectedRows.has(row.entityId));
  }

  // Check if Some Rows are Selected (Indeterminate State)
  isSomePageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    return currentPageData.some((row: any) => this.selectedRows.has(row.entityId)) && !this.isAllPageSelected();
  }

  /**
 * Function: deleteRecord
 * Description: Deletes an entity from the server.
 * @param {EntityRecord} record - The entity record to be deleted.
 * @returns {Promise<void>} A promise that resolves once the entity is deleted.
 */
  deleteRecord(record: EntityRecord) {
    const isParentOfAny = this.dataSource.data.some(
      (item) => item.parentEnitityId === record.entityId
    );

    if (isParentOfAny) {
      this._snackBar.open(
        `You cannot delete "${record.entityName}" because it has child entities.`,
        'Okay',
        {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        }
      );
      return;
    }
    const confirmDelete = window.confirm(
      `Are you sure you want to delete the entity: "${record.entityName}"?`
    );

    if (confirmDelete) {

      // Call the delete API
      this.entityService.deleteEntityWithId(Number(record.entityId)).subscribe({
        next: (response) => {
          if (response.isSuccess) {
            console.log(`Entity with ID ${record.entityId} deleted successfully.`);
            this.fetchEntitiesList(); // Refresh the table after deletion
            this.closeForm();
            this._snackBar.open(response.message, 'Okay', {
              duration: 2000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
            });
          }

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
  }

  // Delete Selected/all Rows from the grid
  deleteMulSelectedRows() {
    if (this.selectedRows.size === 0) {
      alert('Please select at least one row to delete');
      return;
    }

    const dialogRef = this.dialog.open(DeleteDialogComponent);

    dialogRef.afterClosed().subscribe(result => {
      if (result?.delete) {
        const entityIdsToDelete = Array.from(this.selectedRows);

        this.entityService.deleteMultipleEntities(entityIdsToDelete).subscribe({
          next: (response) => {
            console.log('Entities deleted successfully:', response.message);
            this.selectedRows.clear(); // Clear selected rows
            this.fetchEntitiesList(); // Refresh table data
            this._snackBar.open(response.message, 'Okay', {
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
          },
        });
      }
    });
  }

  refreshTable() {
    // Update data source to reflect changes
    this.dataSource.data = [...this.dataSource.data];
  }

  //opens the form when add/edit icon is clicked
  openForm() {
    this.showMaxLengthEntityCode = false
    this.showMaxLengthWarning = false
    this.formVisible = true;
    this.isEditMode = false;
    this.formData = { code: '', entityName: '', entityAddress: '', countryId: null, cityId: null, isparent: false, selected: false, entitylocation: '', parentEnitityId: 0, createdBy: '', updatedBy: '' };
  }

  //closes the form when save/cancel button is clicked
  closeForm() {
    this.formVisible = false;
    this.formData = { code: '', entityName: '', entityAddress: '', countryId: null, cityId: null, isparent: false, selected: false, entitylocation: '', parentEnitityId: 0, createdBy: '', updatedBy: ''};
    this.isEditMode = false;
  }

  // edit the form when edit icon is clicked
  editRecord(record: EntityRecord) {
    this.showMaxLengthWarning = false
    this.showMaxLengthEntityCode = false

    this.isEditMode = true;
    this.formVisible = true;
    this.formData = { ...record };
    this.filterCities(); // Populate cities based on selected country
    this.updateParentEntityOptions();
  }
  updateParentEntityOptions() {
    const currentParentId = this.formData.parentEnitityId;

    this.entitiesOptions = this.dataSource.data
      .filter((entity: EntityRecord) =>
        (!entity.isparent || entity.entityId === currentParentId) &&
        entity.entityId !== this.formData.entityId
      )
      .map((item: any) => ({
        entityId: item.entityId!,
        entityName: item.entityName,
        baseCurrencyId: item.BaseCurrencyId
      }));
  }
  /**
 * Function: exportEntities
 * Description: export the entities in a csv file.
 */
  // exportEntities() {
  //   this.entityService.exportEntities().subscribe({
  //     next: (response: Blob) => {
  //       console.log('Blob resp: ', response);
  //       const url = window.URL.createObjectURL(response);
  //       const anchor = document.createElement('a');
  //       anchor.href = url;
  //       anchor.download = 'Entities.xlsx';
  //       document.body.appendChild(anchor);
  //       anchor.click();
  //       document.body.removeChild(anchor);
  //       window.URL.revokeObjectURL(url);
  //       this._snackBar.open('Export Entities Successfully.', 'Okay', {
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
  exportEntities() {
    const entityIdsToDelete = Array.from(this.selectedRows);
    this.entityService.exportEntities(entityIdsToDelete).subscribe({
      next: (response: Blob) => {
        console.log('Blob resp: ', response);
        const url = window.URL.createObjectURL(response);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'Entities.xlsx';
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);
        window.URL.revokeObjectURL(url);
        this._snackBar.open('Export Entities Successfully.', 'Okay', {
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
  /**
 * Function: downloadTemplate
 * Description: download the csv template to fill the entities in bulk.
 */
  downloadTemplate() {
    if (!this.isDownloading) {
      this.isDownloading = true;
      this.message = "Please wait, template is downloading...";
      this.entityService.downloadTemplate().subscribe({
        next: (response) => {
          this.isDownloading = false;
          const blob = new Blob([response], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
          });
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'Entities-Template.xlsx'; // Filename for the download
          a.click();
          window.URL.revokeObjectURL(url);
          this._snackBar.open('Entities Template Download Successfully.', 'Okay', {
            duration: 2000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
        error: (error: any) => {
          this.isDownloading = false
          this._snackBar.open('Failed to Download Template', 'Okay', {
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
      });
    }
  }

  /**
 * Function: onFileSelected
 * Description: select the csv template to upload bulk entities.
 */
  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      this.importEntities(this.selectedFile);
    } else {
      this._snackBar.open('Please select a file first.', 'Okay', {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
      });
    }
  }

  /**
   * Function: importEntities
   * Description: import the csv template to upload bulk entities.
   */
  importEntities(selectedFile: File) {
    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    this.createdBy = this.loggedInUser.user.userName;
    this.isUploading = true;
    this.message = "Uploading file, please wait...";
    this.entityService.importEntities(selectedFile, this.createdBy).subscribe({
      next: (response) => {
        this.isUploading = false;
        this.fetchEntitiesList();
        this.refreshTable();
        this._snackBar.open(response.message, 'Okay', {
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (error) => {
        this.isUploading = false;
        this._snackBar.open(error.message, 'Okay', {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
    });
  }

  /**
   * Ensures that an entity cannot be selected as its own parent.
   */
  validateParentSelection() {
    const isChild = this.formData.isparent;
    const selectedEntityId = this.formData.entityId;
    const selectedParentId = this.formData.parentEnitityId;

    if (isChild && selectedEntityId && selectedEntityId == selectedParentId) {

      this._snackBar.open("An entity cannot be its own parent.", "Okay", {
        duration: 3000,
        horizontalPosition: "right",
        verticalPosition: "top",
      });
      setTimeout(() => {
        this.formData.parentEnitityId = 0; // Reset parent selection
        this.cdr.detectChanges(); // Force UI update
      });
    }
  }
}



