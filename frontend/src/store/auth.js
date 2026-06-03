import { defineStore } from "pinia";
import api from "../api/axios";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    token: localStorage.getItem("token") || null,
    user: JSON.parse(localStorage.getItem("user")) || null,
  }),

  getters: {
    isLoggedIn: (state) => !!state.token,
    userRole: (state) => state.user?.role || null,
    userName: (state) => state.user?.firstName || null,
  },

  actions: {
    async login(email, password) {
      const response = await api.post("/auth/login", { email, password });
      const data = response.data;

      // Save token and user to localStorage
      localStorage.setItem("token", data.token);
      localStorage.setItem("user", JSON.stringify(data));

      // Save to store state
      this.token = data.token;
      this.user = data;

      return data.role;
    },

    logout() {
      // Clear everything
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      this.token = null;
      this.user = null;
    },
  },
});
