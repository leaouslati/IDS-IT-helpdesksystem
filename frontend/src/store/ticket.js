import { defineStore } from "pinia";
import { ticketApi } from "../api/ticketApi";
import { lookupApi } from "../api/lookupApi";

const STATIC_STATUSES = [
  { id: 1, name: "Open" },
  { id: 2, name: "In Progress" },
  { id: 4, name: "Resolved" },
];

export const useTicketStore = defineStore("ticket", {
  state: () => ({
    tickets: [],
    currentTicket: null,
    categories: [],
    priorities: [],
    statuses: [],
    loading: false,
    error: null,
    filters: {
      search: "",
      status: "",
      priority: "",
      category: "",
      isEscalated: null,
      isUnassigned: null,
      sortBy: "createdAt",
      sortOrder: "desc",
    },
  }),

  actions: {
    async fetchTickets() {
      this.loading = true;
      this.error = null;
      try {
        const response = await ticketApi.getTickets();
        this.tickets = response.data;
      } catch {
        this.error = "Failed to load tickets";
      } finally {
        this.loading = false;
      }
    },

    async fetchTicket(id) {
      this.loading = true;
      this.error = null;
      try {
        const response = await ticketApi.getTicket(id);
        this.currentTicket = response.data;
      } catch {
        this.error = "Failed to load ticket";
      } finally {
        this.loading = false;
      }
    },

    async createTicket(data) {
      const response = await ticketApi.createTicket(data);
      return response.data;
    },

    async updateTicket(id, data) {
      await ticketApi.updateTicket(id, data);
      await this.fetchTicket(id);
    },

    async deleteTicket(id) {
      await ticketApi.deleteTicket(id);
      this.tickets = this.tickets.filter((t) => t.id !== id);
    },

    async fetchLookups() {
      try {
        const [cats, pris, stats] = await Promise.all([
          lookupApi.getCategories(),
          lookupApi.getPriorities(),
          lookupApi.getStatuses(),
        ]);
        this.categories = cats.data ?? [];
        this.priorities = pris.data ?? [];
        this.statuses = stats.data?.length ? stats.data : STATIC_STATUSES;
      } catch {
        this.statuses = STATIC_STATUSES;
      }
    },

    setFilter(key, value) {
      this.filters[key] = value;
    },

    setFilters(newFilters) {
      Object.assign(this.filters, newFilters);
    },

    resetFilters() {
      this.filters = {
        search: "",
        status: "",
        priority: "",
        category: "",
        isEscalated: null,
        isUnassigned: null,
        sortBy: "createdAt",
        sortOrder: "desc",
      };
    },
  },
});
