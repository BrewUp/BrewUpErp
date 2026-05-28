import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private snackBar: MatSnackBar) {}

  error(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['snack-error']
    });
  }

  success(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['snack-success']
    });
  }

  warn(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 4000,
      panelClass: ['snack-warn']
    });
  }
}
