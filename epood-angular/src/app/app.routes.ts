import { Routes } from '@angular/router';
import { ProductsComponent } from './pages/products/products';
import { OrdersComponent } from './pages/orders/orders';

export const routes: Routes = [
  {
    path: '',
    component: ProductsComponent
  },
  {
    path: 'orders',
    component: OrdersComponent
  }
];
