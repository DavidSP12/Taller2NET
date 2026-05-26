import http from 'k6/http';
import { check } from 'k6';

export const BASE_URL = (__ENV.BASE_URL || 'http://localhost').replace(/\/$/, '');
export const USERNAME = __ENV.USERNAME || 'admin';
export const PASSWORD = __ENV.PASSWORD || 'Admin@123';

export const ENDPOINTS = {
  health: `${BASE_URL}/health`,
  academicHealth: `${BASE_URL}/academic/health`,
  statisticsHealth: `${BASE_URL}/statistics/health`,
  login: `${BASE_URL}/api/auth/login`,
  dashboardSummary: `${BASE_URL}/api/dashboard/summary`,
  dashboardTopCourses: (top = 5) => `${BASE_URL}/api/dashboard/top-courses?top=${top}`,
  students: (page = 1, pageSize = 10, search = '') => {
    const query = [`page=${page}`, `pageSize=${pageSize}`];
    if (search) {
      query.push(`search=${encodeURIComponent(search)}`);
    }
    return `${BASE_URL}/api/students?${query.join('&')}`;
  },
  courses: (page = 1, pageSize = 10, search = '') => {
    const query = [`page=${page}`, `pageSize=${pageSize}`];
    if (search) {
      query.push(`search=${encodeURIComponent(search)}`);
    }
    return `${BASE_URL}/api/courses?${query.join('&')}`;
  },
};

export function authHeaders(token) {
  return {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  };
}

export function loginAndGetToken(username = USERNAME, password = PASSWORD) {
  const response = http.post(
    ENDPOINTS.login,
    JSON.stringify({ username, password }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  check(response, {
    'login returns 200 or 201': (r) => r.status === 200 || r.status === 201,
    'login response contains token': (r) => {
      const body = r.json();
      return Boolean(body?.data?.token || body?.data?.accessToken || body?.token);
    },
  });

  const body = response.json();
  const token = body?.data?.token || body?.data?.accessToken || body?.token;

  if (!token) {
    throw new Error(`No se pudo obtener un JWT desde ${ENDPOINTS.login}`);
  }

  return token;
}
