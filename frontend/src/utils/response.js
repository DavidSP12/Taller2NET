export function normalizeCollectionResponse(payload) {
  if (!payload) return { items: [], totalCount: 0, page: 1, pageSize: 10 };

  if (Array.isArray(payload.data)) {
    return {
      items: payload.data,
      totalCount: payload.totalCount ?? payload.data.length,
      page: payload.page ?? 1,
      pageSize: payload.pageSize ?? (payload.data.length || 10)
    };
  }

  if (Array.isArray(payload.data?.items)) {
    return {
      items: payload.data.items,
      totalCount: payload.data.totalCount ?? payload.data.items.length,
      page: payload.data.page ?? 1,
      pageSize: payload.data.pageSize ?? (payload.data.items.length || 10)
    };
  }

  return { items: [], totalCount: 0, page: 1, pageSize: 10 };
}

export function normalizeSingleResponse(payload) {
  if (!payload) return null;
  return payload.data ?? payload;
}