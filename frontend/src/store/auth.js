import { defineStore } from "pinia";
import api from "../api/axios";
import { useNotificationStore } from "./notification";

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

      localStorage.setItem("token", data.token);
      localStorage.setItem("user", JSON.stringify(data));

      this.token = data.token;
      this.user = data;

      useNotificationStore().connectToHub();

      return data.role;
    },

    logout() {
      useNotificationStore().disconnect();

      localStorage.removeItem("token");
      localStorage.removeItem("user");
      this.token = null;
      this.user = null;
    },
  },
});
