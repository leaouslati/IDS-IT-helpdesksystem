import api from "./axios";

export const aiApi = {
  analyzeTicket(description) {
    return api.post("/ai/analyze-ticket", { description });
  },
};
