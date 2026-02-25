import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tab-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="tab-bar">
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    .tab-bar {
      display: flex;
      gap: 32px;
      padding: 0 24px;
      background: var(--surface);
      border-bottom: 1px solid var(--border);
      position: sticky;
      top: 0;
      z-index: 10;
    }

    ::ng-content > .tab-item {
      padding: 16px 4px;
      color: var(--text-2);
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
      position: relative;
      transition: var(--transition);
      white-space: nowrap;

      &:hover {
        color: var(--text-1);
      }

      &.active {
        color: var(--indigo);
        
        &::after {
          content: '';
          position: absolute;
          bottom: -1px;
          left: 0;
          right: 0;
          height: 2px;
          background: var(--indigo);
          box-shadow: 0 0 8px var(--indigo);
        }
      }
    }
  `]
})
export class TabBarComponent { }
