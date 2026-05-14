import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Alert, Button, Card } from '../../components/common/UI';

export function LoginPage() {
  const { login, isAuthenticated, isTokenExpired } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState({ username: '', password: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (isAuthenticated && !isTokenExpired) {
      navigate('/dashboard', { replace: true });
    }
  }, [isAuthenticated, isTokenExpired, navigate]);

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setLoading(true);
    setError('');
    try {
      await login(form);
      const redirectTo = location.state?.from?.pathname || '/dashboard';
      navigate(redirectTo, { replace: true });
    } catch (submitError) {
      setError(submitError.message || 'No fue posible iniciar sesión');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="auth-card-shell">
      <Card className="auth-card">
        <div className="auth-card__header">
          <span className="auth-card__eyebrow">Acceso seguro</span>
          <h2>Iniciar sesión</h2>
          <p>Usa tus credenciales institucionales para entrar al LMS.</p>
        </div>

        {error ? <Alert>{error}</Alert> : null}

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            <span>Usuario</span>
            <input name="username" value={form.username} onChange={handleChange} autoComplete="username" required />
          </label>
          <label>
            <span>Contraseña</span>
            <input name="password" type="password" value={form.password} onChange={handleChange} autoComplete="current-password" required />
          </label>
          <Button type="submit" disabled={loading}>{loading ? 'Ingresando...' : 'Entrar al sistema'}</Button>
        </form>

        <div className="auth-card__hint">
          <strong>Usuarios de prueba</strong>
          <p>admin / Admin@123 · teacher01 / Teacher@123 · student01 / Student@123</p>
        </div>
      </Card>
    </div>
  );
}