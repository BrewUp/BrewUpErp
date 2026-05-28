import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CustomerService } from '../../services/customer.service';
import { CustomerJson } from '../../models/customer.model';
import { AddCustomerDialogComponent } from './add-customer-dialog.component';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatDialogModule,
  ],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss',
})
export class CustomerListComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  readonly customers = signal<CustomerJson[]>([]);
  readonly totalRecords = signal(0);
  readonly isLoading = signal(false);

  pageIndex = 0;
  pageSize = 10;

  readonly displayedColumns = ['ragioneSociale', 'partitaIva', 'consumerLevel', 'citta', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.customerService.getCustomers(this.pageIndex + 1, this.pageSize).subscribe({
      next: (result) => {
        this.customers.set(result.results ?? []);
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
    const ref = this.dialog.open(AddCustomerDialogComponent, { width: '600px', disableClose: true });
    ref.afterClosed().subscribe((created) => {
      if (created) this.load();
    });
  }

  editCustomer(customer: CustomerJson): void {
    const ref = this.dialog.open(AddCustomerDialogComponent, {
      width: '600px',
      disableClose: true,
      data: { customer },
    });
    ref.afterClosed().subscribe((updated) => {
      if (updated) this.load();
    });
  }

  deleteCustomer(customer: CustomerJson): void {
    if (!confirm(`Delete customer "${customer.ragioneSociale}"?`)) return;
    this.customerService.deleteCustomer(customer.customerId).subscribe({
      next: () => {
        this.notifications.success('Customer deleted');
        this.load();
      },
    });
  }
}
