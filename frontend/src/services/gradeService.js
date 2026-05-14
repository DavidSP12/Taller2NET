import { request } from './apiClient';
import { normalizeCollectionResponse, normalizeSingleResponse } from '../utils/response';

export const gradeService = {
  async list() {
    const payload = await request('grades');
    return normalizeCollectionResponse(payload);
  },
  async getById(id) {
    const payload = await request('grades', `/${id}`);
    return normalizeSingleResponse(payload);
  },
  async create(data) {
    return request('grades', '', { method: 'POST', body: data });
  },
  async update(id, data) {
    return request('grades', `/${id}`, { method: 'PUT', body: data });
  },
  async remove(id) {
    return request('grades', `/${id}`, { method: 'DELETE' });
  },
  async byEnrollment(enrollmentId) {
    const payload = await request('grades', `/enrollment/${enrollmentId}`);
    return normalizeCollectionResponse(payload);
  }
};