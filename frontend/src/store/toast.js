import { defineStore } from "pinia";

export const useToastStore = defineStore("toast", {
  state: () => ({ toasts: [] }),
  actions: {
    show(message, type = "success") {
      const id = Date.now();
      this.toasts.push({ id, message, type });
      setTimeout(() => this.remove(id), 4000);
    },
    remove(id) {
      this.toasts = this.toasts.filter((t) => t.id !== id);
    },
  },
});
