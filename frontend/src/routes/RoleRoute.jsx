import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function RoleRoute({ roles, children }) {
  const { hasAnyRole, isAuthenticated, isTokenExpired } = useAuth();

  if (!isAuthenticated || isTokenExpired) {
    return <Navigate to="/login" replace />;
  }

  if (!hasAnyRole(roles)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return children;
}