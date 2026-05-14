import { getBackendProvider } from '../providers';

let unauthorizedHandler = null;

export function setUnauthorizedHandler(handler) {
  unauthorizedHandler = handler;
}

function buildHeaders(body, token) {
  const headers = {};

  if (!(body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

async function parseResponse(response) {
  const text = await response.text();
  if (!text) return null;

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

export async function request(resource, suffix = '', options = {}) {
  const provider = getBackendProvider();
  const token = options.token ?? localStorage.getItem('taller2net.token');
  const url = provider.resolve(resource, suffix);
  const body = options.body instanceof FormData || typeof options.body === 'string'
    ? options.body
    : options.body
      ? JSON.stringify(options.body)
      : undefined;

  const response = await fetch(url, {
    method: options.method || 'GET',
    headers: buildHeaders(body, token),
    body,
    signal: options.signal
  });

  const payload = await parseResponse(response);

  if (response.status === 401) {
    unauthorizedHandler?.();
    throw new Error('Sesión expirada');
  }

  if (!response.ok) {
    throw new Error(payload?.message || payload?.Message || `HTTP ${response.status}`);
  }

  if (payload && payload.success === false) {
    throw new Error(payload.message || 'Operación fallida');
  }

  return payload;
}