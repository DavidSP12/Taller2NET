import { request } from './apiClient';
import { normalizeCollectionResponse, normalizeSingleResponse } from '../utils/response';

export const courseService = {
  async list({ page = 1, pageSize = 10, search = '' } = {}) {
    const payload = await request('courses', `?page=${page}&pageSize=${pageSize}&search=${encodeURIComponent(search)}`);
    return normalizeCollectionResponse(payload);
  },
  async getById(id) {
    const payload = await request('courses', `/${id}`);
    return normalizeSingleResponse(payload);
  },
  async create(data) {
    return request('courses', '', { method: 'POST', body: data });
  },
  async update(id, data) {
    return request('courses', `/${id}`, { method: 'PUT', body: data });
  },
  async remove(id) {
    return request('courses', `/${id}`, { method: 'DELETE' });
  }
};