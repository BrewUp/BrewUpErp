import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { WarehouseMasterService } from '../../services/warehouse-master.service';
import { WarehouseJson } from '../../models/warehouse.model';
import { AddWarehouseDialogComponent } from './add-warehouse-dialog.component';

@Component({
  selector: 'app-warehouse-list',
  standalone: true,
  imports: [
    MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatDialogModule,
  ],
  templateUrl: './warehouse-list.component.html',
  styleUrl: './warehouse-list.component.scss',
})
export class WarehouseListComponent implements OnInit {
  private readonly warehouseService = inject(WarehouseMasterService);
  private readonly dialog = inject(MatDialog);

  readonly warehouses = signal<WarehouseJson[]>([]);
  readonly totalRecords = signal(0);
  readonly isLoading = signal(false);

  pageIndex = 0;
  pageSize = 10;

  readonly displayedColumns = ['id', 'name'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    // API uses 0-based 'page'
    this.warehouseService.getWarehouses(this.pageIndex, this.pageSize).subscribe({
      next: (result) => {
        this.warehouses.set(result.results ?? []);
        this.totalRecords.set(result.totalRecords ?? 0);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  openAddDialog(): void {
    const ref = this.dialog.open(AddWarehouseDialogComponent, { width: '400px', disableClose: true });
    ref.afterClosed().subscribe((created) => {
      if (created) this.load();
    });
  }
}
