import React from 'react';
import Navbar from './NavBar';

interface LayoutProps {
  children: React.ReactNode;
  isConnected: boolean;
  connectionState: string;
}

const Layout: React.FC<LayoutProps> = ({ children, isConnected, connectionState }) => {
  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar isConnected={isConnected} connectionState={connectionState} />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {children}
      </main>
      <footer className="border-t border-gray-200 bg-white mt-auto">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <p className="text-center text-xs text-gray-400">
            Order Management System &copy; {new Date().getFullYear()} — Built with .NET 8, React & TailwindCSS
          </p>
        </div>
      </footer>
    </div>
  );
};

export default Layout;