import { Routes } from '@angular/router';
import { authGuard } from './_guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  {
    path: 'contacts',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/contacts/contacts-page').then((m) => m.ContactsPage),
  },
  {
    path: 'products',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/products/products-page').then((m) => m.ProductsPage),
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/orders/orders-page').then((m) => m.OrdersPage),
    loadChildren: () =>
      import('./pages/orders/orders.routes').then((m) => m.ordersRoutes),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login-page').then((m) => m.LoginPage),
  },
  { path: '**', redirectTo: 'orders' },
];
