import api from "./axios";

export const holidayApi = {
  getHolidays: (year) => api.get("/admin/holidays", { params: { year } }),
  createHoliday: (dto) => api.post("/admin/holidays", dto),
  updateHoliday: (id, dto) => api.put(`/admin/holidays/${id}`, dto),
  deleteHoliday: (id) => api.delete(`/admin/holidays/${id}`),
};
