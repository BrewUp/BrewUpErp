import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { DecimalPipe } from '@angular/common';
import { BeerService } from '../../services/beer.service';
import { BeerJson } from '../../models/beer.model';
import { AddBeerDialogComponent } from './add-beer-dialog.component';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-beer-list',
  standalone: true,
  imports: [
    MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule,
    MatCardModule, MatProgressSpinnerModule, MatDialogModule, MatChipsModule,
    DecimalPipe,
  ],
  templateUrl: './beer-list.component.html',
  styleUrl: './beer-list.component.scss',
})
export class BeerListComponent implements OnInit {
  private readonly beerService = inject(BeerService);
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly beers = signal<BeerJson[]>([]);
  readonly totalRecords = signal(0);
  readonly isLoading = signal(false);

  pageIndex = 0;
  pageSize = 10;

  readonly displayedColumns = ['beerName', 'beerStyle', 'abv', 'packaging', 'price', 'status'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.beerService.getBeers(this.pageIndex + 1, this.pageSize).subscribe({
      next: (result) => {
        this.beers.set(result.results ?? []);
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
    const ref = this.dialog.open(AddBeerDialogComponent, { width: '500px', disableClose: true });
    ref.afterClosed().subscribe((created) => {
      if (created) this.load();
    });
  }
}
