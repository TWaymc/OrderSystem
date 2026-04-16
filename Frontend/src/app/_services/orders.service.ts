import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  AddOrderItem,
  CreateOrder,
  Order,
  UpdateOrder,
  UpdateOrderStatus,
} from '../_models/orders.models';

@Injectable({
  providedIn: 'root',
})
export class OrdersService {
  private readonly ordersUrl = `${environment.apiBaseUrl}/orders`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Order[]> {
    return this.http.get<Order[]>(this.ordersUrl);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.ordersUrl}/${id}`);
  }

  create(dto: CreateOrder): Observable<Order> {
    return this.http.post<Order>(this.ordersUrl, dto);
  }

  updateStatus(id: string, dto: UpdateOrderStatus): Observable<Order> {
    return this.http.put<Order>(`${this.ordersUrl}/${id}/status`, dto);
  }

  update(id: string, dto: UpdateOrder): Observable<Order> {
    return this.http.put<Order>(`${this.ordersUrl}/${id}`, dto);
  }

  addOrderItem(id: string, dto: AddOrderItem): Observable<Order> {
    return this.http.post<Order>(`${this.ordersUrl}/${id}/items`, dto);
  }

  removeOrderItem(id: string, orderItemId: string): Observable<void> {
    return this.http.delete<void>(`${this.ordersUrl}/${id}/items/${orderItemId}`);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.ordersUrl}/${id}`);
  }
}
