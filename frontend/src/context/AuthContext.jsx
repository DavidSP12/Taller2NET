import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { authService } from '../services/authService';
import { decodeJwt, getUserFromToken, isJwtExpired } from '../utils/jwt';
import { setUnauthorizedHandler } from '../services/apiClient';

const AuthContext = createContext(null);
const TOKEN_KEY = 'taller2net.token';

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_KEY) || '');
  const [user, setUser] = useState(() => {
    const stored = localStorage.getItem(TOKEN_KEY);
    return stored ? getUserFromToken(stored) : null;
  });

  useEffect(() => {
    setUnauthorizedHandler(() => {
      logout();
    });
  }, []);

  useEffect(() => {
    if (token && isJwtExpired(token)) {
      logout();
    }
  }, [token]);

  function login(payload) {
    return authService.login(payload).then((data) => {
      const nextToken = data?.token || data?.Token;
      if (!nextToken) {
        throw new Error('Respuesta de autenticación inválida');
      }

      localStorage.setItem(TOKEN_KEY, nextToken);
      setToken(nextToken);
      setUser(getUserFromToken(nextToken) || data);
      return data;
    });
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    setToken('');
    setUser(null);
  }

  const value = useMemo(() => ({
    token,
    user,
    isAuthenticated: Boolean(token),
    isTokenExpired: token ? isJwtExpired(token) : true,
    login,
    logout,
    hasAnyRole: (roles = []) => {
      if (!roles.length) return true;
      return roles.includes(user?.role);
    },
    getClaims: () => decodeJwt(token)
  }), [token, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe usarse dentro de AuthProvider');
  }
  return context;
}