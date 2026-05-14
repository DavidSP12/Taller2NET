const normalizeBase = (value) => (value || '').replace(/\/+$/, '');

export function createMicroservicesProvider(env = import.meta.env) {
  const authBase = normalizeBase(env.VITE_AUTH_SERVICE_URL || env.VITE_API_GATEWAY_URL || '/api');
  const academicBase = normalizeBase(env.VITE_ACADEMIC_SERVICE_URL || env.VITE_API_GATEWAY_URL || '/api');
  const statisticsBase = normalizeBase(env.VITE_STATISTICS_SERVICE_URL || env.VITE_API_GATEWAY_URL || '/api');

  const map = {
    auth: authBase,
    students: academicBase,
    courses: academicBase,
    enrollments: academicBase,
    attendance: academicBase,
    grades: academicBase,
    dashboard: statisticsBase
  };

  const routes = {
    auth: '/api/auth',
    students: '/api/students',
    courses: '/api/courses',
    enrollments: '/api/enrollments',
    attendance: '/api/attendances',
    grades: '/api/grades',
    dashboard: '/api/dashboard'
  };

  return {
    name: 'microservices',
    resolve(resource, suffix = '') {
      return `${map[resource] || academicBase}${routes[resource] || ''}${suffix}`;
    }
  };
}