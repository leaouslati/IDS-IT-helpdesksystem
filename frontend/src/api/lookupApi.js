import api from "./axios";

export const lookupApi = {
  getCategories() {
    return api.get("/lookup/categories");
  },
  getPriorities() {
    return api.get("/lookup/priorities");
  },
  getStatuses() {
    return api.get("/lookup/statuses");
  },
};
