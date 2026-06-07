import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../services/AuthContext';
import { Spin } from 'antd';

export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="full-page-center">
        <Spin size="large" />
      </div>
    );
  }

  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}

export function AdminRoute() {
  const { user, isLoading } = useAuth();

  if (isLoading) return <div className="full-page-center"><Spin size="large" /></div>;
  if (!user) return <Navigate to="/login" replace />;
  if (user.role !== 'Admin') return <Navigate to="/dashboard" replace />;

  return <Outlet />;
}

export function GuestRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <div className="full-page-center"><Spin size="large" /></div>;
  return isAuthenticated ? <Navigate to="/dashboard" replace /> : <Outlet />;
}
