import { useEffect, useState } from 'react';
import { Badge, Card, EmptyState, Loader, PageHeader, StatGrid, DataTable } from '../../components/common/UI';
import { reportService } from '../../services/reportService';
import { formatNumber } from '../../utils/formatters';

export function ReportsPage() {
  const [summary, setSummary] = useState(null);
  const [courseRows, setCourseRows] = useState([]);
  const [programRows, setProgramRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [courseFilter, setCourseFilter] = useState('');

  async function loadData(filter = courseFilter) {
    setLoading(true);
    setError('');
    try {
      const [summaryResponse, coursesResponse, programsResponse] = await Promise.all([
        reportService.summary(),
        reportService.courses(),
        reportService.programs()
      ]);

      setSummary(summaryResponse);
      setCourseRows(coursesResponse.items || []);
      setProgramRows(programsResponse.items || []);

      if (filter) {
        await reportService.attendance(filter);
        await reportService.grades(filter);
      }
    } catch (loadError) {
      setError(loadError.message || 'No fue posible cargar los reportes');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData();
  }, []);

  if (loading) return <Loader />;
  if (error) return <EmptyState title="No se pudieron cargar los reportes" description={error} />;

  const stats = summary ? [
    { label: 'Total estudiantes', value: summary.totalStudents, icon: '👥', tone: 'info' },
    { label: 'Cursos activos', value: summary.activeCourses, icon: '📗', tone: 'success' },
    { label: 'Promedio global', value: formatNumber(summary.globalAverageGrade, 2), icon: '📈', tone: 'accent' },
    { label: 'Asistencia global', value: `${formatNumber(summary.globalAttendanceRate, 1)}%`, icon: '📅', tone: 'warning' }
  ] : [];

  return (
    <div className="page-stack">
      <PageHeader
        title="Reportes y estadísticas"
        subtitle="Métricas de operación y análisis de desempeño académico."
      />

      <StatGrid items={stats} />

      <Card>
        <div className="section-heading">
          <h2>Resumen por curso</h2>
          <Badge tone="neutral">Fuente estadística</Badge>
        </div>
        <DataTable
          columns={[
            { key: 'name', label: 'Curso' },
            { key: 'teacher', label: 'Docente' },
            { key: 'enrolledCount', label: 'Inscritos' },
            { key: 'averageGrade', label: 'Promedio', render: (item) => formatNumber(item.averageGrade, 2) },
            { key: 'attendanceRate', label: 'Asistencia', render: (item) => `${formatNumber(item.attendanceRate, 1)}%` }
          ]}
          rows={courseRows}
        />
      </Card>

      <Card>
        <div className="section-heading">
          <h2>Estadísticas por programa</h2>
        </div>
        <DataTable
          columns={[
            { key: 'programName', label: 'Programa' },
            { key: 'studentCount', label: 'Estudiantes' },
            { key: 'averageGrade', label: 'Promedio', render: (item) => formatNumber(item.averageGrade, 2) },
            { key: 'attendanceRate', label: 'Asistencia', render: (item) => `${formatNumber(item.attendanceRate, 1)}%` }
          ]}
          rows={programRows}
        />
      </Card>
    </div>
  );
}