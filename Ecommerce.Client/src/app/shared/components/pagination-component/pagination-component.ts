import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-pagination-component',
  imports: [CommonModule],
  templateUrl: './pagination-component.html',
  styleUrl: './pagination-component.scss',
})
export class PaginationComponent {
  @Input() totalCount = 0;
  @Input() pageSize = 10;
  @Input() pageIndex = 1;
  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get pages(): number[] {
    const total = this.totalPages;
    const current = this.pageIndex;
    const delta = 2;
    const range: number[] = [];

    for (let i = Math.max(1, current - delta); i <= Math.min(total, current + delta); i++) {
      range.push(i);
    }

    return range;
  }

  onPageChange(page: number) {
    if (page === this.pageIndex || page < 1 || page > this.totalPages) return;
    this.pageChange.emit(page);
  }
}
