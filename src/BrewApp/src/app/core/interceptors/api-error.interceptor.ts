import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const apiErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 0) {
        notifications.error('Unable to reach the server. Check your connection.');
      } else if (error.status === 404) {
        notifications.warn('Resource not found.');
      } else if (error.status >= 500) {
        notifications.error('An unexpected error occurred. Please try again later.');
      }
      // 422 validation errors are passed through for component-level handling
      return throwError(() => error);
    })
  );
};
