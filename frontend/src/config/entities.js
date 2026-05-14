import { studentService } from '../services/studentService';
import { courseService } from '../services/courseService';
import { enrollmentService } from '../services/enrollmentService';
import { attendanceService } from '../services/attendanceService';
import { gradeService } from '../services/gradeService';
import { formatDate, formatDateTime, formatNumber } from '../utils/formatters';
import { Badge } from '../components/common/UI';

const writeRoles = ['Admin', 'Teacher'];
const managementRoles = ['Admin', 'Teacher'];

const statusTone = (status) => {
  const normalized = String(status || '').toLowerCase();
  if (normalized.includes('active') || normalized.includes('present') || normalized.includes('approved')) return 'success';
  if (normalized.includes('withdraw') || normalized.includes('absent') || normalized.includes('failed')) return 'danger';
  if (normalized.includes('late') || normalized.includes('risk')) return 'warning';
  return 'neutral';
};

const enrollmentStatusOptions = [
  { value: 'Active', label: 'Active' },
  { value: 'Withdrawn', label: 'Withdrawn' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Failed', label: 'Failed' }
];

const attendanceStatusOptions = [
  { value: 'Present', label: 'Present' },
  { value: 'Absent', label: 'Absent' },
  { value: 'Late', label: 'Late' },
  { value: 'Excused', label: 'Excused' }
];

const gradeTypeOptions = [
  { value: 'Quiz', label: 'Quiz' },
  { value: 'Midterm', label: 'Midterm' },
  { value: 'Final', label: 'Final' },
  { value: 'Project', label: 'Project' },
  { value: 'Assignment', label: 'Assignment' }
];

function mapOptions(items, labelBuilder) {
  return items.map((item) => ({ value: item.id, label: labelBuilder(item) }));
}

function studentDetailLabel(item) {
  return `${item.firstName} ${item.lastName}`;
}

export const studentConfig = {
  title: 'Estudiantes',
  singular: 'Estudiante',
  subtitle: 'Gestión académica de estudiantes con CRUD ADO.NET y navegación por páginas.',
  routeBase: '/students',
  service: studentService,
  allowCreate: true,
  allowDelete: true,
  writeRoles,
  pageSize: 10,
  columns: [
    { key: 'studentCode', label: 'Código' },
    { key: 'fullName', label: 'Nombre', render: (item) => `${item.firstName} ${item.lastName}` },
    { key: 'program', label: 'Programa' },
    { key: 'semester', label: 'Semestre' },
    { key: 'isActive', label: 'Estado', render: (item) => <Badge tone={item.isActive ? 'success' : 'danger'}>{item.isActive ? 'Activo' : 'Inactivo'}</Badge> }
  ],
  detailFields: [
    { name: 'studentCode', label: 'Código', type: 'text' },
    { name: 'firstName', label: 'Nombres', type: 'text' },
    { name: 'lastName', label: 'Apellidos', type: 'text' },
    { name: 'email', label: 'Correo', type: 'text' },
    { name: 'phone', label: 'Teléfono', type: 'text' },
    { name: 'dateOfBirth', label: 'Fecha de nacimiento', type: 'date' },
    { name: 'program', label: 'Programa', type: 'text' },
    { name: 'semester', label: 'Semestre', type: 'number' },
    { name: 'isActive', label: 'Estado', type: 'boolean', render: (item) => (item.isActive ? 'Activo' : 'Inactivo') },
    { name: 'createdAt', label: 'Creado', type: 'datetime-local', render: (item) => formatDateTime(item.createdAt) }
  ],
  createFields: [
    { name: 'userId', label: 'Usuario asociado', type: 'number', required: true, helperText: 'Debe existir previamente en la tabla Users.', span: 2 },
    { name: 'studentCode', label: 'Código', type: 'text', required: true },
    { name: 'firstName', label: 'Nombres', type: 'text', required: true },
    { name: 'lastName', label: 'Apellidos', type: 'text', required: true },
    { name: 'email', label: 'Correo', type: 'email', required: true },
    { name: 'phone', label: 'Teléfono', type: 'text' },
    { name: 'dateOfBirth', label: 'Fecha de nacimiento', type: 'date', required: true },
    { name: 'program', label: 'Programa', type: 'text', required: true },
    { name: 'semester', label: 'Semestre', type: 'number', required: true },
    { name: 'isActive', label: 'Activo', type: 'checkbox', defaultValue: true }
  ],
  editFields: [
    { name: 'studentCode', label: 'Código', type: 'text', readOnlyOnEdit: true, helperText: 'El código no se modifica desde este formulario.' },
    { name: 'firstName', label: 'Nombres', type: 'text', required: true },
    { name: 'lastName', label: 'Apellidos', type: 'text', required: true },
    { name: 'email', label: 'Correo', type: 'email', required: true },
    { name: 'phone', label: 'Teléfono', type: 'text' },
    { name: 'dateOfBirth', label: 'Fecha de nacimiento', type: 'date', readOnlyOnEdit: true },
    { name: 'program', label: 'Programa', type: 'text', required: true },
    { name: 'semester', label: 'Semestre', type: 'number', required: true },
    { name: 'isActive', label: 'Activo', type: 'checkbox', defaultValue: true }
  ]
};

export const courseConfig = {
  title: 'Cursos',
  singular: 'Curso',
  subtitle: 'Administración de cursos, cupos e inscritos con persistencia ADO.NET.',
  routeBase: '/courses',
  service: courseService,
  allowCreate: true,
  allowDelete: true,
  writeRoles,
  pageSize: 10,
  columns: [
    { key: 'code', label: 'Código' },
    { key: 'name', label: 'Nombre' },
    { key: 'teacher', label: 'Docente' },
    { key: 'semester', label: 'Semestre' },
    { key: 'enrolledCount', label: 'Inscritos', render: (item) => item.enrolledCount },
    { key: 'isActive', label: 'Estado', render: (item) => <Badge tone={item.isActive ? 'success' : 'danger'}>{item.isActive ? 'Activo' : 'Inactivo'}</Badge> }
  ],
  detailFields: [
    { name: 'code', label: 'Código', type: 'text' },
    { name: 'name', label: 'Nombre', type: 'text' },
    { name: 'description', label: 'Descripción', type: 'textarea' },
    { name: 'credits', label: 'Créditos', type: 'number' },
    { name: 'teacher', label: 'Docente', type: 'text' },
    { name: 'semester', label: 'Semestre', type: 'number' },
    { name: 'maxStudents', label: 'Cupo máximo', type: 'number' },
    { name: 'schedule', label: 'Horario', type: 'text' },
    { name: 'enrolledCount', label: 'Inscritos', type: 'number' },
    { name: 'isActive', label: 'Estado', type: 'boolean', render: (item) => (item.isActive ? 'Activo' : 'Inactivo') }
  ],
  createFields: [
    { name: 'code', label: 'Código', type: 'text', required: true },
    { name: 'name', label: 'Nombre', type: 'text', required: true },
    { name: 'description', label: 'Descripción', type: 'textarea', required: true, span: 2 },
    { name: 'credits', label: 'Créditos', type: 'number', required: true },
    { name: 'teacher', label: 'Docente', type: 'text', required: true },
    { name: 'semester', label: 'Semestre', type: 'number', required: true },
    { name: 'maxStudents', label: 'Cupo máximo', type: 'number', required: true },
    { name: 'schedule', label: 'Horario', type: 'text', required: true, span: 2 },
    { name: 'isActive', label: 'Activo', type: 'checkbox', defaultValue: true }
  ],
  editFields: [
    { name: 'code', label: 'Código', type: 'text', readOnlyOnEdit: true },
    { name: 'name', label: 'Nombre', type: 'text', required: true },
    { name: 'description', label: 'Descripción', type: 'textarea', required: true, span: 2 },
    { name: 'credits', label: 'Créditos', type: 'number', required: true },
    { name: 'teacher', label: 'Docente', type: 'text', required: true },
    { name: 'semester', label: 'Semestre', type: 'number', readOnlyOnEdit: true },
    { name: 'maxStudents', label: 'Cupo máximo', type: 'number', required: true },
    { name: 'schedule', label: 'Horario', type: 'text', required: true, span: 2 },
    { name: 'isActive', label: 'Activo', type: 'checkbox', defaultValue: true }
  ]
};

export const enrollmentConfig = {
  title: 'Matrículas',
  singular: 'Matrícula',
  subtitle: 'Inscripción y seguimiento de estudiantes por curso.',
  routeBase: '/enrollments',
  service: enrollmentService,
  allowCreate: true,
  allowDelete: true,
  writeRoles: managementRoles,
  pageSize: 10,
  loadOptions: async () => {
    const [students, courses] = await Promise.all([
      studentService.list({ page: 1, pageSize: 200 }),
      courseService.list({ page: 1, pageSize: 200 })
    ]);

    return {
      students: mapOptions(students.items || [], (student) => `${student.studentCode} · ${student.firstName} ${student.lastName}`),
      courses: mapOptions(courses.items || [], (course) => `${course.code} · ${course.name}`)
    };
  },
  columns: [
    { key: 'studentName', label: 'Estudiante' },
    { key: 'courseName', label: 'Curso' },
    { key: 'status', label: 'Estado', render: (item) => <Badge tone={statusTone(item.status)}>{item.status}</Badge> },
    { key: 'enrolledAt', label: 'Inscrita', render: (item) => formatDateTime(item.enrolledAt) }
  ],
  detailFields: [
    { name: 'studentName', label: 'Estudiante', type: 'text' },
    { name: 'courseName', label: 'Curso', type: 'text' },
    { name: 'courseCode', label: 'Código del curso', type: 'text' },
    { name: 'status', label: 'Estado', type: 'text' },
    { name: 'enrolledAt', label: 'Fecha de matrícula', type: 'datetime-local', render: (item) => formatDateTime(item.enrolledAt) },
    { name: 'withdrawnAt', label: 'Retiro', type: 'datetime-local', render: (item) => formatDateTime(item.withdrawnAt) }
  ],
  createFields: [
    { name: 'studentId', label: 'Estudiante', type: 'select', required: true, optionsKey: 'students', span: 2 },
    { name: 'courseId', label: 'Curso', type: 'select', required: true, optionsKey: 'courses', span: 2 }
  ],
  editFields: [
    { name: 'studentId', label: 'Estudiante', type: 'select', required: true, optionsKey: 'students', readOnlyOnEdit: true, span: 2 },
    { name: 'courseId', label: 'Curso', type: 'select', required: true, optionsKey: 'courses', readOnlyOnEdit: true, span: 2 },
    { name: 'status', label: 'Estado', type: 'select', required: true, options: enrollmentStatusOptions },
    { name: 'withdrawnAt', label: 'Fecha de retiro', type: 'datetime-local' }
  ],
  preparePayload: (values, options, mode) => ({
    ...(mode === 'create' ? { studentId: Number(values.studentId), courseId: Number(values.courseId) } : {}),
    ...(mode === 'edit' ? { status: values.status, withdrawnAt: values.withdrawnAt || null } : {})
  })
};

export const attendanceConfig = {
  title: 'Asistencias',
  singular: 'Asistencia',
  subtitle: 'Control de asistencia por matrícula con DataReaders en el backend.',
  routeBase: '/attendance',
  service: attendanceService,
  allowCreate: true,
  allowDelete: true,
  writeRoles: managementRoles,
  pageSize: 10,
  loadOptions: async () => {
    const enrollments = await enrollmentService.list();
    return {
      enrollments: mapOptions(enrollments.items || [], (enrollment) => `${enrollment.studentName} · ${enrollment.courseName}`)
    };
  },
  columns: [
    { key: 'studentName', label: 'Estudiante' },
    { key: 'courseName', label: 'Curso' },
    { key: 'date', label: 'Fecha', render: (item) => formatDate(item.date) },
    { key: 'status', label: 'Estado', render: (item) => <Badge tone={statusTone(item.status)}>{item.status}</Badge> }
  ],
  detailFields: [
    { name: 'studentName', label: 'Estudiante', type: 'text' },
    { name: 'courseName', label: 'Curso', type: 'text' },
    { name: 'date', label: 'Fecha', type: 'date', render: (item) => formatDate(item.date) },
    { name: 'status', label: 'Estado', type: 'text' },
    { name: 'notes', label: 'Observaciones', type: 'textarea' }
  ],
  createFields: [
    { name: 'enrollmentId', label: 'Matrícula', type: 'select', required: true, optionsKey: 'enrollments', span: 2 },
    { name: 'date', label: 'Fecha', type: 'date', required: true },
    { name: 'status', label: 'Estado', type: 'select', required: true, options: attendanceStatusOptions },
    { name: 'notes', label: 'Observaciones', type: 'textarea', span: 2 }
  ],
  editFields: [
    { name: 'enrollmentId', label: 'Matrícula', type: 'select', required: true, optionsKey: 'enrollments', readOnlyOnEdit: true, span: 2 },
    { name: 'date', label: 'Fecha', type: 'date', required: true },
    { name: 'status', label: 'Estado', type: 'select', required: true, options: attendanceStatusOptions },
    { name: 'notes', label: 'Observaciones', type: 'textarea', span: 2 }
  ]
};

export const gradeConfig = {
  title: 'Notas',
  singular: 'Nota',
  subtitle: 'Registro y seguimiento de calificaciones por matrícula.',
  routeBase: '/grades',
  service: gradeService,
  allowCreate: true,
  allowDelete: true,
  writeRoles: managementRoles,
  pageSize: 10,
  loadOptions: async () => {
    const enrollments = await enrollmentService.list();
    return {
      enrollments: mapOptions(enrollments.items || [], (enrollment) => `${enrollment.studentName} · ${enrollment.courseName}`)
    };
  },
  columns: [
    { key: 'studentName', label: 'Estudiante' },
    { key: 'courseName', label: 'Curso' },
    { key: 'type', label: 'Tipo', render: (item) => <Badge tone={statusTone(item.type)}>{item.type}</Badge> },
    { key: 'value', label: 'Valor', render: (item) => formatNumber(item.value, 2) },
    { key: 'weight', label: 'Peso', render: (item) => formatNumber(item.weight, 2) },
    { key: 'evaluatedAt', label: 'Evaluada', render: (item) => formatDate(item.evaluatedAt) }
  ],
  detailFields: [
    { name: 'studentName', label: 'Estudiante', type: 'text' },
    { name: 'courseName', label: 'Curso', type: 'text' },
    { name: 'type', label: 'Tipo', type: 'text' },
    { name: 'value', label: 'Valor', type: 'number' },
    { name: 'weight', label: 'Peso', type: 'number' },
    { name: 'description', label: 'Descripción', type: 'textarea' },
    { name: 'evaluatedAt', label: 'Fecha de evaluación', type: 'date', render: (item) => formatDate(item.evaluatedAt) }
  ],
  createFields: [
    { name: 'enrollmentId', label: 'Matrícula', type: 'select', required: true, optionsKey: 'enrollments', span: 2 },
    { name: 'type', label: 'Tipo', type: 'select', required: true, options: gradeTypeOptions },
    { name: 'value', label: 'Valor', type: 'number', required: true },
    { name: 'weight', label: 'Peso', type: 'number', required: true },
    { name: 'description', label: 'Descripción', type: 'textarea', span: 2 },
    { name: 'evaluatedAt', label: 'Fecha de evaluación', type: 'date', required: true }
  ],
  editFields: [
    { name: 'enrollmentId', label: 'Matrícula', type: 'select', required: true, optionsKey: 'enrollments', readOnlyOnEdit: true, span: 2 },
    { name: 'type', label: 'Tipo', type: 'select', required: true, options: gradeTypeOptions },
    { name: 'value', label: 'Valor', type: 'number', required: true },
    { name: 'weight', label: 'Peso', type: 'number', required: true },
    { name: 'description', label: 'Descripción', type: 'textarea', span: 2 },
    { name: 'evaluatedAt', label: 'Fecha de evaluación', type: 'date', required: true }
  ]
};