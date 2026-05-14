import { NavLink } from 'react-router-dom';
import { navigationItems } from '../../config/navigation';
import { useAuth } from '../../context/AuthContext';

export function Sidebar({ open, onClose }) {
  const { user } = useAuth();
  const items = navigationItems.filter((item) => item.roles.includes(user?.role));

  return (
    <aside className={`sidebar ${open ? 'sidebar--open' : ''}`}>
      <div className="sidebar__brand">
        <div className="sidebar__mark">LMS</div>
        <div>
          <strong>Taller2NET</strong>
          <span>Academic Suite</span>
        </div>
      </div>

      <nav className="sidebar__nav">
        {items.map((item) => (
          <NavLink key={item.to} to={item.to} onClick={onClose} className={({ isActive }) => `sidebar__link ${isActive ? 'is-active' : ''}`}>
            {item.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}