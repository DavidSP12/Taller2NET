import http from 'k6/http';
import { check, sleep } from 'k6';
import { ENDPOINTS, authHeaders, loginAndGetToken } from './config.js';

export const options = {
  scenarios: {
    availability: {
      executor: 'constant-vus',
      vus: Number(__ENV.VUS || 5),
      duration: __ENV.DURATION || '3m',
      gracefulStop: '10s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<900'],
    checks: ['rate>0.98'],
  },
};

export function setup() {
  return {
    token: loginAndGetToken(),
  };
}

export default function (data) {
  const auth = authHeaders(data.token);

  const gatewayHealth = http.get(ENDPOINTS.health);
  check(gatewayHealth, {
    'gateway health is ok': (r) => r.status === 200,
    'gateway reports healthy': (r) => r.json()?.Status === 'Healthy' || r.json()?.status === 'Healthy',
  });

  const academicHealth = http.get(ENDPOINTS.academicHealth);
  check(academicHealth, {
    'academic health is ok': (r) => r.status === 200,
    'academic reports healthy': (r) => r.json()?.Status === 'Healthy' || r.json()?.status === 'Healthy',
  });

  const statisticsHealth = http.get(ENDPOINTS.statisticsHealth);
  check(statisticsHealth, {
    'statistics health is ok': (r) => r.status === 200,
    'statistics reports healthy': (r) => r.json()?.Status === 'Healthy' || r.json()?.status === 'Healthy',
  });

  const summary = http.get(ENDPOINTS.dashboardSummary, auth);
  check(summary, {
    'dashboard summary is ok': (r) => r.status === 200,
    'dashboard summary contains data': (r) => Boolean(r.json()?.data),
  });

  const topCourses = http.get(ENDPOINTS.dashboardTopCourses(3), auth);
  check(topCourses, {
    'top courses is ok': (r) => r.status === 200,
    'top courses contains data': (r) => Boolean(r.json()?.data),
  });

  sleep(1);
}
