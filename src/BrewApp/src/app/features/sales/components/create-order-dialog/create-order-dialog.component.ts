import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SalesService } from '../../services/sales.service';
import { CustomerService } from '../../../master-data/services/customer.service';
import { CustomerJson } from '../../../master-data/models/customer.model';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-create-order-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatProgressSpinnerModule,
  ],
  templateUrl: './create-order-dialog.component.html',
})
export class CreateOrderDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<CreateOrderDialogComponent>);
  private readonly salesService = inject(SalesService);
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly isSaving = signal(false);
  readonly customers = signal<CustomerJson[]>([]);

  readonly form = this.fb.nonNullable.group({
    orderNumber: ['', Validators.required],
    orderDate: [new Date().toISOString().substring(0, 10), Validators.required],
    customerId: ['', Validators.required],
    deliveryDate: [''],
  });

  ngOnInit(): void {
    this.customerService.getCustomers(1, 100).subscribe({
      next: (result) => this.customers.set(result.results ?? []),
    });
  }

  get selectedCustomerName(): string {
    const id = this.form.value.customerId;
    return this.customers().find(c => c.customerId === id)?.ragioneSociale ?? '';
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    const customerName = this.customers().find(c => c.customerId === v.customerId)?.ragioneSociale ?? '';

    this.isSaving.set(true);
    this.salesService.createSalesOrder({
      orderNumber: v.orderNumber,
      orderDate: v.orderDate,
      customerId: v.customerId,
      customerName,
      deliveryDate: v.deliveryDate || undefined,
      rows: [],
    }).subscribe({
      next: () => {
        this.notifications.success('Sales order created');
        this.dialogRef.close(true);
      },
      error: () => this.isSaving.set(false),
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
