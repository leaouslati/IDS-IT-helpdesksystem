import api from "./axios";

export const notificationApi = {
  getNotifications(page = 1, pageSize = 20) {
    return api.get("/notification", { params: { page, pageSize } });
  },
  markAsRead(id) {
    return api.put(`/notification/${id}/read`);
  },
  markAllAsRead() {
    return api.put("/notification/read-all");
  },
  getUnreadCount() {
    return api.get("/notification/unread-count");
  },
};
