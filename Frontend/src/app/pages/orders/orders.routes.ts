import { Routes } from '@angular/router';

export const ordersRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/orders-product-list-page').then((m) => m.OrdersProductListPage),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/orders-product-details-page').then(
        (m) => m.OrdersProductDetailsPage
      ),
  },
];
