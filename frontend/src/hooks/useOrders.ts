import { useState, useEffect, useCallback } from 'react';
import type { Order, CreateOrderRequest, OrderStatusChangeEvent } from '../types/order';
import { orderApi } from '../api/orderApi';
import { useSignalR } from './useSignalR';
import toast from 'react-hot-toast';

interface UseOrdersReturn {
  orders: Order[];
  loading: boolean;
  error: string | null;
  isConnected: boolean;
  connectionState: string;
  createOrder: (request: CreateOrderRequest) => Promise<Order | null>;
  refreshOrders: () => Promise<void>;
}

export function useOrders(): UseOrdersReturn {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchOrders = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await orderApi.getAll();
      setOrders(data);
    } catch (err: any) {
      const message = err.response?.data?.message || 'Failed to fetch orders';
      setError(message);
      toast.error(message);
    } finally {
      setLoading(false);
    }
  }, []);

  const handleOrderCreated = useCallback((order: Order) => {
    setOrders((prev) => {
      // Avoid duplicates
      const exists = prev.some((o) => o.id === order.id);
      if (exists) return prev;
      return [order, ...prev];
    });
    toast.success(`New order created for ${order.customerName}!`, {
      icon: '🎉',
      duration: 4000,
    });
  }, []);

  const handleOrderStatusChanged = useCallback((event: OrderStatusChangeEvent) => {
    setOrders((prev) =>
      prev.map((order) =>
        order.id === event.orderId
          ? { ...order, status: event.status, updatedAt: event.updatedAt }
          : order
      )
    );

    const statusEmoji = event.status === 'Processing' ? '⚙️' : '✅';
    toast.success(`Order status changed to ${event.status}`, {
      icon: statusEmoji,
      duration: 3000,
    });
  }, []);

  const { isConnected, connectionState } = useSignalR({
    onOrderCreated: handleOrderCreated,
    onOrderStatusChanged: handleOrderStatusChanged,
  });

  const createOrder = useCallback(async (request: CreateOrderRequest): Promise<Order | null> => {
    try {
      const order = await orderApi.create(request);
      // Add to local state immediately (SignalR might also add it, but we handle duplicates)
      setOrders((prev) => {
        const exists = prev.some((o) => o.id === order.id);
        if (exists) return prev;
        return [order, ...prev];
      });
      toast.success('Order created successfully!', { icon: '✅' });
      return order;
    } catch (err: any) {
      const message = err.response?.data?.message
        || err.response?.data?.title
        || 'Failed to create order';
      toast.error(message);
      return null;
    }
  }, []);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  return {
    orders,
    loading,
    error,
    isConnected,
    connectionState,
    createOrder,
    refreshOrders: fetchOrders,
  };
}