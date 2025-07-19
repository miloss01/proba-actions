import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from 'app/infrastructure/material/material.module';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [CommonModule, FormsModule, MaterialModule],
  templateUrl: './paginator.component.html',
  styleUrls: ['./paginator.component.css']
})
export class PaginatorComponent {
  @Input() currentPage: number = 1;  
  @Input() totalItems: number = 0;  
  @Input() pageSize: number = 2;     

  @Output() pageChange = new EventEmitter<number>();   
  @Output() pageSizeChange = new EventEmitter<number>();

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  onPageChange(offset: number) {
    const newPage = this.currentPage + offset;
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.pageChange.emit(newPage);
    }
  }

  onPageSizeChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    const newPageSize = Number(target.value);
    this.pageSizeChange.emit(newPageSize);
  }
}
