import { Card, PageHeader } from '../../components/common/UI';
import { useAuth } from '../../context/AuthContext';
import { formatDateTime } from '../../utils/formatters';

export function ProfilePage() {
  const { user, token } = useAuth();

  return (
    <div className="page-stack">
      <PageHeader title="Perfil de usuario" subtitle="Información de la sesión activa y datos de autenticación." />

      <Card className="profile-card">
        <div>
          <span className="profile-card__label">Usuario</span>
          <strong>{user?.username}</strong>
        </div>
        <div>
          <span className="profile-card__label">Correo</span>
          <strong>{user?.email || 'No registrado'}</strong>
        </div>
        <div>
          <span className="profile-card__label">Rol</span>
          <strong>{user?.role}</strong>
        </div>
        <div>
          <span className="profile-card__label">Expiración</span>
          <strong>{formatDateTime(user?.expiresAt)}</strong>
        </div>
        <div className="profile-card__token">
          <span className="profile-card__label">JWT</span>
          <code>{token.slice(0, 32)}...</code>
        </div>
      </Card>
    </div>
  );
}