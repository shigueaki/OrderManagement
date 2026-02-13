import React from 'react';
import type { OrderStatus } from '../types/order';

interface StatusBadgeProps {
  status: OrderStatus;
  animate?: boolean;
}

const StatusBadge: React.FC<StatusBadgeProps> = ({ status, animate = true }) => {
  const config = {
    Pending: {
      bg: 'bg-yellow-100',
      text: 'text-yellow-800',
      ring: 'ring-yellow-300',
      dot: 'bg-yellow-500',
      label: 'Pending',
    },
    Processing: {
      bg: 'bg-blue-100',
      text: 'text-blue-800',
      ring: 'ring-blue-300',
      dot: 'bg-blue-500',
      label: 'Processing',
    },
    Completed: {
      bg: 'bg-green-100',
      text: 'text-green-800',
      ring: 'ring-green-300',
      dot: 'bg-green-500',
      label: 'Completed',
    },
  };

  const statusConfig = config[status] || config.Pending;
  const isProcessing = status === 'Processing' && animate;

  return (
    <span
      className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold 
        ${statusConfig.bg} ${statusConfig.text} ring-1 ${statusConfig.ring}
        transition-all duration-300`}
    >
      <span
        className={`h-2 w-2 rounded-full ${statusConfig.dot} 
          ${isProcessing ? 'animate-pulse' : ''}`}
      />
      {statusConfig.label}
    </span>
  );
};

export default StatusBadge;