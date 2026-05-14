export function formatDate(value) {
  if (!value) return '-';
  return new Date(value).toLocaleDateString('es-CO', {
    year: 'numeric',
    month: 'short',
    day: '2-digit'
  });
}

export function formatDateTime(value) {
  if (!value) return '-';
  return new Date(value).toLocaleString('es-CO');
}

export function formatNumber(value, digits = 2) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return '-';
  return Number(value).toFixed(digits);
}

export function toInputDate(value) {
  if (!value) return '';
  return new Date(value).toISOString().slice(0, 10);
}

export function toInputDateTime(value) {
  if (!value) return '';
  return new Date(value).toISOString().slice(0, 16);
}