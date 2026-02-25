import { Component, inject, ViewChild, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { ListsService } from '../../../core/services/setting/lists.service';
import { EntityService } from '../../../core/services/setting/entity.service';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { DeleteDialogComponent } from '../../../core/components/delete-dialog/delete-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { NgForm } from '@angular/forms';
import { UtilityService } from '../../../core/services/utility/utils';
import { AuthService } from '../../../core/services/auth/auth.service';
import { MatSort } from '@angular/material/sort';
import { PermissionsService } from '../../../core/services/setting/permission.service';
import { TranslateService } from '@ngx-translate/core';
import { HeaderTitleService } from '../../../core/services/header-title.service';

@Component({
  selector: 'app-lists',
  standalone: false,

  templateUrl: './lists.component.html',
  styleUrl: './lists.component.scss'
})
export class ListsComponent implements OnInit {
  private _snackBar = inject(MatSnackBar);
  @ViewChild('listsPaginator') listsPaginator!: MatPaginator;
  @ViewChild('listItemsPaginator') listItemsPaginator!: MatPaginator;
  @ViewChild('listsSort') listsSort!: MatSort;
  @ViewChild('listItemsSort') listItemsSort!: MatSort;
  parameters: any[] = [];
  displayedColumns: string[] = ['select', 'listName', 'createdBy', 'updatedBy', 'actions'];
  listItemdisplayedColumns: string[] = ['select', 'code', 'listName', 'itemName', 'createdBy', 'updatedBy', 'actions'];
  dataSource = new MatTableDataSource<any>([]);
  listItemDataSource = new MatTableDataSource<any>([]);
  searchTerm: string = '';
  menuVisible: boolean = false;
  formVisible: boolean = false;
  isEditMode: boolean = false;
  activeTab: string = 'lists';
  listItemFormVisible: boolean = false;
  listItemisEditMode: boolean = false;

  formData: any = {
    listName: '',
    entityId: null,
    listId: 0
  };

  listItemFormData: any = {
    itemName: '',
    listId: null,
    itemId: 0,
    code: ''
  };
  selectedRows: Set<number> = new Set();
  entities: any[] = [];
  listItems: any[] = [];
  searchTerms: { [key: string]: string } = {
    lists: '',
    listitems: ''
  };
  selectedFile: File | null = null;
  isDownloading: boolean = false;
  isLoading: boolean = false; // Show loader on page load
  isUploading: boolean = false;
  message: string = this.translate.instant("Loading data, please wait...");
  loggedInUser: any = null;
  createdBy: string = '';

  constructor(
    private snackBar: MatSnackBar,
    private listsService: ListsService,
    private entityService: EntityService,
    private dialog: MatDialog,
    private utilityService: UtilityService,
    private authService: AuthService,
    private PermissionsService: PermissionsService,
    private titleService: Title,
    private location: Location,
    private router: Router,
    private route: ActivatedRoute,
    private translate: TranslateService,
    private headerTitleService: HeaderTitleService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeTab = params['tab']?.toLowerCase();
      }
    });
    // this.fetchEntities();
    this.updateTitle();

    if (this.activeTab === 'listitems') {
      this.fetchListItems();
      this.fetchLists(); // Still fetch lists to populate dropdowns
    } else {
      this.fetchLists();
    }
  }

  get activeTabTitle(): string {
    switch (this.activeTab?.toLowerCase()) {
      case 'lists':
        return this.translate.instant('List - Managed List');
      case 'listitems':
        return this.translate.instant('List - List Items');
      default:
        return this.translate.instant('List - Managed List');
    }
  }

  updateTitle() {
    this.titleService.setTitle(`${this.activeTabTitle} - 3M Eligibility`);
    this.headerTitleService.setTitle(this.activeTabTitle);
  }

  getAddPermission(): boolean {
    return (this.activeTab?.toLowerCase() === 'lists' && this.hasPermission('Permissions.ManagedList.Create')) ||
      (this.activeTab?.toLowerCase() === 'listitems' && this.hasPermission('Permissions.ListItem.Create'));
  }

  getDeletePermission(): boolean {
    return (this.activeTab?.toLowerCase() === 'lists' && this.hasPermission('Permissions.ManagedList.Delete')) ||
      (this.activeTab?.toLowerCase() === 'listitems' && this.hasPermission('Permissions.ListItem.Delete'));
  }

  getImportPermission(): boolean {
    return (this.activeTab?.toLowerCase() === 'lists' && this.hasPermission('Permissions.ManagedList.Import')) ||
      (this.activeTab?.toLowerCase() === 'listitems' && this.hasPermission('Permissions.ListItem.Import'));
  }

  getExportPermission(): boolean {
    return (this.activeTab?.toLowerCase() === 'lists' && this.hasPermission('Permissions.ManagedList.Export')) ||
      (this.activeTab?.toLowerCase() === 'listitems' && this.hasPermission('Permissions.ListItem.Export'));
  }

  hasPermission(permissionId: string): boolean {
    return this.PermissionsService.hasPermission(permissionId);
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

  toggleColumnlistItem(column: string, afterColumn: string) {
    const index = this.listItemdisplayedColumns.indexOf(column);

    if (index > -1) {
      // Remove column if already visible
      this.listItemdisplayedColumns.splice(index, 1);
    } else {
      // Find the index of the afterColumn and insert right after it
      const afterIndex = this.listItemdisplayedColumns.indexOf(afterColumn);
      if (afterIndex !== -1) {
        this.listItemdisplayedColumns.splice(afterIndex + 1, 0, column);
      } else {
        this.listItemdisplayedColumns.push(column); // Default push if not found
      }
    }

    this.listItemdisplayedColumns = [...this.listItemdisplayedColumns]; // Ensure reactivity
  }

  switchTab(tab: string): void {
    this.activeTab = tab?.toLowerCase();

    const urlTree = this.router.createUrlTree([], {
      relativeTo: this.route,
      queryParams: { tab: tab },
      queryParamsHandling: 'merge'
    });
    this.location.go(urlTree.toString());
    this.updateTitle();

    if (tab?.toLowerCase() === 'lists') {
      this.fetchLists();
    } else if (tab?.toLowerCase() === 'listitems') {
      this.fetchListItems();
    }
    this.searchTerm = this.searchTerms[this.activeTab] || '';
  }

  fetchEntities(): void {
    this.entityService.getEntitiesList().subscribe({
      next: (response) => {
        this.entities = response.data; // Store entities for real-time lookup
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        console.error('Error fetching entities:', error);
      },
    });
  }

  fetchListItems(): void {
    this.isLoading = true;
    this.listsService.fetchListItems().subscribe({
      next: (response) => {


        // Sorting the response data based on lastModifiedDateTime in descending order
        const sortedData = response.data.sort((a: any, b: any) =>
          new Date(b.lastModifiedDateTime).getTime() - new Date(a.lastModifiedDateTime).getTime()
        );
        this.listItemDataSource.data = sortedData.map((item: any) => ({
          ...item,
        }));
        this.listItemDataSource.paginator = this.listItemsPaginator;
        this.listItemDataSource.sort = this.listItemsSort;
        this.isLoading = false;
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.isLoading = false;
        console.error('Error fetching list items:', error);
      },
    });
  }

  getEntityName(entityId: number, list: any): string {
    const entity = this.entities.find((e) => e.entityId === entityId);
    list.entityName = entity ? entity.entityName : this.translate.instant('Unknown');
    return entity ? entity.entityName : this.translate.instant('Unknown'); // Return the corresponding entityName or 'Unknown'
  }

  getListName(listId: number, list: any) {
    const listName = this.listItems.find((list => list.listId === listId))
    list.listName = listName ? listName.listName : this.translate.instant('Unknown');
    return listName ? listName.listName : this.translate.instant('Unknown');
  }

  fetchLists(): void {
    this.isLoading = true;
    this.listsService.fetchAllLists().subscribe({
      next: (response) => {

        this.dataSource.data = this.listItems = response.data;
        this.dataSource.paginator = this.listsPaginator;
        this.dataSource.sort = this.listsSort;
        this.isLoading = false;
      },
      error: (error) => {
        this._snackBar.open(this.translate.instant(error), this.translate.instant('Okay'), {
          horizontalPosition: 'right',
          verticalPosition: 'top', duration: 3000
        });
        this.isLoading = false;
        console.error('Error fetching lists:', error);
      },
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement)?.value?.trim()?.toLowerCase();
    if (this.activeTab?.toLowerCase() === 'lists') {
      this.dataSource.filter = filterValue;
    } else if (this.activeTab?.toLowerCase() === 'listitems') {
      this.listItemDataSource.filter = filterValue;
    }
  }

  toggleMenu(): void {
    this.menuVisible = !this.menuVisible;
  }

  closeMenu(): void {
    this.menuVisible = false;
  }

  addList(): void {
    if (this.activeTab?.toLowerCase() == 'lists') {
      this.formVisible = true;
      this.isEditMode = false;
      this.formData = {
        listName: '',
        entityId: null,
      };
    }
    if (this.activeTab?.toLowerCase() == 'listitems') {
      this.listItemFormVisible = true;
      this.listItemisEditMode = false;
      this.listItemFormData = {
        listId: null,
        itemName: '',
        code: ''
      };
    }
  }

  editList(list: any): void {
    if (this.activeTab?.toLowerCase() == 'lists') {
      this.formVisible = true;
      this.isEditMode = true;
      this.formData = { ...list };
    }
    if (this.activeTab?.toLowerCase() == 'listitems') {
      this.listItemFormVisible = true;
      this.listItemisEditMode = true;
      this.listItemFormData = { ...list };
    }
  }

  submitEditForm(form: NgForm): void {

    this.authService.currentUser$.subscribe((user) => {
      this.loggedInUser = user;
    });
    if (form.invalid) {
      // Mark all controls as touched to display validation messages
      Object.keys(form.controls).forEach((controlName) => {
        form.controls[controlName].markAsTouched();
      });

      this._snackBar.open(this.translate.instant('Please fill in the required fields.'), this.translate.instant('Close'), {
        horizontalPosition: 'right',
        verticalPosition: 'top', duration: 3000
        //duration: 5000, // Auto-close after 5 seconds
      });

      return; // Prevent form submission
    }

    if (this.activeTab?.toLowerCase() === 'lists') {
      if (this.isEditMode) {
        // this.formData.createdBy = this.formData.createdBy;
        // this.formData.updatedBy = this.loggedInUser.user.userName;
        this.listsService.updateLists(this.formData).subscribe({
          next: (response) => {
            this.formVisible = false;
            this.fetchLists();
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          },
          error: (error) => {
            console.error('Error updating parameter:', error)
            this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        });
      } else {
        // this.formData.createdBy = this.loggedInUser.user.userName;
        // this.formData.updatedBy = this.loggedInUser.user.userName;


        this.listsService.addLists(this.formData).subscribe({
          next: (response) => {
            this.formVisible = false;
            this.fetchLists();
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          },
          error: (error) => {
            console.error('Error updating parameter:', error),
              this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
          }
        });
      }

    } else if (this.activeTab?.toLowerCase() === 'listitems') {
      if (this.listItemisEditMode) {
        // this.listItemFormData.createdBy = this.listItemFormData.createdBy;
        // this.listItemFormData.updatedBy = this.loggedInUser.user.userName;
        this.listsService.updateListsItem(this.listItemFormData).subscribe({
          next: (response) => {
            this.listItemFormVisible = false;
            this.fetchListItems();
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          },
          error: (error) => {
            console.error('Error updating parameter:', error)
            this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        });
      } else {
        // this.listItemFormData.createdBy = this.loggedInUser.user.userName;
        // this.listItemFormData.updatedBy = this.loggedInUser.user.userName;
        this.listItemFormData.entityId = 0;
        this.listsService.addListsItem(this.listItemFormData).subscribe({
          next: (response) => {
            this.listItemFormVisible = false;
            this.fetchListItems();
            this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          },
          error: (error) => {
            console.error('Error updating parameter:', error)
            this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
              horizontalPosition: 'right',
              verticalPosition: 'top', duration: 3000
            });
          }
        });
      }
    }

  }

  deleteList(list: any): void {
    // if (this.activeTab == 'lists') {
    //   const confirmDelete = window.confirm(
    //     `Are you sure you want to delete the list: "${list.listName}"?`
    //   );

    if (this.activeTab?.toLowerCase() === 'lists') {

      var matdialogRef = this.dialog.open(DeleteDialogComponent, {
        data: {
          title: this.translate.instant('Confirm'),
          message: this.translate.instant('Are you sure you want to delete the list: "{{list}}"?', { list: list.listName })
        }
      });

      matdialogRef.afterClosed().subscribe(result => {
        if (result?.delete) {
          this.listsService.deleteParameter(list.listId).subscribe({
            next: (response) => {
              // Remove the list locally after successful API call
              this.dataSource.data = this.dataSource.data.filter(
                (item) => item.listId !== list.listId
              );
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Close'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
              this.selectedRows.clear();

            },
            error: (error) => {
              this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Close'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
              console.error('Error deleting list:', error);
              this.selectedRows.clear();

            },
          });
        }
      });
    }


    if (this.activeTab?.toLowerCase() == 'listitems') {
      var matdialogRef = this.dialog.open(DeleteDialogComponent, {
        data: {
          title: this.translate.instant('Confirm'),
          message: this.translate.instant('Are you sure you want to delete the list item: "{{list}}"?', { list: list.listName })
        }
      });
      matdialogRef.afterClosed().subscribe(result => {
        if (result?.delete) {
          this.listsService.deleteListItem(list.itemId).subscribe({
            next: (response) => {
              this.listItemDataSource.data = this.listItemDataSource.data.filter(
                (item) => item.itemId !== list.itemId
              );
              this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Close'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
            },
            error: (error) => {
              this._snackBar.open(this.translate.instant('Failed to delete the list. Please try again.'), this.translate.instant('Close'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
              console.error('Error deleting list:', error);
            },
          });
        }
      });
    }


  }

  deleteSelectedLists() {
    if (this.selectedRows.size === 0) {
      this._snackBar.open(this.translate.instant('Please select at least one row to delete'), this.translate.instant('Close'), {
        horizontalPosition: 'right',
        verticalPosition: 'top', duration: 3000
      });
      return;
    }

    if (this.activeTab?.toLowerCase() == 'lists') {
      const dialogRef = this.dialog.open(DeleteDialogComponent, {
        data: {
          title: this.translate.instant('Confirm'),
          message: this.translate.instant('Are you sure you want to delete the selected records?')
        }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result?.delete) {
          const entityIdsToDelete = Array.from(this.selectedRows);

          this.listsService.deleteMultipleList(entityIdsToDelete).subscribe({
            next: (response) => {
              if (response.isSuccess) {
                this.fetchLists();
                this.selectedRows.clear();
                this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                  horizontalPosition: 'right',
                  verticalPosition: 'top', duration: 3000
                });
              }
              else {
                this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                  horizontalPosition: 'right',
                  verticalPosition: 'top', duration: 3000
                });
              }
            },
            error: (err) => {
              console.error('Error deleting parameters:', err)
              this._snackBar.open(this.translate.instant(err.message), this.translate.instant('Okay'), {
                horizontalPosition: 'right',
                verticalPosition: 'top', duration: 3000
              });
            }
          });
        }
      });
    } else {
      const dialogRef = this.dialog.open(DeleteDialogComponent, {
        data: {
          title: this.translate.instant('Confirm'),
          message: this.translate.instant('Are you sure you want to delete the selected records?')
        }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result?.delete) {
          const entityIdsToDelete = Array.from(this.selectedRows);
          this.listsService.deleteMultipleListItem(entityIdsToDelete).subscribe({
            next: (response) => {
              if (response.isSuccess) {
                this.selectedRows.clear();
                this.fetchListItems();
                this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                  horizontalPosition: 'right',
                  verticalPosition: 'top', duration: 3000
                });
              }
              else {
                this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
                  horizontalPosition: 'right',
                  verticalPosition: 'top', duration: 3000
                });
              }
            },
            error: (err) => console.error('Error deleting parameters:', err),
          });
        }
      });
    }
  }

  toggleAll(event: any): void {
    const isChecked = event.target.checked;
    if (isChecked) {
      this.dataSource.data.forEach((row) => this.selectedRows.add(row.id));
    } else {
      this.selectedRows.clear();
    }
  }

  areAllSelected(): boolean {
    return this.selectedRows.size === this.dataSource.data.length;
  }

  closeForm(): void {
    if (this.activeTab?.toLowerCase() == 'lists') {
      this.formVisible = false;
    }
    if (this.activeTab?.toLowerCase() == 'listitems') {
      this.listItemFormVisible = false;
    }
  }

  getCurrentPageData(): any[] {
    if (this.activeTab?.toLowerCase() === 'lists') {
      if (!this.listsPaginator) return [];
      const startIndex = this.listsPaginator.pageIndex * this.listsPaginator.pageSize;
      const endIndex = startIndex + this.listsPaginator.pageSize;
      return this.dataSource.filteredData.slice(startIndex, endIndex);
    } else {
      if (!this.listItemsPaginator) return [];
      const startIndex = this.listItemsPaginator.pageIndex * this.listItemsPaginator.pageSize;
      const endIndex = startIndex + this.listItemsPaginator.pageSize;
      return this.listItemDataSource.filteredData.slice(startIndex, endIndex);
    }
  }

  isAllSelected(): boolean {
    if (this.activeTab?.toLowerCase() === 'lists') {
      const data = this.dataSource.data;
      return data.length > 0 && data.every(row => this.selectedRows.has(row.listId));
    } else {
      const data = this.listItemDataSource.data;
      return data.length > 0 && data.every(row => this.selectedRows.has(row.itemId));
    }
  }

  isAllPageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    if (currentPageData.length === 0) return false;
    if (this.activeTab?.toLowerCase() === 'lists') {
      return currentPageData.every((row: any) => this.selectedRows.has(row.listId));
    } else {
      return currentPageData.every((row: any) => this.selectedRows.has(row.itemId));
    }
  }

  isSomePageSelected(): boolean {
    const currentPageData = this.getCurrentPageData();
    if (currentPageData.length === 0) return false;
    const numSelected = this.activeTab?.toLowerCase() === 'lists'
      ? currentPageData.filter((row: any) => this.selectedRows.has(row.listId)).length
      : currentPageData.filter((row: any) => this.selectedRows.has(row.itemId)).length;
    return numSelected > 0 && numSelected < currentPageData.length;
  }

  toggleSelectAll(event: MatCheckboxChange) {
    const currentPageData = this.getCurrentPageData(); // Paginated rows
    if (this.activeTab?.toLowerCase() === 'lists') {
      if (event.checked) {
        currentPageData.forEach((row: any) => this.selectedRows.add(row.listId));
      } else {
        currentPageData.forEach((row: any) => this.selectedRows.delete(row.listId));
      }
    } else {
      if (event.checked) {
        currentPageData.forEach((row: any) => this.selectedRows.add(row.itemId));
      } else {
        currentPageData.forEach((row: any) => this.selectedRows.delete(row.itemId));
      }
    }
  }

  toggleSelection(event: MatCheckboxChange, ids: number) {
    if (this.activeTab?.toLowerCase() === 'lists') {
      if (event.checked) {
        this.selectedRows.add(ids);
      } else {
        this.selectedRows.delete(ids);
      }
    } else {
      if (event.checked) {
        this.selectedRows.add(ids);
      } else {
        this.selectedRows.delete(ids);
      }
    }

  }

  downloadTemplate() {
    this.isDownloading = true;
    this.message = this.translate.instant("Please wait, template is downloading...");
    if (this.activeTab?.toLowerCase() === 'lists') {
      this.listsService.downloadListTemplate().subscribe((response) => {
        this.isDownloading = false;
        const blob = new Blob([response], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'Lists-Template.xlsx'; // Filename for the download
        a.click();
        window.URL.revokeObjectURL(url);
        this._snackBar.open(this.translate.instant('Lists Template Download Successfully.'), this.translate.instant('Okay'), {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
      });
    } else {
      this.listsService.downloadItemTemplate().subscribe((response) => {
        this.isDownloading = false;
        const blob = new Blob([response], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'ListItem-Template.xlsx'; // Filename for the download
        a.click();
        window.URL.revokeObjectURL(url);
        this._snackBar.open(this.translate.instant('ListItem Template Download Successfully.'), this.translate.instant('Okay'), {
          duration: 2000,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
      });
    }
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    event.target.value = '';
    if (this.selectedFile) {
      this.importList(this.selectedFile);
    } else {
      this._snackBar.open(this.translate.instant('Please select a file first.'), this.translate.instant('Okay'), {
        duration: 2000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
    }
  }

  importList(selectedFile: File) {
    // this.authService.currentUser$.subscribe((user) => {
    //   this.loggedInUser = user;
    // });
    // this.createdBy = this.loggedInUser.user.userName;
    this.isUploading = true;
    this.message = this.translate.instant("Uploading file, please wait...");
    if (this.activeTab?.toLowerCase() === 'lists') {
      this.listsService.importList(selectedFile).subscribe({
        next: (response) => {
          this.isUploading = false;
          this.fetchLists();
          this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
        error: (error) => {
          this.isUploading = false;
          this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
      });
    } else {
      this.listsService.importListItem(selectedFile).subscribe({
        next: (response) => {
          this.isUploading = false;
          this.fetchListItems();
          this._snackBar.open(this.translate.instant(response.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
        error: (error) => {
          this.isUploading = false;
          this._snackBar.open(this.translate.instant(error.message), this.translate.instant('Okay'), {
            horizontalPosition: 'right',
            verticalPosition: 'top', duration: 3000
          });
        },
      });
    }
  }

  sanitizeCode(event: any) {
    event.target.value = this.utilityService.sanitizeCode(event.target.value);
  }

  // ExportLists() {
  //   if (this.activeTab === 'lists') {
  //     this.listsService.ExportLists().subscribe({
  //       next: (response: Blob) => {
  //         console.log('Blob resp: ', response);
  //         const url = window.URL.createObjectURL(response);
  //         const anchor = document.createElement('a');
  //         anchor.href = url;
  //         anchor.download = 'Lists.xlsx';
  //         document.body.appendChild(anchor);
  //         anchor.click();
  //         document.body.removeChild(anchor);
  //         window.URL.revokeObjectURL(url);
  //         this._snackBar.open('Export Lists Successfully.', 'Okay', {
  //           duration: 2000,
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top',
  //         });
  //       },
  //       error: (error) => {
  //         this._snackBar.open(error.message, 'Okay', {
  //           duration: 2000,
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top',
  //         });
  //       }
  //     });
  //   } else {
  //     this.listsService.ExportListIteam().subscribe({
  //       next: (response: Blob) => {
  //         console.log('Blob resp: ', response);
  //         const url = window.URL.createObjectURL(response);
  //         const anchor = document.createElement('a');
  //         anchor.href = url;
  //         anchor.download = 'List Items.xlsx';
  //         document.body.appendChild(anchor);
  //         anchor.click();
  //         document.body.removeChild(anchor);
  //         window.URL.revokeObjectURL(url);
  //         this._snackBar.open('Export List Items Successfully.', 'Okay', {
  //           duration: 2000,
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top',
  //         });
  //       },
  //       error: (error) => {
  //         this._snackBar.open(error.message, 'Okay', {
  //           duration: 2000,
  //           horizontalPosition: 'right',
  //           verticalPosition: 'top',
  //         });
  //       }
  //     });
  //   }
  // }

  ExportLists() {
    if (this.activeTab?.toLowerCase() === 'lists') {
      const listsIdsToExport = this.selectedRows.size > 0
        ? Array.from(this.selectedRows)
        : [];
      const searchTerm = listsIdsToExport.length === 0 ? (this.searchTerms[this.activeTab] || '').trim() : '';

      this.listsService.ExportLists(listsIdsToExport, searchTerm).subscribe({
        next: (response: Blob) => {
          const url = window.URL.createObjectURL(response);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = 'Lists.xlsx';
          document.body.appendChild(anchor);
          anchor.click();
          document.body.removeChild(anchor);
          window.URL.revokeObjectURL(url);
          this._snackBar.open('Export Lists Successfully.', 'Okay', {
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
    } else {
      // For List Items tab
      const listItemIdsToExport = this.selectedRows.size > 0
        ? Array.from(this.selectedRows)
        : [];
      const searchTerm = listItemIdsToExport.length === 0 ? (this.searchTerms[this.activeTab] || '').trim() : '';

      this.listsService.ExportListIteam(listItemIdsToExport, searchTerm).subscribe({
        next: (response: Blob) => {
          const url = window.URL.createObjectURL(response);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = 'List Items.xlsx';
          document.body.appendChild(anchor);
          anchor.click();
          document.body.removeChild(anchor);
          window.URL.revokeObjectURL(url);
          this._snackBar.open('Export List Items Successfully.', 'Okay', {
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
  }

}



