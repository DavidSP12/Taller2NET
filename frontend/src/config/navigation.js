export const navigationItems = [
  { label: 'Dashboard', to: '/dashboard', roles: ['Admin', 'Teacher', 'Student'] },
  { label: 'Estudiantes', to: '/students', roles: ['Admin', 'Teacher', 'Student'] },
  { label: 'Cursos', to: '/courses', roles: ['Admin', 'Teacher', 'Student'] },
  { label: 'Matrículas', to: '/enrollments', roles: ['Admin', 'Teacher', 'Student'] },
  { label: 'Asistencias', to: '/attendance', roles: ['Admin', 'Teacher'] },
  { label: 'Notas', to: '/grades', roles: ['Admin', 'Teacher'] },
  { label: 'Reportes', to: '/reports', roles: ['Admin', 'Teacher'] },
  { label: 'Perfil', to: '/profile', roles: ['Admin', 'Teacher', 'Student'] }
];