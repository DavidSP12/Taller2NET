import http from 'k6/http';
import { check, sleep } from 'k6';
import { ENDPOINTS, authHeaders, loginAndGetToken } from './config.js';

export const options = {
  scenarios: {
    load: {
      executor: 'ramping-vus',
      startVUs: 1,
      stages: [
        { duration: '1m', target: 20 },
        { duration: '3m', target: 50 },
        { duration: '1m', target: 50 },
        { duration: '1m', target: 0 },
      ],
      gracefulRampDown: '20s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<1200', 'p(99)<1800'],
    checks: ['rate>0.99'],
  },
};

export function setup() {
  return {
    token: loginAndGetToken(),
  };
}

export default function (data) {
  const auth = authHeaders(data.token);

  const summary = http.get(ENDPOINTS.dashboardSummary, auth);
  check(summary, {
    'summary status is 200': (r) => r.status === 200,
    'summary returns data': (r) => Boolean(r.json()?.data),
  });

  const students = http.get(ENDPOINTS.students(1, 10), auth);
  check(students, {
    'students status is 200': (r) => r.status === 200,
    'students returns data': (r) => Boolean(r.json()?.data),
  });

  const courses = http.get(ENDPOINTS.courses(1, 10), auth);
  check(courses, {
    'courses status is 200': (r) => r.status === 200,
    'courses returns data': (r) => Boolean(r.json()?.data),
  });

  const topCourses = http.get(ENDPOINTS.dashboardTopCourses(5), auth);
  check(topCourses, {
    'top courses status is 200': (r) => r.status === 200,
    'top courses returns data': (r) => Boolean(r.json()?.data),
  });

  sleep(1);
}
