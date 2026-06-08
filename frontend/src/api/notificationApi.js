import api from "./axios";

export const notificationApi = {
  getNotifications() {
    return api.get("/notification");
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
