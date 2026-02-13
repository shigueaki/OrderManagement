import React, { useState } from 'react';
import type { CreateOrderRequest } from '../types/order';
import { HiPlus, HiX } from 'react-icons/hi';

interface OrderFormProps {
  onSubmit: (request: CreateOrderRequest) => Promise<any>;
  onClose: () => void;
}

const OrderForm: React.FC<OrderFormProps> = ({ onSubmit, onClose }) => {
  const [customerName, setCustomerName] = useState('');
  const [productName, setProductName] = useState('');
  const [value, setValue] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!customerName.trim()) {
      newErrors.customerName = 'Customer name is required';
    } else if (customerName.length > 200) {
      newErrors.customerName = 'Customer name must not exceed 200 characters';
    }

    if (!productName.trim()) {
      newErrors.productName = 'Product name is required';
    } else if (productName.length > 200) {
      newErrors.productName = 'Product name must not exceed 200 characters';
    }

    const numValue = parseFloat(value);
    if (!value || isNaN(numValue)) {
      newErrors.value = 'Value is required and must be a number';
    } else if (numValue <= 0) {
      newErrors.value = 'Value must be greater than zero';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    setSubmitting(true);
    try {
      const result = await onSubmit({
        customerName: customerName.trim(),
        productName: productName.trim(),
        value: parseFloat(value),
      });

      if (result) {
        setCustomerName('');
        setProductName('');
        setValue('');
        setErrors({});
        onClose();
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md animate-slide-up">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">Create New Order</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <HiX className="h-5 w-5" />
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {/* Customer Name */}
          <div>
            <label htmlFor="customerName" className="block text-sm font-medium text-gray-700 mb-1">
              Customer Name
            </label>
            <input
              id="customerName"
              type="text"
              value={customerName}
              onChange={(e) => setCustomerName(e.target.value)}
              className={`input-field ${errors.customerName ? 'border-red-500 focus:ring-red-500' : ''}`}
              placeholder="e.g. John Doe"
              disabled={submitting}
            />
            {errors.customerName && (
              <p className="mt-1 text-xs text-red-600">{errors.customerName}</p>
            )}
          </div>

          {/* Product Name */}
          <div>
            <label htmlFor="productName" className="block text-sm font-medium text-gray-700 mb-1">
              Product Name
            </label>
            <input
              id="productName"
              type="text"
              value={productName}
              onChange={(e) => setProductName(e.target.value)}
              className={`input-field ${errors.productName ? 'border-red-500 focus:ring-red-500' : ''}`}
              placeholder="e.g. Laptop Pro"
              disabled={submitting}
            />
            {errors.productName && (
              <p className="mt-1 text-xs text-red-600">{errors.productName}</p>
            )}
          </div>

          {/* Value */}
          <div>
            <label htmlFor="value" className="block text-sm font-medium text-gray-700 mb-1">
              Value (R$)
            </label>
            <input
              id="value"
              type="number"
              step="0.01"
              min="0.01"
              value={value}
              onChange={(e) => setValue(e.target.value)}
              className={`input-field ${errors.value ? 'border-red-500 focus:ring-red-500' : ''}`}
              placeholder="e.g. 1500.00"
              disabled={submitting}
            />
            {errors.value && (
              <p className="mt-1 text-xs text-red-600">{errors.value}</p>
            )}
          </div>

          {/* Buttons */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="btn-secondary flex-1"
              disabled={submitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn-primary flex-1 flex items-center justify-center gap-2"
              disabled={submitting}
            >
              {submitting ? (
                <>
                  <div className="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                  Creating...
                </>
              ) : (
                <>
                  <HiPlus className="h-4 w-4" />
                  Create Order
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default OrderForm;