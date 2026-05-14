const normalizeBase = (value) => (value || '').replace(/\/+$/, '');

export function createMonolithProvider(env = import.meta.env) {
  const baseUrl = normalizeBase(env.VITE_API_GATEWAY_URL || '/api');

  return {
    name: 'monolith',
    resolve(resource, suffix = '') {
      const routes = {
        auth: '/api/auth',
        students: '/api/students',
        courses: '/api/courses',
        enrollments: '/api/enrollments',
        attendance: '/api/attendances',
        grades: '/api/grades',
        dashboard: '/api/dashboard'
      };

      return `${baseUrl}${routes[resource] || ''}${suffix}`;
    }
  };
}