export interface Order {
  id: string;
  customerName: string;
  productName: string;
  value: number;
  status: OrderStatus;
  createdAt: string;
  updatedAt: string | null;
  statusHistory: StatusHistoryEntry[] | null;
}

export type OrderStatus = 'Pending' | 'Processing' | 'Completed';

export interface StatusHistoryEntry {
  id: string;
  status: string;
  changedAt: string;
}

export interface CreateOrderRequest {
  customerName: string;
  productName: string;
  value: number;
}

export interface OrderStatusChangeEvent {
  orderId: string;
  status: OrderStatus;
  updatedAt: string;
}