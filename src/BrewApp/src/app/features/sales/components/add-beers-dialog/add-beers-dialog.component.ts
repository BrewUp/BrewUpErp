import { Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SalesService } from '../../services/sales.service';
import { BeerService } from '../../../master-data/services/beer.service';
import { BeerJson } from '../../../master-data/models/beer.model';
import { NotificationService } from '../../../../core/services/notification.service';

export interface AddBeersDialogData {
  orderId: string;
}

@Component({
  selector: 'app-add-beers-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './add-beers-dialog.component.html',
})
export class AddBeersDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<AddBeersDialogComponent>);
  private readonly data: AddBeersDialogData = inject(MAT_DIALOG_DATA);
  private readonly salesService = inject(SalesService);
  private readonly beerService = inject(BeerService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly isSaving = signal(false);
  readonly beers = signal<BeerJson[]>([]);

  readonly form = this.fb.group({
    rows: this.fb.array([this.buildRow()]),
  });

  get rows(): FormArray {
    return this.form.get('rows') as FormArray;
  }

  ngOnInit(): void {
    this.beerService.getBeers(1, 100).subscribe({
      next: (result) => this.beers.set(result.results ?? []),
    });
  }

  buildRow() {
    return this.fb.group({
      beerId: ['', Validators.required],
      quantityValue: [1, [Validators.required, Validators.min(1)]],
      quantityUnit: ['unit', Validators.required],
      priceValue: [0, [Validators.required, Validators.min(0)]],
      priceCurrency: ['EUR', Validators.required],
    });
  }

  addRow(): void {
    this.rows.push(this.buildRow());
  }

  removeRow(index: number): void {
    if (this.rows.length > 1) this.rows.removeAt(index);
  }

  getBeerName(beerId: string): string {
    return this.beers().find(b => b.beerId === beerId)?.beerName ?? '';
  }

  onBeerSelected(index: number, beerId: string): void {
    const beer = this.beers().find(b => b.beerId === beerId);
    if (beer?.price) {
      this.rows.at(index).patchValue({
        priceValue: beer.price.value,
        priceCurrency: beer.price.currency,
      });
    }
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const rows = this.rows.controls.map((ctrl) => {
      const v = ctrl.value;
      return {
        beerId: v.beerId,
        beerName: this.getBeerName(v.beerId),
        quantity: { value: v.quantityValue, unitOfMeasure: v.quantityUnit },
        price: { value: v.priceValue, currency: v.priceCurrency },
      };
    });

    this.isSaving.set(true);
    this.salesService.addBeersToOrder(this.data.orderId, {
      orderId: this.data.orderId,
      rows,
    }).subscribe({
      next: () => {
        this.notifications.success('Beers added to order');
        this.dialogRef.close(true);
      },
      error: () => this.isSaving.set(false),
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
