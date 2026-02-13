import React from 'react';
import { Link } from 'react-router-dom';
import type { Order } from '../types/order';
import StatusBadge from './StatusBadge';
import { format } from 'date-fns';
import { HiEye } from 'react-icons/hi';

interface OrderTableProps {
  orders: Order[];
}

const OrderTable: React.FC<OrderTableProps> = ({ orders }) => {
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

  if (orders.length === 0) {
    return (
      <div className="card text-center py-12">
        <div className="text-gray-400 text-5xl mb-4">📦</div>
        <h3 className="text-lg font-medium text-gray-900 mb-1">No orders yet</h3>
        <p className="text-sm text-gray-500">
          Create your first order to get started.
        </p>
      </div>
    );
  }

  return (
    <div className="card overflow-hidden p-0">
      {/* Desktop Table */}
      <div className="hidden md:block overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th className="text-left text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                Customer
              </th>
              <th className="text-left text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                Product
              </th>
              <th className="text-left text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                Value
              </th>
              <th className="text-left text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                Status
              </th>
              <th className="text-left text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                Created At
              </th>
              <th className="text-right text-xs font-semibold text-gray-500 uppercase tracking-wider px-6 py-3">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {orders.map((order) => (
              <tr
                key={order.id}
                className="hover:bg-gray-50 transition-colors duration-150 animate-fade-in"
              >
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-medium text-gray-900">
                    {order.customerName}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-600">{order.productName}</div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm font-semibold text-gray-900">
                    {formatCurrency(order.value)}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusBadge status={order.status} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-500">
                    {formatDate(order.createdAt)}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right">
                  <Link
                    to={`/orders/${order.id}`}
                    className="inline-flex items-center gap-1 text-primary-600 hover:text-primary-800 
                             text-sm font-medium transition-colors duration-200"
                  >
                    <HiEye className="h-4 w-4" />
                    View
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile Cards */}
      <div className="md:hidden divide-y divide-gray-200">
        {orders.map((order) => (
          <div key={order.id} className="p-4 animate-fade-in">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-semibold text-gray-900">
                {order.customerName}
              </span>
              <StatusBadge status={order.status} />
            </div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-gray-600">{order.productName}</span>
              <span className="text-sm font-bold text-gray-900">
                {formatCurrency(order.value)}
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-400">
                {formatDate(order.createdAt)}
              </span>
              <Link
                to={`/orders/${order.id}`}
                className="inline-flex items-center gap-1 text-primary-600 hover:text-primary-800 
                         text-sm font-medium transition-colors duration-200"
              >
                <HiEye className="h-4 w-4" />
                Details
              </Link>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default OrderTable;