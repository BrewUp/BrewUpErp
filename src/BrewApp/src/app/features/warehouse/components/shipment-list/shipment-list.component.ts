import { Component, inject, OnInit, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { DatePipe } from '@angular/common';
import { ShipmentService } from '../../services/shipment.service';
import { ShipmentJson } from '../../models/shipment.model';

@Component({
  selector: 'app-shipment-list',
  standalone: true,
  imports: [
    MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatChipsModule, DatePipe,
  ],
  templateUrl: './shipment-list.component.html',
  styleUrl: './shipment-list.component.scss',
})
export class ShipmentListComponent implements OnInit {
  private readonly shipmentService = inject(ShipmentService);

  readonly shipments = signal<ShipmentJson[]>([]);
  readonly totalRecords = signal(0);
  readonly isLoading = signal(false);

  pageIndex = 0;
  pageSize = 10;

  readonly displayedColumns = ['id', 'salesOrderId', 'customerId', 'deliveryDate', 'rows', 'shipmentState'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.shipmentService.getShipments(this.pageIndex + 1, this.pageSize).subscribe({
      next: (result) => {
        this.shipments.set(result.results ?? []);
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
}
