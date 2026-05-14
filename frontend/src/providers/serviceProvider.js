import { createMonolithProvider } from './monolithProvider';

export function createServiceProvider(env = import.meta.env) {
  return {
    ...createMonolithProvider(env),
    name: 'service-based'
  };
}