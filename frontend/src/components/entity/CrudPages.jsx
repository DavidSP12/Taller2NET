import { Link, useNavigate, useParams } from 'react-router-dom';
import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { Alert, Badge, Button, Card, ConfirmDialog, DataTable, EmptyState, FormField, Loader, Pagination, PageHeader } from '../common/UI';
import { formatDate, formatDateTime, formatNumber, toInputDate, toInputDateTime } from '../../utils/formatters';

function getFields(config, mode) {
  if (typeof config.getFormFields === 'function') return config.getFormFields(mode);
  if (mode === 'edit' && Array.isArray(config.editFields)) return config.editFields;
  if (mode === 'create' && Array.isArray(config.createFields)) return config.createFields;
  return config.formFields;
}

function buildInitialValues(fields, item) {
  return fields.reduce((accumulator, field) => {
    const raw = item?.[field.name];
    if (field.type === 'date') {
      accumulator[field.name] = toInputDate(raw ?? field.defaultValue ?? '');
    } else if (field.type === 'datetime-local') {
      accumulator[field.name] = toInputDateTime(raw ?? field.defaultValue ?? '');
    } else if (field.type === 'checkbox') {
      accumulator[field.name] = Boolean(raw ?? field.defaultValue ?? false);
    } else {
      accumulator[field.name] = raw ?? field.defaultValue ?? '';
    }
    return accumulator;
  }, {});
}

function preparePayload(fields, values) {
  const payload = {};
  for (const field of fields) {
    const value = values[field.name];
    if (field.hidden) continue;
    if (field.type === 'number') {
      payload[field.name] = value === '' || value === null || value === undefined ? null : Number(value);
    } else if (field.type === 'select' && (field.valueType === 'number' || field.name.toLowerCase().endsWith('id'))) {
      payload[field.name] = value === '' || value === null || value === undefined ? null : Number(value);
    } else if (field.type === 'checkbox') {
      payload[field.name] = Boolean(value);
    } else if (field.type === 'date' || field.type === 'datetime-local') {
      payload[field.name] = value ? new Date(value).toISOString() : null;
    } else {
      payload[field.name] = value;
    }
  }
  return payload;
}

function resolveFieldOptions(field, options) {
  if (typeof field.options === 'function') return field.options(options);
  if (field.optionsKey) return options?.[field.optionsKey] || [];
  return field.options || [];
}

export function EntityListPage({ config }) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [items, setItems] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(config.pageSize || 10);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  async function loadData(nextPage = page, nextSearch = search) {
    setLoading(true);
    setError('');
    try {
      const response = await config.service.list({ page: nextPage, pageSize, search: nextSearch });
      setItems(response.items || []);
      setTotalCount(response.totalCount || 0);
      setPage(response.page || nextPage);
    } catch (loadError) {
      setError(loadError.message || 'No fue posible cargar los datos');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadData(1, '');
  }, []);

  const canWrite = config.writeRoles?.includes(user?.role);
  const columns = useMemo(() => config.columns, [config.columns]);

  async function handleDelete() {
    if (!deleteTarget) return;
    try {
      await config.service.remove(deleteTarget.id);
      setDeleteTarget(null);
      await loadData(page, search);
    } catch (deleteError) {
      setError(deleteError.message || 'No fue posible eliminar el registro');
    }
  }

  const actions = [
    { label: 'Ver', onClick: (row) => navigate(`${config.routeBase}/${row.id}`) },
    ...(canWrite ? [{ label: 'Editar', variant: 'secondary', onClick: (row) => navigate(`${config.routeBase}/${row.id}/edit`) }] : []),
    ...(config.allowDelete && canWrite ? [{ label: 'Eliminar', variant: 'danger', onClick: (row) => setDeleteTarget(row) }] : [])
  ];

  return (
    <div className="page-stack">
      <PageHeader
        title={config.title}
        subtitle={config.subtitle}
        breadcrumbs={[{ label: 'Inicio', to: '/dashboard' }, { label: config.title }]}
        actions={canWrite && config.allowCreate ? [<Button key="create" onClick={() => navigate(`${config.routeBase}/new`)}>Nuevo {config.singular}</Button>] : []}
      />

      <Card className="filters-card">
        <div className="filters-row">
          <label className="search-field">
            <span>Buscar</span>
            <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder={`Buscar ${config.title.toLowerCase()}`} />
          </label>
          <div className="filters-actions">
            <Button variant="secondary" onClick={() => loadData(1, search)}>Filtrar</Button>
            <Button variant="ghost" onClick={() => { setSearch(''); loadData(1, ''); }}>Limpiar</Button>
          </div>
        </div>
      </Card>

      {error ? <Alert>{error}</Alert> : null}

      {loading ? <Loader /> : items.length ? (
        <>
          <DataTable columns={columns} rows={items} actions={actions} />
          <Pagination page={page} totalCount={totalCount} pageSize={pageSize} onPageChange={(nextPage) => loadData(nextPage, search)} />
        </>
      ) : (
        <EmptyState
          title={`No hay ${config.title.toLowerCase()}`}
          description={`Aún no se registran ${config.title.toLowerCase()} en el sistema.`}
          action={canWrite && config.allowCreate ? <Button onClick={() => navigate(`${config.routeBase}/new`)}>Crear {config.singular}</Button> : null}
        />
      )}

      <ConfirmDialog
        open={Boolean(deleteTarget)}
        title={`Eliminar ${config.singular}`}
        description={`Esta acción desactivará el registro de ${deleteTarget?.name || deleteTarget?.title || deleteTarget?.code || config.singular.toLowerCase()}.`}
        confirmLabel="Eliminar"
        onCancel={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
      />
    </div>
  );
}

