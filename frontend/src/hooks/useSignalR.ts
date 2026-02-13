import { useState, useEffect, useCallback, useRef } from 'react';
import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
  HubConnectionState,
} from '@microsoft/signalr';
import type { Order, OrderStatusChangeEvent } from '../types/order';

const HUB_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

interface UseSignalRReturn {
  isConnected: boolean;
  connectionState: string;
}

interface UseSignalRProps {
  onOrderCreated?: (order: Order) => void;
  onOrderStatusChanged?: (event: OrderStatusChangeEvent) => void;
}

export function useSignalR({
  onOrderCreated,
  onOrderStatusChanged,
}: UseSignalRProps): UseSignalRReturn {
  const [isConnected, setIsConnected] = useState(false);
  const [connectionState, setConnectionState] = useState('Disconnected');
  const connectionRef = useRef<HubConnection | null>(null);
  const onOrderCreatedRef = useRef(onOrderCreated);
  const onOrderStatusChangedRef = useRef(onOrderStatusChanged);

  useEffect(() => {
    onOrderCreatedRef.current = onOrderCreated;
  }, [onOrderCreated]);

  useEffect(() => {
    onOrderStatusChangedRef.current = onOrderStatusChanged;
  }, [onOrderStatusChanged]);

  const startConnection = useCallback(async () => {
    try {
      const connection = new HubConnectionBuilder()
        .withUrl(`${HUB_URL}/hubs/orders`)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(LogLevel.Information)
        .build();

      connection.onreconnecting(() => {
        setConnectionState('Reconnecting');
        setIsConnected(false);
        console.log('[SignalR] Reconnecting...');
      });

      connection.onreconnected(() => {
        setConnectionState('Connected');
        setIsConnected(true);
        console.log('[SignalR] Reconnected!');
      });

      connection.onclose(() => {
        setConnectionState('Disconnected');
        setIsConnected(false);
        console.log('[SignalR] Connection closed.');
      });

      connection.on('OrderCreated', (order: Order) => {
        console.log('[SignalR] OrderCreated:', order);
        onOrderCreatedRef.current?.(order);
      });

      connection.on('OrderStatusChanged', (event: OrderStatusChangeEvent) => {
        console.log('[SignalR] OrderStatusChanged:', event);
        onOrderStatusChangedRef.current?.(event);
      });

      await connection.start();
      connectionRef.current = connection;
      setIsConnected(true);
      setConnectionState('Connected');
      console.log('[SignalR] Connected!');
    } catch (error) {
      console.error('[SignalR] Connection failed:', error);
      setConnectionState('Failed');
      setIsConnected(false);
      setTimeout(startConnection, 5000);
    }
  }, []);

  useEffect(() => {
    startConnection();

    return () => {
      if (connectionRef.current?.state === HubConnectionState.Connected) {
        connectionRef.current.stop();
      }
    };
  }, [startConnection]);

  return { isConnected, connectionState };
}