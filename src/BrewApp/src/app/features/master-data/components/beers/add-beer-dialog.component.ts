import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BeerService } from '../../services/beer.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-add-beer-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatProgressSpinnerModule,
  ],
  templateUrl: './add-beer-dialog.component.html',
})
export class AddBeerDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddBeerDialogComponent>);
  private readonly beerService = inject(BeerService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly isSaving = signal(false);

  readonly form = this.fb.nonNullable.group({
    beerName: ['', Validators.required],
    beerStyle: ['', Validators.required],
    alcoholByVolume: [null as number | null],
    packaging: [''],
    priceValue: [0, [Validators.required, Validators.min(0)]],
    priceCurrency: ['EUR', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    this.isSaving.set(true);
    this.beerService.createBeer({
      beerName: v.beerName,
      beerStyle: v.beerStyle,
      alcoholByVolume: v.alcoholByVolume ?? undefined,
      packaging: v.packaging || undefined,
      price: { value: v.priceValue, currency: v.priceCurrency },
      isActive: true,
    }).subscribe({
      next: () => {
        this.notifications.success('Beer created successfully');
        this.dialogRef.close(true);
      },
      error: () => this.isSaving.set(false),
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
