import { Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PurchaseService } from '../../services/purchase.service';
import { BeerService } from '../../../master-data/services/beer.service';
import { BeerJson } from '../../../master-data/models/beer.model';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-create-purchase',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatButtonModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatIconModule, MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './create-purchase.component.html',
  styleUrl: './create-purchase.component.scss',
})
export class CreatePurchaseComponent implements OnInit {
  private readonly purchaseService = inject(PurchaseService);
  private readonly beerService = inject(BeerService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly beers = signal<BeerJson[]>([]);
  readonly isSaving = signal(false);

  readonly form = this.fb.group({
    supplierId: ['', Validators.required],
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
    const v = this.form.getRawValue();
    const rows = this.rows.controls.map((ctrl) => {
      const rv = ctrl.value;
      return {
        beerId: rv.beerId,
        beerName: this.getBeerName(rv.beerId),
        quantity: { value: rv.quantityValue, unitOfMeasure: rv.quantityUnit },
        price: { value: rv.priceValue, currency: rv.priceCurrency },
      };
    });

    this.isSaving.set(true);
    this.purchaseService.createPurchaseOrder({
      supplierId: v.supplierId!,
      rows,
    }).subscribe({
      next: () => {
        this.notifications.success('Purchase order created');
        this.resetForm();
        this.isSaving.set(false);
      },
      error: () => this.isSaving.set(false),
    });
  }

  resetForm(): void {
    this.form.reset();
    while (this.rows.length > 1) this.rows.removeAt(1);
    this.rows.at(0).reset({ quantityValue: 1, quantityUnit: 'unit', priceValue: 0, priceCurrency: 'EUR' });
  }
}
