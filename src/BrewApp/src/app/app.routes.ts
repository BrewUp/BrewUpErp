import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'sales', pathMatch: 'full' },
  {
    path: 'chat',
    loadComponent: () =>
      import('./features/chat/components/chat/chat.component').then(m => m.ChatComponent),
  },
  {
    path: 'master-data/beers',
    loadComponent: () =>
      import('./features/master-data/components/beers/beer-list.component').then(m => m.BeerListComponent),
  },
  {
    path: 'master-data/customers',
    loadComponent: () =>
      import('./features/master-data/components/customers/customer-list.component').then(m => m.CustomerListComponent),
  },
  {
    path: 'master-data/warehouses',
    loadComponent: () =>
      import('./features/master-data/components/warehouses/warehouse-list.component').then(m => m.WarehouseListComponent),
  },
  {
    path: 'sales',
    loadComponent: () =>
      import('./features/sales/components/order-list/sales-order-list.component').then(m => m.SalesOrderListComponent),
  },
  {
    path: 'sales/:id',
    loadComponent: () =>
      import('./features/sales/components/order-detail/sales-order-detail.component').then(m => m.SalesOrderDetailComponent),
  },
  {
    path: 'warehouse/shipments',
    loadComponent: () =>
      import('./features/warehouse/components/shipment-list/shipment-list.component').then(m => m.ShipmentListComponent),
  },
  {
    path: 'purchases',
    loadComponent: () =>
      import('./features/purchases/components/create-purchase/create-purchase.component').then(m => m.CreatePurchaseComponent),
  },
  {
    path: 'dashboards',
    loadComponent: () =>
      import('./features/dashboards/components/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
];
