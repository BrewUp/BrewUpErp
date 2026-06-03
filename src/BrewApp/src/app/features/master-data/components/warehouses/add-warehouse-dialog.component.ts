import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WarehouseMasterService } from '../../services/warehouse-master.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-add-warehouse-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatProgressSpinnerModule,
  ],
  templateUrl: './add-warehouse-dialog.component.html',
})
export class AddWarehouseDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddWarehouseDialogComponent>);
  private readonly warehouseService = inject(WarehouseMasterService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly isSaving = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.isSaving.set(true);
    this.warehouseService.createWarehouse({ name: this.form.value.name! }).subscribe({
      next: () => {
        this.notifications.success('Warehouse created');
        this.dialogRef.close(true);
      },
      error: () => this.isSaving.set(false),
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
