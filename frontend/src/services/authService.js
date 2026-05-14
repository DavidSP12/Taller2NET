import { request } from './apiClient';

export const authService = {
  async login(credentials) {
    const payload = await request('auth', '/login', {
      method: 'POST',
      body: credentials,
      token: ''
    });

    return payload?.data || payload?.Data;
  },

  async getProfile() {
    return request('auth', '/me');
  }
};