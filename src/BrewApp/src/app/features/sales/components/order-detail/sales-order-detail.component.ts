import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { SalesService } from '../../services/sales.service';
import { SalesOrderJson } from '../../models/sales-order.model';
import { AddBeersDialogComponent } from '../add-beers-dialog/add-beers-dialog.component';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-sales-order-detail',
  standalone: true,
  imports: [
    MatTableModule, MatButtonModule, MatIconModule, MatCardModule,
    MatProgressSpinnerModule, MatDialogModule, MatChipsModule,
    MatDividerModule, RouterLink, DatePipe, DecimalPipe,
  ],
  templateUrl: './sales-order-detail.component.html',
  styleUrl: './sales-order-detail.component.scss',
})
export class SalesOrderDetailComponent implements OnInit {
  @Input() id!: string;

  private readonly salesService = inject(SalesService);
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  readonly order = signal<SalesOrderJson | null>(null);
  readonly isLoading = signal(false);
  readonly isClosing = signal(false);

  readonly displayedColumns = ['beerName', 'quantity', 'price', 'total'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.salesService.getSalesOrderById(this.id).subscribe({
      next: (o) => {
        this.order.set(o);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  get isOpen(): boolean {
    return this.order()?.status === 'Open';
  }

  closeOrder(): void {
    if (!confirm('Close this order? This action cannot be undone.')) return;
    this.isClosing.set(true);
    this.salesService.closeSalesOrder(this.id).subscribe({
      next: () => {
        this.notifications.success('Order closed');
        this.load();
        this.isClosing.set(false);
      },
      error: () => this.isClosing.set(false),
    });
  }

  openAddBeersDialog(): void {
    const ref = this.dialog.open(AddBeersDialogComponent, {
      width: '700px',
      disableClose: true,
      data: { orderId: this.id },
    });
    ref.afterClosed().subscribe((added) => {
      if (added) this.load();
    });
  }

  rowTotal(row: { quantity: { value: number }; price: { value: number } }): number {
    return (row.quantity?.value ?? 0) * (row.price?.value ?? 0);
  }

  get orderTotal(): number {
    return this.order()?.rows?.reduce((sum, r) => sum + this.rowTotal(r), 0) ?? 0;
  }
}
