export function decodeJwt(token) {
  if (!token) return null;
  const payload = token.split('.')[1];
  if (!payload) return null;
  const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
  const json = decodeURIComponent(
    atob(normalized)
      .split('')
      .map((char) => `%${(`00${char.charCodeAt(0).toString(16)}`).slice(-2)}`)
      .join('')
  );

  return JSON.parse(json);
}

export function isJwtExpired(token) {
  const payload = decodeJwt(token);
  if (!payload?.exp) return true;
  return payload.exp * 1000 <= Date.now();
}

export function getUserFromToken(token) {
  const payload = decodeJwt(token);
  if (!payload) return null;

  return {
    username: payload.unique_name || payload.sub || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || 'Usuario',
    email: payload.email || '',
    role: payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || 'Student',
    expiresAt: payload.exp ? new Date(payload.exp * 1000) : null
  };
}