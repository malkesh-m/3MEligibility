import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ParameterService } from '../../../core/services/setting/parameter.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';

type TabKeys = 'product' | 'customer';

@Component({
  selector: 'app-parameter-binding',
  standalone: false,
  templateUrl: './parameter-binding.component.html',
  styleUrls: ['./parameter-binding.component.scss']
})
export class ParameterBindingComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['parameterName', 'inputAlias', 'actions'];
  dataSource = new MatTableDataSource<any>([]);
  modifiedParameters = new Set<number>();
  parameters: any[] = [];
  sourceParameters: any[] = [];
  activeTab: TabKeys = 'product';
  searchTerms: { [key: string]: string } = {
    customer: '',
    product: ''
  };
  isLoading: boolean = false;
  message: string = "";

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private parameterService: ParameterService, private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    // Custom filter predicate that considers both search term AND active tab
    this.dataSource.filterPredicate = (data: any, filter: string) => {
      const searchTerm = this.searchTerms[this.activeTab]?.toLowerCase() || '';
      const tabIdentifier = this.activeTab === 'customer' ? 1 : 2;

      // Check if parameter matches the current tab
      const matchesTab = data.identifier === tabIdentifier;

      // Check if parameter matches search term
      const matchesSearch = !searchTerm ||
        data.name.toLowerCase().includes(searchTerm) ||
        (data.description && data.description.toLowerCase().includes(searchTerm));

      return matchesTab && matchesSearch;
    };

    this.loadData();
  }

  ngAfterViewInit(): void {
    // Set paginator and sort after view initialization
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  async loadData() {
    this.isLoading = true;
    this.message = "";

    try {
      // Load system parameters
      const sourceResp = await firstValueFrom(
        this.parameterService.getSystemParameters()
      );
      const rawSource = sourceResp.data || sourceResp;
      this.sourceParameters = Array.isArray(rawSource)
        ? rawSource.map((p: any) => ({ ...p, identifier: 2 }))
        : [];

      if (!Array.isArray(this.sourceParameters)) {
        console.error('System parameters is not an array:', this.sourceParameters);
        this.sourceParameters = [];
      }

      // Load parameters
      const paramResp = await firstValueFrom(
        this.parameterService.getParameters()
      );
      const allParams = paramResp.data || paramResp;
      this.parameters = Array.isArray(allParams) ? allParams : [];

      // Load existing bindings
      const bindingResp = await firstValueFrom(
        this.parameterService.getParameterBindings()
      );
      const bindings = bindingResp.data || bindingResp || [];

      // Map bindings to source parameters
      this.mapCurrentBindings(bindings);

      // Set data source
      this.dataSource.data = this.sourceParameters;

      // Apply initial filter to show only active tab data
      this.applyFilter();

      // this.message = "Data loaded successfully";

    } catch (err: any) {
      console.error('Error loading data', err);
      this.message = "Error loading data";
      this.snackBar.open(
        err?.error?.message || 'Failed to load data. Please try again.',
        'Close',
        { duration: 5000 }
      );
    } finally {
      this.isLoading = false;
    }
  }

  // Map existing bindings to source parameters
  mapCurrentBindings(bindings: any[]) {
    if (!Array.isArray(bindings)) {
      console.error('Bindings is not an array:', bindings);
      return;
    }

    this.sourceParameters.forEach(source => {
      const binding = bindings.find(b =>
        b.systemParameter?.toLowerCase() === source.name?.toLowerCase() ||
        b.systemParameterId === source.id
      );

      if (binding && binding.mappedParameterId) {
        source.selectedParamId = binding.mappedParameterId;
        source.originalParamId = binding.mappedParameterId;
      } else {
        source.selectedParamId = null;
        source.originalParamId = null;
      }
    });
  }

  // Filter available parameters for the dropdown based on active tab
  getFilteredParameters() {
    const tabIdentifier = this.activeTab === 'customer' ? 1 : 2;
    return this.parameters.filter(p => p.identifier === tabIdentifier);
  }

  switchTab(tab: TabKeys): void {
    this.activeTab = tab;
    // Reapply filter when switching tabs
    this.applyFilter();
  }

  applyFilter() {
    // Trigger filter by setting a dummy value
    // The filterPredicate will handle both tab and search filtering
    const searchTerm = this.searchTerms[this.activeTab]?.toLowerCase() || '';
    this.dataSource.filter = searchTerm + '_' + this.activeTab; // Include tab to force refresh

    // Reset to first page when filtering
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  markAsModified(element: any) {
    // Only mark if value actually changed
    if (element.selectedParamId !== element.originalParamId) {
      this.modifiedParameters.add(element.id);
    } else {
      this.modifiedParameters.delete(element.id);
    }
  }

  isModified(element: any): boolean {
    return this.modifiedParameters.has(element.id);
  }

  saveParameter(sourceElement: any) {
    const newParamId = sourceElement.selectedParamId;
    const oldParamId = sourceElement.originalParamId;

    // No change, nothing to save
    if (newParamId === oldParamId) {
      this.modifiedParameters.delete(sourceElement.id);
      this.snackBar.open('No changes to save', 'Close', { duration: 2000 });
      return;
    }

    this.isLoading = true;

    const bindingModel = {
      systemParameterId: sourceElement.id,
      mappedParameterId: newParamId // can be null if clearing
    };

    console.log('Saving binding', bindingModel);

    this.parameterService.saveParameterBinding(bindingModel).subscribe({
      next: (response) => {
        console.log('Save successful', response);
        this.snackBar.open('Binding updated successfully', 'Close', { duration: 3000 });

        // Update state to reflect saved changes
        sourceElement.originalParamId = newParamId;
        this.modifiedParameters.delete(sourceElement.id);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error saving binding', err);
        this.snackBar.open(
          err?.error?.message || 'Error saving binding. Please try again.',
          'Close',
          { duration: 5000 }
        );
        this.isLoading = false;
      }
    });
  }
  cancelChanges(element: any) {
    // Revert the selected value to the original
    element.selectedParamId = element.originalParamId;

    // Remove from modified set
    this.modifiedParameters.delete(element.id);
  }
  // Optional: Save all modified parameters at once
  saveAllModified() {
    if (this.modifiedParameters.size === 0) {
      this.snackBar.open('No changes to save', 'Close', { duration: 2000 });
      return;
    }

    const modifiedElements = this.sourceParameters.filter(p =>
      this.modifiedParameters.has(p.id)
    );

    this.isLoading = true;
    let savedCount = 0;
    let errorCount = 0;

    modifiedElements.forEach((element, index) => {
      this.parameterService.saveParameterBinding({
        systemParameterId: element.id,
        mappedParameterId: element.selectedParamId
      }).subscribe({
        next: () => {
          element.originalParamId = element.selectedParamId;
          this.modifiedParameters.delete(element.id);
          savedCount++;

          // Check if all saves are complete
          if (savedCount + errorCount === modifiedElements.length) {
            this.isLoading = false;
            this.snackBar.open(
              `${savedCount} binding(s) saved successfully${errorCount > 0 ? `, ${errorCount} failed` : ''}`,
              'Close',
              { duration: 3000 }
            );
          }
        },
        error: (err) => {
          console.error('Error saving binding', err);
          errorCount++;

          if (savedCount + errorCount === modifiedElements.length) {
            this.isLoading = false;
            this.snackBar.open(
              `${savedCount} binding(s) saved${errorCount > 0 ? `, ${errorCount} failed` : ''}`,
              'Close',
              { duration: 5000 }
            );
          }
        }
      });
    });
  }
}