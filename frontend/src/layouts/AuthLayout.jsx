import { Outlet } from 'react-router-dom';

export function AuthLayout() {
  return (
    <main className="auth-layout">
      <section className="auth-layout__hero">
        <div className="auth-brand">Taller2NET LMS</div>
        <h1>Gestión académica moderna, responsive y lista para presentar.</h1>
        <p>
          Autenticación JWT, rutas protegidas, módulos académicos completos y una experiencia visual diseñada para móvil, tablet y escritorio.
        </p>
        <div className="auth-highlights">
          <span>React Router real</span>
          <span>API Gateway</span>
          <span>DataReaders</span>
        </div>
      </section>

      <section className="auth-layout__panel">
        <Outlet />
      </section>
    </main>
  );
}