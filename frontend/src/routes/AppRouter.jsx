import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { RoleRoute } from './RoleRoute';
import { AuthLayout } from '../layouts/AuthLayout';
import { DashboardLayout } from '../layouts/DashboardLayout';
import { LoginPage } from '../pages/auth/LoginPage';
import { DashboardPage } from '../pages/dashboard/DashboardPage';
import { StudentsListPage } from '../pages/students/StudentsListPage';
import { StudentCreatePage } from '../pages/students/StudentCreatePage';
import { StudentDetailPage } from '../pages/students/StudentDetailPage';
import { StudentEditPage } from '../pages/students/StudentEditPage';
import { CoursesListPage } from '../pages/courses/CoursesListPage';
import { CourseCreatePage } from '../pages/courses/CourseCreatePage';
import { CourseDetailPage } from '../pages/courses/CourseDetailPage';
import { CourseEditPage } from '../pages/courses/CourseEditPage';
import { EnrollmentsListPage } from '../pages/enrollments/EnrollmentsListPage';
import { EnrollmentCreatePage } from '../pages/enrollments/EnrollmentCreatePage';
import { EnrollmentDetailPage } from '../pages/enrollments/EnrollmentDetailPage';
import { EnrollmentEditPage } from '../pages/enrollments/EnrollmentEditPage';
import { AttendanceListPage } from '../pages/attendance/AttendanceListPage';
import { AttendanceCreatePage } from '../pages/attendance/AttendanceCreatePage';
import { AttendanceDetailPage } from '../pages/attendance/AttendanceDetailPage';
import { AttendanceEditPage } from '../pages/attendance/AttendanceEditPage';
import { GradesListPage } from '../pages/grades/GradesListPage';
import { GradeCreatePage } from '../pages/grades/GradeCreatePage';
import { GradeDetailPage } from '../pages/grades/GradeDetailPage';
import { GradeEditPage } from '../pages/grades/GradeEditPage';
import { ReportsPage } from '../pages/reports/ReportsPage';
import { ProfilePage } from '../pages/profile/ProfilePage';
import { UnauthorizedPage } from '../pages/system/UnauthorizedPage';
import { NotFoundPage } from '../pages/system/NotFoundPage';

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route element={<DashboardLayout />}>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/reports" element={<ReportsPage />} />

          <Route path="/students" element={<StudentsListPage />} />
          <Route path="/students/new" element={<RoleRoute roles={["Admin", "Teacher"]}><StudentCreatePage /></RoleRoute>} />
          <Route path="/students/:id" element={<StudentDetailPage />} />
          <Route path="/students/:id/edit" element={<RoleRoute roles={["Admin", "Teacher"]}><StudentEditPage /></RoleRoute>} />

          <Route path="/courses" element={<CoursesListPage />} />
          <Route path="/courses/new" element={<RoleRoute roles={["Admin", "Teacher"]}><CourseCreatePage /></RoleRoute>} />
          <Route path="/courses/:id" element={<CourseDetailPage />} />
          <Route path="/courses/:id/edit" element={<RoleRoute roles={["Admin", "Teacher"]}><CourseEditPage /></RoleRoute>} />

          <Route path="/enrollments" element={<EnrollmentsListPage />} />
          <Route path="/enrollments/new" element={<RoleRoute roles={["Admin", "Teacher"]}><EnrollmentCreatePage /></RoleRoute>} />
          <Route path="/enrollments/:id" element={<EnrollmentDetailPage />} />
          <Route path="/enrollments/:id/edit" element={<RoleRoute roles={["Admin", "Teacher"]}><EnrollmentEditPage /></RoleRoute>} />

          <Route path="/attendance" element={<AttendanceListPage />} />
          <Route path="/attendance/new" element={<RoleRoute roles={["Admin", "Teacher"]}><AttendanceCreatePage /></RoleRoute>} />
          <Route path="/attendance/:id" element={<AttendanceDetailPage />} />
          <Route path="/attendance/:id/edit" element={<RoleRoute roles={["Admin", "Teacher"]}><AttendanceEditPage /></RoleRoute>} />

          <Route path="/grades" element={<GradesListPage />} />
          <Route path="/grades/new" element={<RoleRoute roles={["Admin", "Teacher"]}><GradeCreatePage /></RoleRoute>} />
          <Route path="/grades/:id" element={<GradeDetailPage />} />
          <Route path="/grades/:id/edit" element={<RoleRoute roles={["Admin", "Teacher"]}><GradeEditPage /></RoleRoute>} />
        </Route>
      </Route>

      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}