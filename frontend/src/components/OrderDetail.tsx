import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import type { Order } from '../types/order';
import { orderApi } from '../api/orderApi';
import StatusBadge from './StatusBadge';
import LoadingSpinner from './LoadingSpinner';
import { format } from 'date-fns';
import {
  HiArrowLeft,
  HiUser,
  HiCube,
  HiCurrencyDollar,
  HiClock,
  HiRefresh,
} from 'react-icons/hi';
import toast from 'react-hot-toast';

const OrderDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchOrder = async () => {
    if (!id) return;

    try {
      setLoading(true);
      setError(null);
      const data = await orderApi.getById(id);
      setOrder(data);
    } catch (err: any) {
      const message =
        err.response?.status === 404
          ? 'Order not found'
          : err.response?.data?.message || 'Failed to fetch order details';
      setError(message);
      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrder();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  // Poll for updates when order is not completed
  useEffect(() => {
    if (order && order.status !== 'Completed') {
      const interval = setInterval(fetchOrder, 3000);
      return () => clearInterval(interval);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [order?.status]);

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value);
  };

  const formatDate = (dateString: string): string => {
    try {
      return format(new Date(dateString), 'MMM dd, yyyy HH:mm:ss');
    } catch {
      return dateString;
    }
  };

  if (loading && !order) {
    return <LoadingSpinner size="lg" message="Loading order details..." />;
  }

  if (error || !order) {
    return (
      <div className="text-center py-12">
        <div className="text-red-400 text-5xl mb-4">❌</div>
        <h3 className="text-lg font-medium text-gray-900 mb-2">
          {error || 'Order not found'}
        </h3>
        <Link to="/" className="btn-primary inline-flex items-center gap-2 mt-4">
          <HiArrowLeft className="h-4 w-4" />
          Back to Orders
        </Link>
      </div>
    );
  }

  return (
    <div className="animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-4">
          <Link
            to="/"
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <HiArrowLeft className="h-6 w-6" />
          </Link>
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Order Details</h1>
            <p className="text-sm text-gray-500 mt-1 font-mono">
              ID: {order.id}
            </p>
          </div>
        </div>
        <button
          onClick={fetchOrder}
          className="btn-secondary flex items-center gap-2"
        >
          <HiRefresh className="h-4 w-4" />
          Refresh
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Order Info Card */}
        <div className="lg:col-span-2 card">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Order Information
          </h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div className="flex items-start gap-3">
              <div className="p-2 bg-primary-50 rounded-lg">
                <HiUser className="h-5 w-5 text-primary-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wider">Customer</p>
                <p className="text-sm font-semibold text-gray-900 mt-1">
                  {order.customerName}
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="p-2 bg-purple-50 rounded-lg">
                <HiCube className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wider">Product</p>
                <p className="text-sm font-semibold text-gray-900 mt-1">
                  {order.productName}
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="p-2 bg-green-50 rounded-lg">
                <HiCurrencyDollar className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wider">Value</p>
                <p className="text-sm font-semibold text-gray-900 mt-1">
                  {formatCurrency(order.value)}
                </p>
              </div>
            </div>

            <div className="flex items-start gap-3">
              <div className="p-2 bg-yellow-50 rounded-lg">
                <HiClock className="h-5 w-5 text-yellow-600" />
              </div>
              <div>
                <p className="text-xs text-gray-500 uppercase tracking-wider">Created At</p>
                <p className="text-sm font-semibold text-gray-900 mt-1">
                  {formatDate(order.createdAt)}
                </p>
              </div>
            </div>
          </div>

          {/* Current Status */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-500">Current Status</span>
              <StatusBadge status={order.status} />
            </div>
            {order.updatedAt && (
              <p className="text-xs text-gray-400 mt-2 text-right">
                Last updated: {formatDate(order.updatedAt)}
              </p>
            )}
          </div>
        </div>

        {/* Status History Card */}
        <div className="card">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Status History
          </h2>

          {order.statusHistory && order.statusHistory.length > 0 ? (
            <div className="relative">
              {/* Timeline line */}
              <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-gray-200" />

              <div className="space-y-6">
                {order.statusHistory.map((entry, index) => {
                  const isLast = index === order.statusHistory!.length - 1;
                  const statusColors: Record<string, string> = {
                    Pending: 'bg-yellow-500',
                    Processing: 'bg-blue-500',
                    Completed: 'bg-green-500',
                  };

                  return (
                    <div key={entry.id} className="relative flex items-start gap-4 pl-2">
                      {/* Timeline dot */}
                      <div
                        className={`relative z-10 h-4 w-4 rounded-full border-2 border-white shadow 
                          ${statusColors[entry.status] || 'bg-gray-400'}
                          ${isLast ? 'ring-2 ring-offset-2 ring-primary-300' : ''}`}
                      />

                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-semibold text-gray-900">
                          {entry.status}
                        </p>
                        <p className="text-xs text-gray-500 mt-0.5">
                          {formatDate(entry.changedAt)}
                        </p>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          ) : (
            <p className="text-sm text-gray-500 text-center py-4">
              No status history available
            </p>
          )}

          {/* Status Progress Bar */}
          <div className="mt-6 pt-6 border-t border-gray-200">
            <p className="text-xs text-gray-500 uppercase tracking-wider mb-3">
              Progress
            </p>
            <div className="flex items-center gap-2">
              {['Pending', 'Processing', 'Completed'].map((step, index) => {
                const steps = ['Pending', 'Processing', 'Completed'];
                const currentIndex = steps.indexOf(order.status);
                const isActive = index <= currentIndex;
                const isCurrent = index === currentIndex;

                return (
                  <React.Fragment key={step}>
                    <div
                      className={`flex-shrink-0 h-8 w-8 rounded-full flex items-center justify-center text-xs font-bold
                        ${isActive
                          ? 'bg-primary-600 text-white'
                          : 'bg-gray-200 text-gray-500'
                        }
                        ${isCurrent && order.status === 'Processing'
                          ? 'animate-pulse ring-2 ring-primary-300'
                          : ''
                        }`}
                    >
                      {index + 1}
                    </div>
                    {index < 2 && (
                      <div
                        className={`flex-1 h-1 rounded ${
                          index < currentIndex ? 'bg-primary-600' : 'bg-gray-200'
                        }`}
                      />
                    )}
                  </React.Fragment>
                );
              })}
            </div>
            <div className="flex justify-between mt-1">
              <span className="text-xs text-gray-400">Pending</span>
              <span className="text-xs text-gray-400">Processing</span>
              <span className="text-xs text-gray-400">Completed</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrderDetail;