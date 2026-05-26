import http from 'k6/http';
import { check, sleep } from 'k6';
import { ENDPOINTS, authHeaders, loginAndGetToken } from './config.js';

export const options = {
  scenarios: {
    stress: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '1m', target: 25 },
        { duration: '2m', target: 75 },
        { duration: '2m', target: 150 },
        { duration: '2m', target: 250 },
        { duration: '1m', target: 0 },
      ],
      gracefulRampDown: '10s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.15'],
    http_req_duration: ['p(95)<2500', 'p(99)<4000'],
    checks: ['rate>0.90'],
  },
};

export function setup() {
  return {
    token: loginAndGetToken(),
  };
}

function pickOperation(token) {
  const auth = authHeaders(token);
  const operations = [
    () => http.get(ENDPOINTS.dashboardSummary, auth),
    () => http.get(ENDPOINTS.dashboardTopCourses(10), auth),
    () => http.get(ENDPOINTS.students(1, 25), auth),
    () => http.get(ENDPOINTS.courses(1, 25), auth),
  ];

  return operations[Math.floor(Math.random() * operations.length)]();
}

export default function (data) {
  const response = pickOperation(data.token);
  check(response, {
    'critical endpoint responded': (r) => r.status === 200,
    'critical endpoint returned payload': (r) => Boolean(r.body),
  });

  if (__ITER % 10 === 0) {
    const health = http.get(ENDPOINTS.health);
    check(health, {
      'gateway still healthy under stress': (r) => r.status === 200,
    });
  }

  sleep(0.5);
}
