import { Link } from 'react-router-dom';
import { Button, Card } from '../../components/common/UI';

export function NotFoundPage() {
  return (
    <div className="system-state">
      <Card className="system-state__card">
        <h1>Página no encontrada</h1>
        <p>La ruta solicitada no existe o fue movida.</p>
        <Button as={Link} to="/dashboard">Ir al inicio</Button>
      </Card>
    </div>
  );
}