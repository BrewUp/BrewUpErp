import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { DecimalPipe } from '@angular/common';
import { DashboardService } from '../../services/dashboard.service';
import { SalesByCustomerJson, SalesByProductsJson } from '../../models/dashboard.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatTabsModule, DecimalPipe,
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);

  // Sales by Customer
  readonly customerRows = signal<SalesByCustomerJson[]>([]);
  readonly customerTotal = signal(0);
  readonly isLoadingCustomers = signal(false);
  customerPageIndex = 0;
  customerPageSize = 10;
  readonly customerColumns = ['customerName', 'year', 'totalSales', 'currency'];

  // Sales by Products
  readonly productRows = signal<SalesByProductsJson[]>([]);
  readonly productTotal = signal(0);
  readonly isLoadingProducts = signal(false);
  productPageIndex = 0;
  productPageSize = 10;
  readonly productColumns = ['productName', 'year', 'totalSales', 'currency', 'quantity', 'unitOfMeasure'];

  ngOnInit(): void {
    this.loadCustomers();
    this.loadProducts();
  }

  loadCustomers(): void {
    this.isLoadingCustomers.set(true);
    this.dashboardService.getSalesByCustomer(this.customerPageIndex + 1, this.customerPageSize).subscribe({
      next: (result) => {
        this.customerRows.set(result.results ?? []);
        this.customerTotal.set(result.totalRecords ?? 0);
        this.isLoadingCustomers.set(false);
      },
      error: () => this.isLoadingCustomers.set(false),
    });
  }

  loadProducts(): void {
    this.isLoadingProducts.set(true);
    this.dashboardService.getSalesByProducts(this.productPageIndex + 1, this.productPageSize).subscribe({
      next: (result) => {
        this.productRows.set(result.results ?? []);
        this.productTotal.set(result.totalRecords ?? 0);
        this.isLoadingProducts.set(false);
      },
      error: () => this.isLoadingProducts.set(false),
    });
  }

  onCustomerPageChange(event: PageEvent): void {
    this.customerPageIndex = event.pageIndex;
    this.customerPageSize = event.pageSize;
    this.loadCustomers();
  }

  onProductPageChange(event: PageEvent): void {
    this.productPageIndex = event.pageIndex;
    this.productPageSize = event.pageSize;
    this.loadProducts();
  }
}
