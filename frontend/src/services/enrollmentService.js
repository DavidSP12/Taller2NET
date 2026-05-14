import { request } from './apiClient';
import { normalizeCollectionResponse, normalizeSingleResponse } from '../utils/response';

export const enrollmentService = {
  async list() {
    const payload = await request('enrollments');
    return normalizeCollectionResponse(payload);
  },
  async getById(id) {
    const payload = await request('enrollments', `/${id}`);
    return normalizeSingleResponse(payload);
  },
  async create(data) {
    return request('enrollments', '', { method: 'POST', body: data });
  },
  async update(id, data) {
    return request('enrollments', `/${id}`, { method: 'PUT', body: data });
  },
  async remove(id) {
    return request('enrollments', `/${id}`, { method: 'DELETE' });
  },
  async withdraw(id) {
    return request('enrollments', `/${id}/withdraw`, { method: 'PUT' });
  },
  async byStudent(studentId) {
    const payload = await request('enrollments', `/student/${studentId}`);
    return normalizeCollectionResponse(payload);
  },
  async byCourse(courseId) {
    const payload = await request('enrollments', `/course/${courseId}`);
    return normalizeCollectionResponse(payload);
  }
};