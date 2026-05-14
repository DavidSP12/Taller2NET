import { request } from './apiClient';
import { normalizeCollectionResponse, normalizeSingleResponse } from '../utils/response';

export const attendanceService = {
  async list() {
    const payload = await request('attendance');
    return normalizeCollectionResponse(payload);
  },
  async getById(id) {
    const payload = await request('attendance', `/${id}`);
    return normalizeSingleResponse(payload);
  },
  async create(data) {
    return request('attendance', '', { method: 'POST', body: data });
  },
  async update(id, data) {
    return request('attendance', `/${id}`, { method: 'PUT', body: data });
  },
  async remove(id) {
    return request('attendance', `/${id}`, { method: 'DELETE' });
  },
  async byEnrollment(enrollmentId) {
    const payload = await request('attendance', `/enrollment/${enrollmentId}`);
    return normalizeCollectionResponse(payload);
  }
};