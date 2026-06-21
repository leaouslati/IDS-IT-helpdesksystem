import api from "./axios";

export const adminApi = {
  getUsers: () => api.get("/admin/users"),
  createUser: (dto) => api.post("/admin/users", dto),
  updateUserRole: (id, dto) => api.put(`/admin/users/${id}/role`, dto),
  toggleActive: (id) => api.put(`/admin/users/${id}/toggle-active`),
  deleteUser: (id) => api.delete(`/admin/users/${id}`),
  getRoles: () => api.get("/admin/roles"),
  getDepartments: () => api.get("/admin/departments"),
};
