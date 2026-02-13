import React, { useState } from 'react';
import OrderTable from '../components/OrderTable';
import OrderForm from '../components/OrderForm';
import LoadingSpinner from '../components/LoadingSpinner';
import { useOrders } from '../hooks/useOrders';
import { HiPlus, HiRefresh } from 'react-icons/hi';

const OrdersPage: React.FC = () => {
  const [showForm, setShowForm] = useState(false);
  const { orders, loading, error, createOrder, refreshOrders } = useOrders();

  // Summary stats
  const stats = {
    total: orders.length,
    pending: orders.filter((o) => o.status === 'Pending').length,
    processing: orders.filter((o) => o.status === 'Processing').length,
    completed: orders.filter((o) => o.status === 'Completed').length,
    totalValue: orders.reduce((sum, o) => sum + o.value, 0),
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value);
  };

  return (
    <div className="animate-fade-in">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Orders</h1>
          <p className="text-sm text-gray-500 mt-1">
            Manage and track all your orders
          </p>
        </div>
        <div className="flex items-center gap-3">
          <button
            onClick={refreshOrders}
            className="btn-secondary flex items-center gap-2"
            disabled={loading}
          >
            <HiRefresh className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            Refresh
          </button>
          <button
            onClick={() => setShowForm(true)}
            className="btn-primary flex items-center gap-2"
          >
            <HiPlus className="h-4 w-4" />
            New Order
          </button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-4 mb-6">
        <div className="card">
          <p className="text-xs text-gray-500 uppercase tracking-wider">Total</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{stats.total}</p>
        </div>
        <div className="card">
          <p className="text-xs text-yellow-600 uppercase tracking-wider">Pending</p>
          <p className="text-2xl font-bold text-yellow-600 mt-1">{stats.pending}</p>
        </div>
        <div className="card">
          <p className="text-xs text-blue-600 uppercase tracking-wider">Processing</p>
          <p className="text-2xl font-bold text-blue-600 mt-1">{stats.processing}</p>
        </div>
        <div className="card">
          <p className="text-xs text-green-600 uppercase tracking-wider">Completed</p>
          <p className="text-2xl font-bold text-green-600 mt-1">{stats.completed}</p>
        </div>
        <div className="card col-span-2 lg:col-span-1">
          <p className="text-xs text-gray-500 uppercase tracking-wider">Total Value</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">
            {formatCurrency(stats.totalValue)}
          </p>
        </div>
      </div>

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <p className="text-sm text-red-800">{error}</p>
          <button
            onClick={refreshOrders}
            className="text-sm text-red-600 hover:text-red-800 font-medium mt-2 underline"
          >
            Try again
          </button>
        </div>
      )}

      {/* Loading State */}
      {loading && orders.length === 0 ? (
        <LoadingSpinner size="lg" message="Loading orders..." />
      ) : (
        <OrderTable orders={orders} />
      )}

      {/* Create Order Modal */}
      {showForm && (
        <OrderForm
          onSubmit={createOrder}
          onClose={() => setShowForm(false)}
        />
      )}
    </div>
  );
};

export default OrdersPage;