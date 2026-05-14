import { Link } from 'react-router-dom';
import { Button, Card } from '../../components/common/UI';

export function UnauthorizedPage() {
  return (
    <div className="system-state">
      <Card className="system-state__card">
        <h1>No autorizado</h1>
        <p>No tienes permisos para acceder a esta ruta.</p>
        <Button as={Link} to="/dashboard">Volver al dashboard</Button>
      </Card>
    </div>
  );
}