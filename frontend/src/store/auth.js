import { defineStore } from "pinia";
import api from "../api/axios";
import { useNotificationStore } from "./notification";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    token: localStorage.getItem("token") || null,
    user: JSON.parse(localStorage.getItem("user")) || null,
    avatarUrl: null,
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
      this.clearAvatar();
    },

    // Keeps navbar/sidebar in sync immediately after a profile edit, without a re-login
    updateUserInfo({ firstName, lastName, email }) {
      this.user = { ...this.user, firstName, lastName, email };
      localStorage.setItem("user", JSON.stringify(this.user));
    },

    setAvatar(objectUrl) {
      if (this.avatarUrl) URL.revokeObjectURL(this.avatarUrl);
      this.avatarUrl = objectUrl;
    },

    clearAvatar() {
      if (this.avatarUrl) URL.revokeObjectURL(this.avatarUrl);
      this.avatarUrl = null;
    },
  },
});
