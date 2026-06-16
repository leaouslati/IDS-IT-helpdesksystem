import api from "./axios";

export const ticketApi = {
  getTickets(filters = {}) {
    return api.get("/ticket", { params: filters });
  },
  getTicket(id) {
    return api.get(`/ticket/${id}`);
  },
  createTicket(data) {
    return api.post("/ticket", data);
  },
  updateTicket(id, data) {
    return api.put(`/ticket/${id}`, data);
  },
  deleteTicket(id) {
    return api.delete(`/ticket/${id}`);
  },
  updateStatus(id, statusName) {
    return api.put(`/ticket/${id}/status`, { statusName });
  },
  assignTicket(id, agentUserId) {
    return api.put(`/ticket/${id}/assign`, { agentUserId });
  },
  escalateTicket(id, reason) {
    return api.put(`/ticket/${id}/escalate`, { reason });
  },
  addComment(id, content, isInternal = false) {
    return api.post(`/ticket/${id}/comment`, { content, isInternal });
  },
  getComments(id) {
    return api.get(`/ticket/${id}/comments`);
  },
  uploadAttachment(id, file, commentId = null) {
    const formData = new FormData();
    formData.append("file", file);
    if (commentId) formData.append("commentId", commentId);
    return api.post(`/ticket/${id}/attachments`, formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
  getAgentsAvailability(ticketId) {
    return api.get(`/ticket/${ticketId}/agents-availability`);
  },
  logHours(ticketId, data) {
    return api.post(`/ticket/${ticketId}/hours`, data);
  },
};
