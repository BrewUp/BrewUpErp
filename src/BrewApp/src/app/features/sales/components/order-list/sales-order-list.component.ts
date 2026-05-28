import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { SalesService } from '../../services/sales.service';
import { SalesOrderJson } from '../../models/sales-order.model';
import { CreateOrderDialogComponent } from '../create-order-dialog/create-order-dialog.component';

@Component({
  selector: 'app-sales-order-list',
  standalone: true,
  imports: [
    MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatDialogModule, MatChipsModule,
    RouterLink, DatePipe,
  ],
  templateUrl: './sales-order-list.component.html',
  styleUrl: './sales-order-list.component.scss',
})
export class SalesOrderListComponent implements OnInit {
  private readonly salesService = inject(SalesService);
  private readonly dialog = inject(MatDialog);

  readonly orders = signal<SalesOrderJson[]>([]);
  readonly totalRecords = signal(0);
  readonly isLoading = signal(false);

  pageIndex = 0;
  pageSize = 10;

  readonly displayedColumns = ['orderNumber', 'orderDate', 'customerName', 'deliveryDate', 'rows', 'status', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.salesService.getSalesOrders(this.pageIndex + 1, this.pageSize).subscribe({
      next: (result) => {
        this.orders.set(result.results ?? []);
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

  openCreateDialog(): void {
    const ref = this.dialog.open(CreateOrderDialogComponent, { width: '600px', disableClose: true });
    ref.afterClosed().subscribe((created) => {
      if (created) this.load();
    });
  }
}
