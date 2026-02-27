import { AfterViewInit, Directive, Input } from '@angular/core';
import { MatSort, SortDirection } from '@angular/material/sort';

@Directive({
  selector: '[appAutoSort]',
  standalone: true
})
export class AutoSortDirective implements AfterViewInit {
  @Input('appAutoSortColumn') appAutoSortColumn?: string;
  @Input('appAutoSortDirection') appAutoSortDirection: SortDirection = 'desc';

  constructor(private sort: MatSort) { }

  ngAfterViewInit() {
    this.activateSort();
  }

  private activateSort() {
    if (!this.sort || !this.sort.sortables?.size) {
      return;
    }

    queueMicrotask(() => {
      if (this.sort.active) {
        return;
      }

      const column = (this.appAutoSortColumn || this.pickDateColumn() || this.pickFirstColumn())?.trim();
      if (!column) {
        return;
      }

      const direction = this.appAutoSortDirection || 'desc';
      this.sort.active = column;
      this.sort.direction = direction;
      this.sort.sortChange.emit({ active: column, direction });
    });
  }

  private pickDateColumn(): string | undefined {
    return Array.from(this.sort.sortables.keys())
      .find(key => key && /date/i.test(key) && !/select|action/i.test(key));
  }

  private pickFirstColumn(): string | undefined {
    return Array.from(this.sort.sortables.keys())
      .find(key => key && !/select|action/i.test(key));
  }
}
