import { useEffect, useState } from 'react';
import { Button, Card, EmptyState, Loader, PageHeader, StatGrid, DataTable, Badge } from '../../components/common/UI';
import { reportService } from '../../services/reportService';
import { formatDateTime, formatNumber } from '../../utils/formatters';
import { Link } from 'react-router-dom';

export function DashboardPage() {
  const [summary, setSummary] = useState(null);
  const [topCourses, setTopCourses] = useState([]);
  const [activity, setActivity] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;

    async function load() {
      setLoading(true);
      setError('');
      try {
        const [summaryResponse, coursesResponse, activityResponse] = await Promise.all([
          reportService.summary(),
          reportService.topCourses(5),
          reportService.activity(8)
        ]);

        if (!active) return;
        setSummary(summaryResponse);
        setTopCourses(coursesResponse.items || []);
        setActivity(activityResponse.items || []);
      } catch (loadError) {
        if (active) setError(loadError.message || 'No fue posible cargar el dashboard');
      } finally {
        if (active) setLoading(false);
      }
    }

    load();
    return () => { active = false; };
  }, []);

  if (loading) return <Loader />;
  if (error) return <EmptyState title="No se pudo cargar el dashboard" description={error} />;

  const stats = summary ? [
    { label: 'Estudiantes', value: summary.totalStudents, icon: '👨‍🎓', tone: 'info' },
    { label: 'Cursos', value: summary.totalCourses, icon: '📚', tone: 'success' },
    { label: 'Matrículas', value: summary.totalEnrollments, icon: '📝', tone: 'warning' },
    { label: 'Promedio global', value: formatNumber(summary.globalAverageGrade, 2), icon: '📊', tone: 'accent' },
    { label: 'Asistencia', value: `${formatNumber(summary.globalAttendanceRate, 1)}%`, icon: '📅', tone: 'success' },
    { label: 'Actualizado', value: formatDateTime(summary.generatedAt), icon: '⏱', tone: 'neutral' }
  ] : [];

  return (
    <div className="page-stack">
      <PageHeader
        title="Dashboard académico"
        subtitle="Vista ejecutiva de la operación LMS, con métricas y accesos rápidos."
        actions={[
          <Button key="students" variant="secondary" as={Link} to="/students">Ir a estudiantes</Button>
        ]}
      />

      <StatGrid items={stats} />

      <div className="dashboard-grid">
        <Card>
          <div className="section-heading">
            <h2>Top cursos</h2>
            <Link to="/reports">Ver reportes</Link>
          </div>
          <DataTable
            columns={[
              { key: 'name', label: 'Curso' },
              { key: 'enrolledCount', label: 'Inscritos' },
              { key: 'averageGrade', label: 'Promedio', render: (item) => formatNumber(item.averageGrade, 2) }
            ]}
            rows={topCourses}
          />
        </Card>

        <Card>
          <div className="section-heading">
            <h2>Actividad reciente</h2>
          </div>
          {activity.length ? (
            <div className="activity-list">
              {activity.map((item) => (
                <article key={`${item.type}-${item.timestamp}`} className="activity-item">
                  <Badge tone="info">{item.type}</Badge>
                  <div>
                    <strong>{item.description}</strong>
                    <p>{formatDateTime(item.timestamp)}</p>
                  </div>
                </article>
              ))}
            </div>
          ) : (
            <EmptyState title="Sin actividad" description="Todavía no hay eventos recientes para mostrar." />
          )}
        </Card>
      </div>
    </div>
  );
}