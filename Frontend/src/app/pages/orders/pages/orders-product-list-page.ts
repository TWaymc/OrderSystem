import { Component, OnInit, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { Order } from '../../../_models/orders.models';
import { OrdersService } from '../../../_services/orders.service';

type SortColumn =
  | 'code'
  | 'statusCode'
  | 'customer'
  | 'totalItems'
  | 'totalAmount'
  | 'createdAt'
  | 'modifiedAt';
type SortDirection = 'asc' | 'desc';

@Component({
  selector: 'app-orders-product-list-page',
  imports: [DatePipe, RouterLink],
  templateUrl: './orders-product-list-page.html',
  styleUrl: './orders-product-list-page.scss',
})
export class OrdersProductListPage implements OnInit {
  readonly orders = signal<Order[]>([]);
  readonly isLoading = signal(false);
  readonly deletingOrderId = signal<string | null>(null);
  readonly openStatusMenuOrderId = signal<string | null>(null);
  readonly updatingStatusOrderId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly sortColumn = signal<SortColumn>('createdAt');
  readonly sortDirection = signal<SortDirection>('desc');
  readonly sortedOrders = computed(() => {
    const items = [...this.orders()];
    const column = this.sortColumn();
    const direction = this.sortDirection();
    const directionFactor = direction === 'asc' ? 1 : -1;

    return items.sort((a, b) => {
      const aValue = this.getSortValue(a, column);
      const bValue = this.getSortValue(b, column);

      if (aValue < bValue) {
        return -1 * directionFactor;
      }

      if (aValue > bValue) {
        return 1 * directionFactor;
      }

      return 0;
    });
  });

  constructor(private readonly ordersService: OrdersService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.ordersService
      .getAll()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (orders) => {
          this.orders.set(orders);
        },
        error: () => {
          this.errorMessage.set('Unable to load orders.');
        },
      });
  }

  isDeleting(orderId: string): boolean {
    return this.deletingOrderId() === orderId;
  }

  deleteOrder(orderId: string): void {
    if (this.isDeleting(orderId)) {
      return;
    }

    this.deletingOrderId.set(orderId);
    this.errorMessage.set('');

    this.ordersService
      .delete(orderId)
      .pipe(finalize(() => this.deletingOrderId.set(null)))
      .subscribe({
        next: () => {
          this.orders.set(this.orders().filter((order) => order.id !== orderId));
        },
        error: () => {
          this.errorMessage.set('Unable to delete order.');
        },
      });
  }

  isStatusMenuOpen(orderId: string): boolean {
    return this.openStatusMenuOrderId() === orderId;
  }

  toggleStatusMenu(orderId: string): void {
    if (this.isStatusMenuOpen(orderId)) {
      this.openStatusMenuOrderId.set(null);
      return;
    }

    this.openStatusMenuOrderId.set(orderId);
  }

  isUpdatingStatus(orderId: string): boolean {
    return this.updatingStatusOrderId() === orderId;
  }

  changeStatus(order: Order, newStatusCode: number): void {
    const currentStatus = this.getStatusRank(order.statusCode);

    if (currentStatus === newStatusCode) {
      this.openStatusMenuOrderId.set(null);
      return;
    }

    this.updatingStatusOrderId.set(order.id);
    this.errorMessage.set('');

    this.ordersService
      .updateStatus(order.id, { statusCode: newStatusCode })
      .pipe(finalize(() => this.updatingStatusOrderId.set(null)))
      .subscribe({
        next: (updatedOrder) => {
          this.orders.set(
            this.orders().map((existing) =>
              existing.id === updatedOrder.id ? updatedOrder : existing
            )
          );
          this.openStatusMenuOrderId.set(null);
        },
        error: () => {
          this.errorMessage.set('Unable to change order status.');
        },
      });
  }

  toggleSort(column: SortColumn): void {
    if (this.sortColumn() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
      return;
    }

    this.sortColumn.set(column);
    this.sortDirection.set('asc');
  }

  getSortIndicator(column: SortColumn): string {
    if (this.sortColumn() !== column) {
      return '';
    }

    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  getStatusLabel(statusCode: string | number): string {
    if (statusCode === 0 || statusCode === 'Pending') {
      return 'Pending';
    }

    if (statusCode === 1 || statusCode === 'Cancelled') {
      return 'Cancelled';
    }

    if (statusCode === 2 || statusCode === 'Processed') {
      return 'Processed';
    }

    return String(statusCode);
  }

  getStatusClass(statusCode: string | number): string {
    if (statusCode === 0 || statusCode === 'Pending') {
      return 'status-pending';
    }

    if (statusCode === 1 || statusCode === 'Cancelled') {
      return 'status-cancelled';
    }

    if (statusCode === 2 || statusCode === 'Processed') {
      return 'status-processed';
    }

    return 'status-default';
  }

  private getSortValue(order: Order, column: SortColumn): number | string {
    if (column === 'code') {
      return order.code;
    }

    if (column === 'statusCode') {
      return this.getStatusRank(order.statusCode);
    }

    if (column === 'customer') {
      return `${order.customerName} ${order.customerSurname}`;
    }

    if (column === 'totalItems') {
      return order.totalItems;
    }

    if (column === 'totalAmount') {
      return order.totalAmount;
    }

    if (column === 'createdAt') {
      return Date.parse(order.createdAt);
    }

    if (column === 'modifiedAt') {
      return order.modifiedAt ? Date.parse(order.modifiedAt) : 0;
    }

    return '';
  }

  private getStatusRank(statusCode: string | number): number {
    if (statusCode === 0 || statusCode === 'Pending') {
      return 0;
    }

    if (statusCode === 1 || statusCode === 'Cancelled') {
      return 1;
    }

    if (statusCode === 2 || statusCode === 'Processed') {
      return 2;
    }

    return 99;
  }
}
