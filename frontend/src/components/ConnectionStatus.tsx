import React from 'react';
import { HiStatusOnline, HiStatusOffline } from 'react-icons/hi';

interface ConnectionStatusProps {
  isConnected: boolean;
  connectionState: string;
}

const ConnectionStatus: React.FC<ConnectionStatusProps> = ({
  isConnected,
  connectionState,
}) => {
  return (
    <div className="flex items-center gap-2">
      {isConnected ? (
        <>
          <HiStatusOnline className="h-5 w-5 text-green-500" />
          <span className="text-xs text-green-600 font-medium">Live</span>
        </>
      ) : (
        <>
          <HiStatusOffline
            className={`h-5 w-5 ${
              connectionState === 'Reconnecting'
                ? 'text-yellow-500 animate-pulse'
                : 'text-red-500'
            }`}
          />
          <span
            className={`text-xs font-medium ${
              connectionState === 'Reconnecting'
                ? 'text-yellow-600'
                : 'text-red-600'
            }`}
          >
            {connectionState}
          </span>
        </>
      )}
    </div>
  );
};

export default ConnectionStatus;