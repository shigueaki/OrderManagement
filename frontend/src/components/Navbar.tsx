import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { HiClipboardList } from 'react-icons/hi';
import ConnectionStatus from './ConnectionStatus';

interface NavbarProps {
  isConnected: boolean;
  connectionState: string;
}

const Navbar: React.FC<NavbarProps> = ({ isConnected, connectionState }) => {
  const location = useLocation();

  return (
    <nav className="bg-white border-b border-gray-200 shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <HiClipboardList className="h-8 w-8 text-primary-600" />
            <span className="text-xl font-bold text-gray-900">
              Order<span className="text-primary-600">Manager</span>
            </span>
          </Link>

          {/* Navigation Links */}
          <div className="flex items-center gap-6">
            <Link
              to="/"
              className={`text-sm font-medium transition-colors duration-200 ${
                location.pathname === '/'
                  ? 'text-primary-600'
                  : 'text-gray-500 hover:text-gray-900'
              }`}
            >
              Orders
            </Link>

            {/* Connection Status */}
            <div className="border-l border-gray-200 pl-4">
              <ConnectionStatus
                isConnected={isConnected}
                connectionState={connectionState}
              />
            </div>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;