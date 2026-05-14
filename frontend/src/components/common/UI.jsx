import { Link } from 'react-router-dom';
import { useMemo } from 'react';
import { formatDate, formatDateTime } from '../../utils/formatters';

export function Button({ children, as: Component = 'button', variant = 'primary', size = 'md', type = 'button', onClick, disabled, className = '', ...props }) {
  const classes = ['btn', `btn-${variant}`, `btn-${size}`, className].filter(Boolean).join(' ');
  return (
    <Component type={Component === 'button' ? type : undefined} className={classes} onClick={onClick} disabled={disabled} {...props}>
      {children}
    </Component>
  );
}

export function Badge({ children, tone = 'neutral' }) {
  return <span className={`badge badge-${tone}`}>{children}</span>;
}

export function Card({ children, className = '' }) {
  return <section className={`card ${className}`.trim()}>{children}</section>;
}

export function Loader({ label = 'Cargando...' }) {
  return (
    <div className="loader-shell">
      <div className="loader-spinner" />
      <p>{label}</p>
    </div>
  );
}

export function EmptyState({ title, description, action }) {
  return (
    <div className="empty-state">
      <div className="empty-state__icon">∅</div>
      <h3>{title}</h3>
      <p>{description}</p>
      {action}
    </div>
  );
}

export function Alert({ children, tone = 'danger' }) {
  return <div className={`alert alert-${tone}`}>{children}</div>;
}

export function Pagination({ page, totalCount, pageSize, onPageChange }) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const pages = useMemo(() => {
    const start = Math.max(1, page - 2);
    const end = Math.min(totalPages, page + 2);
    const list = [];
    for (let current = start; current <= end; current += 1) list.push(current);
    return list;
  }, [page, totalPages]);

  if (totalPages <= 1) return null;

  return (
    <div className="pagination">
      <Button variant="ghost" size="sm" disabled={page === 1} onClick={() => onPageChange(page - 1)}>Anterior</Button>
      {pages.map((currentPage) => (
        <Button
          key={currentPage}
          variant={currentPage === page ? 'primary' : 'ghost'}
          size="sm"
          onClick={() => onPageChange(currentPage)}
        >
          {currentPage}
        </Button>
      ))}
      <Button variant="ghost" size="sm" disabled={page === totalPages} onClick={() => onPageChange(page + 1)}>Siguiente</Button>
    </div>
  );
}

export function ConfirmDialog({ open, title, description, confirmLabel = 'Confirmar', cancelLabel = 'Cancelar', onConfirm, onCancel, tone = 'danger' }) {
  if (!open) return null;

  return (
    <div className="modal-backdrop" role="presentation" onClick={onCancel}>
      <div className="modal" role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
        <h3>{title}</h3>
        <p>{description}</p>
        <div className="modal-actions">
          <Button variant="ghost" onClick={onCancel}>{cancelLabel}</Button>
          <Button variant={tone === 'danger' ? 'danger' : 'primary'} onClick={onConfirm}>{confirmLabel}</Button>
        </div>
      </div>
    </div>
  );
}

export function FormField({ label, error, field, value, onChange, options = [], disabled = false }) {
  const inputId = field.name;

  const commonProps = {
    id: inputId,
    name: field.name,
    value: value ?? '',
    onChange: (event) => onChange(field.name, field.type === 'checkbox' ? event.target.checked : event.target.value),
    disabled: disabled || field.disabled || false,
    required: field.required ?? false,
    placeholder: field.placeholder || ''
  };

  return (
    <label className={`form-field form-field-${field.span || 1}`} htmlFor={inputId}>
      <span className="form-field__label">{label}</span>
      {field.type === 'textarea' ? (
        <textarea {...commonProps} rows={field.rows || 4} />
      ) : field.type === 'select' ? (
        <select {...commonProps}>
          <option value="">Selecciona una opción</option>
          {options.map((option) => (
            <option key={String(option.value)} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      ) : field.type === 'checkbox' ? (
        <input
          id={inputId}
          name={field.name}
          type="checkbox"
          checked={Boolean(value)}
          onChange={(event) => onChange(field.name, event.target.checked)}
          disabled={disabled || field.disabled || false}
        />
      ) : (
        <input {...commonProps} type={field.type || 'text'} />
      )}
      {field.helperText ? <small className="form-field__helper">{field.helperText}</small> : null}
      {error ? <small className="form-field__error">{error}</small> : null}
    </label>
  );
}

export function DataTable({ columns, rows, actions = [] }) {
  if (!rows.length) return null;

  return (
    <div className="table-shell">
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((column) => <th key={column.key || column.label}>{column.label}</th>)}
            {actions.length ? <th className="table-actions-col">Acciones</th> : null}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.id ?? row.Id ?? row.code ?? JSON.stringify(row)}>
              {columns.map((column) => (
                <td key={column.key || column.label}>{column.render ? column.render(row) : row[column.key]}</td>
              ))}
              {actions.length ? (
                <td className="row-actions">
                  {actions.map((action) => (
                    <Button key={action.label} variant={action.variant || 'ghost'} size="sm" onClick={() => action.onClick(row)}>
                      {action.label}
                    </Button>
                  ))}
                </td>
              ) : null}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function PageHeader({ title, subtitle, breadcrumbs = [], actions = [] }) {
  return (
    <div className="page-header">
      <div>
        <div className="breadcrumbs">
          {breadcrumbs.map((item, index) => (
            <span key={`${item.label}-${index}`}>
              {index > 0 ? <span className="breadcrumbs__divider">/</span> : null}
              {item.to ? <Link to={item.to}>{item.label}</Link> : <span>{item.label}</span>}
            </span>
          ))}
        </div>
        <h1>{title}</h1>
        {subtitle ? <p>{subtitle}</p> : null}
      </div>
      {actions.length ? <div className="page-header__actions">{actions}</div> : null}
    </div>
  );
}

export function StatGrid({ items }) {
  return (
    <div className="stat-grid">
      {items.map((item) => (
        <Card key={item.label} className="stat-card">
          <div className="stat-card__meta">
            <span className="stat-card__icon">{item.icon}</span>
            <Badge tone={item.tone || 'neutral'}>{item.badge || item.label}</Badge>
          </div>
          <h3>{item.value}</h3>
          <p>{item.label}</p>
        </Card>
      ))}
    </div>
  );
}

export function formatWithFallback(value, fallback = '-') {
  return value === null || value === undefined || value === '' ? fallback : value;
}

export function displayDate(value) {
  return formatDate(value);
}

export function displayDateTime(value) {
  return formatDateTime(value);
}