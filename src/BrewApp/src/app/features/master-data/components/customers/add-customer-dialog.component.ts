import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CustomerService } from '../../services/customer.service';
import { CustomerJson } from '../../models/customer.model';
import { NotificationService } from '../../../../core/services/notification.service';

export interface AddCustomerDialogData {
  customer?: CustomerJson;
}

@Component({
  selector: 'app-add-customer-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatProgressSpinnerModule,
  ],
  templateUrl: './add-customer-dialog.component.html',
})
export class AddCustomerDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<AddCustomerDialogComponent>);
  private readonly data: AddCustomerDialogData = inject(MAT_DIALOG_DATA) ?? {};
  private readonly customerService = inject(CustomerService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly isSaving = signal(false);
  readonly isEdit = !!this.data?.customer;

  readonly form = this.fb.nonNullable.group({
    ragioneSociale: ['', Validators.required],
    partitaIva: ['', Validators.required],
    via: [''],
    numeroCivico: [''],
    citta: [''],
    provincia: [''],
    cap: [''],
    nazione: [''],
  });

  ngOnInit(): void {
    const c = this.data?.customer;
    if (c) {
      this.form.patchValue({
        ragioneSociale: c.ragioneSociale,
        partitaIva: c.partitaIva,
        via: c.indirizzo?.via ?? '',
        numeroCivico: c.indirizzo?.numeroCivico ?? '',
        citta: c.indirizzo?.citta ?? '',
        provincia: c.indirizzo?.provincia ?? '',
        cap: c.indirizzo?.cap ?? '',
        nazione: c.indirizzo?.nazione ?? '',
      });
    }
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    const hasAddress = v.citta || v.via;
    const dto = {
      ragioneSociale: v.ragioneSociale,
      partitaIva: v.partitaIva,
      indirizzo: hasAddress ? {
        via: v.via, numeroCivico: v.numeroCivico,
        citta: v.citta, provincia: v.provincia, cap: v.cap, nazione: v.nazione,
      } : undefined,
    };

    this.isSaving.set(true);

    if (this.isEdit) {
      const id = this.data.customer!.customerId;
      this.customerService.updateCustomer(id, { customerId: id, ...dto }).subscribe({
        next: () => {
          this.notifications.success('Customer updated');
          this.dialogRef.close(true);
        },
        error: () => this.isSaving.set(false),
      });
    } else {
      this.customerService.createCustomer(dto).subscribe({
        next: () => {
          this.notifications.success('Customer created');
          this.dialogRef.close(true);
        },
        error: () => this.isSaving.set(false),
      });
    }
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