export function EntityDetailPage({ config }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [item, setItem] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const canWrite = config.writeRoles?.includes(user?.role);

  useEffect(() => {
    let active = true;
    setLoading(true);
    config.service.getById(id)
      .then((response) => {
        if (!active) return;
        setItem(response);
      })
      .catch((detailError) => {
        if (!active) return;
        setError(detailError.message || 'No fue posible obtener el detalle');
      })
      .finally(() => active && setLoading(false));

    return () => {
      active = false;
    };
  }, [id, config]);

  const fields = useMemo(() => config.detailFields, [config.detailFields]);

  if (loading) return <Loader />;
  if (error) return <Alert>{error}</Alert>;
  if (!item) return <EmptyState title="Registro no encontrado" description="El elemento solicitado no existe o fue eliminado." />;

  return (
    <div className="page-stack">
      <PageHeader
        title={`${config.singular} #${item.id || item.Id}`}
        subtitle={config.subtitle}
        breadcrumbs={[{ label: 'Inicio', to: '/dashboard' }, { label: config.title, to: config.routeBase }, { label: config.singular }]}
        actions={[
          <Button key="back" variant="ghost" onClick={() => navigate(config.routeBase)}>Volver</Button>,
          ...(canWrite ? [<Button key="edit" onClick={() => navigate(`${config.routeBase}/${id}/edit`)}>Editar</Button>] : [])
        ]}
      />

      <Card className="detail-grid">
        {fields.map((field) => (
          <div key={field.name} className="detail-field">
            <span>{field.label}</span>
            <strong>{field.render ? field.render(item) : formatDetailValue(field, item[field.name])}</strong>
          </div>
        ))}
      </Card>
    </div>
  );
}

function formatDetailValue(field, value) {
  if (value === null || value === undefined || value === '') return '-';
  if (field.type === 'date') return formatDate(value);
  if (field.type === 'datetime-local') return formatDateTime(value);
  if (field.type === 'number') return formatNumber(value, field.precision ?? 0);
  if (field.type === 'boolean') return value ? 'Sí' : 'No';
  if (field.badge) return <Badge tone={field.badge(value)}>{String(value)}</Badge>;
  return String(value);
}

export function EntityFormPage({ config, mode }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(mode === 'edit');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [item, setItem] = useState(null);
  const [options, setOptions] = useState({});
  const fields = getFields(config, mode);
  const [formValues, setFormValues] = useState(() => buildInitialValues(fields, null));
  const [fieldErrors, setFieldErrors] = useState({});

  useEffect(() => {
    let active = true;

    async function load() {
      try {
        const loadedOptions = config.loadOptions ? await config.loadOptions(mode) : {};
        if (!active) return;
        setOptions(loadedOptions || {});

        if (mode === 'edit') {
          const response = await config.service.getById(id);
          if (!active) return;
          setItem(response);
          setFormValues(buildInitialValues(fields, response));
        }
      } catch (loadError) {
        if (active) setError(loadError.message || 'No fue posible preparar el formulario');
      } finally {
        if (active) setLoading(false);
      }
    }

    load();
    return () => { active = false; };
  }, [id, mode, config]);

  function handleChange(name, value) {
    setFormValues((current) => ({ ...current, [name]: value }));
  }

  function validate(values) {
    const nextErrors = {};
    for (const field of fields) {
      if (field.required && (values[field.name] === '' || values[field.name] === null || values[field.name] === undefined)) {
        nextErrors[field.name] = 'Este campo es obligatorio';
      }
    }
    setFieldErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    if (!validate(formValues)) return;

    setSaving(true);
    setError('');
    try {
      const payload = config.preparePayload
        ? config.preparePayload(formValues, options, mode)
        : preparePayload(fields, formValues);

      if (mode === 'edit') {
        await config.service.update(id, payload);
      } else {
        await config.service.create(payload);
      }

      navigate(config.routeBase);
    } catch (submitError) {
      setError(submitError.message || 'No fue posible guardar el registro');
    } finally {
      setSaving(false);
    }
  }

  const title = mode === 'edit' ? `Editar ${config.singular}` : `Nuevo ${config.singular}`;

  if (loading) return <Loader />;

  return (
    <div className="page-stack">
      <PageHeader
        title={title}
        subtitle={config.subtitle}
        breadcrumbs={[{ label: 'Inicio', to: '/dashboard' }, { label: config.title, to: config.routeBase }, { label: mode === 'edit' ? 'Editar' : 'Nuevo' }]}
        actions={[<Button key="cancel" variant="ghost" onClick={() => navigate(config.routeBase)}>Cancelar</Button>]}
      />

      {error ? <Alert>{error}</Alert> : null}

      <Card>
        <form className="form-grid" onSubmit={handleSubmit}>
          {fields.map((field) => (
            <FormField
              key={field.name}
              label={field.label}
              error={fieldErrors[field.name]}
                field={{ ...field, disabled: field.disabled || (mode === 'edit' && field.readOnlyOnEdit) }}
              value={formValues[field.name]}
              onChange={handleChange}
              options={resolveFieldOptions(field, options)}
            />
          ))}
          <div className="form-actions">
            <Button type="submit" disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button>
          </div>
        </form>
      </Card>
    </div>
  );
}