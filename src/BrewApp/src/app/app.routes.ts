import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'chat', pathMatch: 'full' },
  {
    path: 'chat',
    loadComponent: () =>
      import('./features/chat/components/chat/chat.component').then(
        (m) => m.ChatComponent
      )
  }
];
