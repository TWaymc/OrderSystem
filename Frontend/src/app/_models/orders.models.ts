export type OrderStatus = string | number;

export interface OrderItem {
  id: string;
  productId: string;
  productCode: string;
  productName: string;
  productUnitPrice: number;
  quantity: number;
  subTotal: number;
}

export interface Order {
  id: string;
  code: string;
  statusCode: OrderStatus;
  customerId: string;
  customerName: string;
  customerSurname: string;
  customerMobileNumber: string | null;
  customerEmail: string | null;
  orderItems: OrderItem[];
  totalAmount: number;
  totalItems: number;
  createdBy: string;
  lastModifiedBy: string;
  createdAt: string;
  modifiedAt: string | null;
}

export interface CreateOrderItem {
  productId: string;
  quantity: number;
}

export interface CreateOrder {
  customerId: string;
  orderItems: CreateOrderItem[];
}

export interface UpdateOrder {
  customerId: string;
}

export interface AddOrderItem {
  productId: string;
  quantity: number;
}

export interface UpdateOrderStatus {
  statusCode: OrderStatus;
}
