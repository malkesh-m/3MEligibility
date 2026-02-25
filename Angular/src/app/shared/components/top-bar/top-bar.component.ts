import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  template: `
    <div class="top-bar">
      <div class="title-group">
        <h2 class="title">{{ title }}</h2>
      </div>

      <div class="search" *ngIf="showSearch">
        <mat-icon>search</mat-icon>
        <input
          type="text"
          [placeholder]="searchPlaceholder"
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearchChange($event)"
        />
      </div>

      <div class="actions">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .top-bar {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px 24px;
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      position: sticky;
      top: 0;
      z-index: 20;
    }

    .title-group {
      flex: 0 0 auto;
    }

    .title {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
      color: var(--text-1);
    }

    .search {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 6px 10px;
      border: 1px solid var(--border);
      border-radius: 8px;
      background: var(--surface-2, #fff);
      flex: 1 1 auto;
      min-width: 220px;
      max-width: 420px;
    }

    .search input {
      border: none;
      outline: none;
      width: 100%;
      font-size: 14px;
      background: transparent;
      color: var(--text-1);
    }

    .actions {
      margin-left: auto;
      display: flex;
      align-items: center;
      gap: 12px;
    }
  `]
})
export class TopBarComponent {
  @Input() title: string = '';
  @Input() searchPlaceholder: string = 'Search';
  @Input() showSearch: boolean = true;
  @Input() searchTerm: string = '';

  @Output() searchTermChange = new EventEmitter<string>();
  @Output() searchChange = new EventEmitter<string>();

  onSearchChange(value: string): void {
    this.searchTermChange.emit(value);
    this.searchChange.emit(value);
  }
}
