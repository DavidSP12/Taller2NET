import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Button, Badge } from '../common/UI';

const routeTitles = {
  '/dashboard': 'Dashboard',
  '/students': 'Estudiantes',
  '/courses': 'Cursos',
  '/enrollments': 'Matrículas',
  '/attendance': 'Asistencias',
  '/grades': 'Notas',
  '/reports': 'Reportes',
  '/profile': 'Perfil'
};

export function Topbar({ onMenuToggle }) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  const label = Object.entries(routeTitles).find(([path]) => location.pathname.startsWith(path))?.[1] || 'Taller2NET';

  return (
    <header className="topbar">
      <div className="topbar__left">
        <Button variant="ghost" className="topbar__menu" onClick={onMenuToggle}>☰</Button>
        <div>
          <p className="topbar__eyebrow">Sistema LMS</p>
          <h1>{label}</h1>
        </div>
      </div>

      <div className="topbar__right">
        <Badge tone="info">{user?.role || 'Student'}</Badge>
        <button className="topbar__user" onClick={() => navigate('/profile')}>
          <span>{user?.username || 'Usuario'}</span>
          <small>{user?.email || ''}</small>
        </button>
        <Button variant="secondary" onClick={logout}>Salir</Button>
      </div>
    </header>
  );
}