import { createMonolithProvider } from './monolithProvider';
import { createServiceProvider } from './serviceProvider';
import { createMicroservicesProvider } from './microservicesProvider';

export function getBackendProvider() {
  const mode = (import.meta.env.VITE_BACKEND_PROVIDER || 'service').toLowerCase();

  if (mode === 'monolith') return createMonolithProvider();
  if (mode === 'microservices') return createMicroservicesProvider();
  return createServiceProvider();
}