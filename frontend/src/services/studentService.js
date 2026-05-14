import { request } from './apiClient';
import { normalizeCollectionResponse, normalizeSingleResponse } from '../utils/response';

export const studentService = {
  async list({ page = 1, pageSize = 10, search = '' } = {}) {
    const payload = await request('students', `?page=${page}&pageSize=${pageSize}&search=${encodeURIComponent(search)}`);
    return normalizeCollectionResponse(payload);
  },
  async getById(id) {
    const payload = await request('students', `/${id}`);
    return normalizeSingleResponse(payload);
  },
  async create(data) {
    return request('students', '', { method: 'POST', body: data });
  },
  async update(id, data) {
    return request('students', `/${id}`, { method: 'PUT', body: data });
  },
  async remove(id) {
    return request('students', `/${id}`, { method: 'DELETE' });
  }
};