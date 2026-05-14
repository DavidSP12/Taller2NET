import { request } from './apiClient';
import { normalizeCollectionResponse, normalizeSingleResponse } from '../utils/response';

export const reportService = {
  async summary() {
    const payload = await request('dashboard', '/summary');
    return normalizeSingleResponse(payload);
  },
  async courses() {
    const payload = await request('dashboard', '/courses');
    return normalizeCollectionResponse(payload);
  },
  async students(page = 1, pageSize = 20) {
    const payload = await request('dashboard', `/students?page=${page}&pageSize=${pageSize}`);
    return normalizeCollectionResponse(payload);
  },
  async topCourses(top = 5) {
    const payload = await request('dashboard', `/top-courses?top=${top}`);
    return normalizeCollectionResponse(payload);
  },
  async attendance(courseId) {
    const query = courseId ? `?courseId=${courseId}` : '';
    const payload = await request('dashboard', `/attendance${query}`);
    return normalizeSingleResponse(payload);
  },
  async grades(courseId) {
    const query = courseId ? `?courseId=${courseId}` : '';
    const payload = await request('dashboard', `/grades${query}`);
    return normalizeSingleResponse(payload);
  },
  async activity(count = 10) {
    const payload = await request('dashboard', `/activity?count=${count}`);
    return normalizeCollectionResponse(payload);
  },
  async programs() {
    const payload = await request('dashboard', '/programs');
    return normalizeCollectionResponse(payload);
  }
};