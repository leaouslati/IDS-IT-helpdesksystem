<template>
  <AppLayout :navLinks="navLinks" :pageTitle="pageTitle">
    <div class="space-y-5">
      <!-- Page header -->
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-[#0F172A] dark:text-white">
            {{ pageTitle }}
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            {{ subtitle }}
          </p>
        </div>
        <button
          v-if="canCreate"
          @click="router.push('/tickets/create')"
          class="inline-flex items-center gap-2 px-4 py-2.5 bg-[#14B8A6] text-white text-sm font-semibold rounded-lg hover:bg-teal-600 active:bg-teal-700 transition-all shadow-sm"
        >
          <Plus :size="15" />
          Create Ticket
        </button>
      </div>

      <!-- Manager tabs -->
      <div
        v-if="isManager"
        class="flex gap-1 bg-gray-100 dark:bg-white/5 p-1 rounded-lg w-fit"
      >
        <button
          v-for="tab in tabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          class="px-4 py-1.5 rounded-md text-sm font-medium transition-all duration-150"
          :class="
            activeTab === tab.key
              ? 'bg-white dark:bg-[#1A1D2E] text-[#0F172A] dark:text-white shadow-sm'
              : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
          "
        >
          {{ tab.label }}
          <span
            v-if="tab.count"
            class="ml-1.5 px-1.5 py-0.5 rounded-full text-[10px] font-bold"
            :class="
              activeTab === tab.key
                ? 'bg-[#14B8A6]/20 text-[#14B8A6]'
                : 'bg-gray-200 dark:bg-white/10 text-gray-500'
            "
          >
            {{ tab.count }}
          </span>
        </button>
      </div>

      <!-- Filters -->
      <TicketFilters
        :show-unassigned="isManager"
        :show-escalated="role !== 'Employee'"
        :statuses="ticketStore.statuses"
        :priorities="ticketStore.priorities"
        :categories="ticketStore.categories"
        @filter-change="onFilterChange"
      />

      <!-- Unassigned tab info banner (manager) -->
      <div
        v-if="isManager && activeTab === 'unassigned'"
        class="flex items-center gap-3 px-4 py-3 bg-amber-50 dark:bg-amber-900/10 border border-amber-200 dark:border-amber-800/40 rounded-xl text-sm text-amber-700 dark:text-amber-400"
      >
        <AlertCircle :size="15" class="flex-shrink-0" />
        Click any ticket to open it and assign an agent from the ticket detail
        page.
      </div>

      <!-- Loading skeleton -->
      <template v-if="ticketStore.loading">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div
            v-for="i in 4"
            :key="i"
            class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] p-4 animate-pulse"
          >
            <div class="flex gap-2 mb-3">
              <div class="h-4 bg-gray-200 dark:bg-white/10 rounded w-20" />
            </div>
            <div class="h-5 bg-gray-200 dark:bg-white/10 rounded w-3/4 mb-3" />
            <div class="flex gap-2 mb-4">
              <div class="h-5 bg-gray-200 dark:bg-white/10 rounded-full w-16" />
              <div class="h-5 bg-gray-200 dark:bg-white/10 rounded-full w-14" />
            </div>
            <div class="h-8 bg-gray-100 dark:bg-white/5 rounded-lg" />
          </div>
        </div>
      </template>

      <!-- Error -->
      <div
        v-else-if="ticketStore.error"
        class="flex flex-col items-center justify-center py-20 gap-4"
      >
        <div
          class="w-14 h-14 rounded-full bg-red-100 dark:bg-red-900/20 flex items-center justify-center"
        >
          <AlertCircle :size="24" class="text-red-500" />
        </div>
        <div class="text-center">
          <h3 class="font-semibold text-[#0F172A] dark:text-white mb-1">
            Failed to load tickets
          </h3>
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
            {{ ticketStore.error }}
          </p>
          <button
            @click="ticketStore.fetchTickets()"
            class="px-4 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors"
          >
            Try Again
          </button>
        </div>
      </div>

      <!-- Empty state -->
      <div
        v-else-if="displayedTickets.length === 0"
        class="flex flex-col items-center justify-center py-20 gap-4"
      >
        <svg
          width="120"
          height="96"
          viewBox="0 0 120 96"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          class="opacity-40"
        >
          <rect
            x="10"
            y="16"
            width="100"
            height="72"
            rx="8"
            fill="currentColor"
            class="text-gray-200 dark:text-gray-700"
          />
          <rect
            x="24"
            y="32"
            width="72"
            height="6"
            rx="3"
            fill="currentColor"
            class="text-gray-300 dark:text-gray-600"
          />
          <rect
            x="24"
            y="46"
            width="48"
            height="6"
            rx="3"
            fill="currentColor"
            class="text-gray-300 dark:text-gray-600"
          />
          <rect
            x="24"
            y="60"
            width="56"
            height="6"
            rx="3"
            fill="currentColor"
            class="text-gray-300 dark:text-gray-600"
          />
          <circle
            cx="95"
            cy="22"
            r="18"
            fill="currentColor"
            class="text-[#14B8A6]/20"
          />
          <path
            d="M88 22 L95 29 L105 17"
            stroke="#14B8A6"
            stroke-width="2.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
        </svg>
        <div class="text-center">
          <h3 class="font-semibold text-[#0F172A] dark:text-white mb-1">
            No tickets found
          </h3>
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
            {{
              canCreate
                ? "Create your first ticket to get started."
                : "No tickets match your current filters."
            }}
          </p>
          <button
            v-if="canCreate"
            @click="router.push('/tickets/create')"
            class="px-4 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors"
          >
            Create New Ticket
          </button>
        </div>
      </div>

      <!-- Ticket grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <TicketCard
          v-for="ticket in displayedTickets"
          :key="ticket.id"
          :ticket="ticket"
        />
      </div>
    </div>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import {
  Plus,
  AlertCircle,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import TicketFilters from "../../components/tickets/TicketFilters.vue";
import TicketCard from "../../components/tickets/TicketCard.vue";
import { useTicketStore } from "../../store/ticket";
import { useAuthStore } from "../../store/auth";
import { useNavLinks } from "../../composables/useNavLinks";

const router = useRouter();
const ticketStore = useTicketStore();
const authStore = useAuthStore();

const activeTab = ref("all");

const localFilters = ref({
  search: "",
  status: "",
  priority: "",
  category: "",
  isEscalated: false,
  isUnassigned: false,
});

const role = computed(() => authStore.userRole);
const isManager = computed(() => role.value === "Manager");
const canCreate = computed(() => ["Employee", "Manager"].includes(role.value));

const pageTitle = computed(() => {
  const map = {
    Employee: "My Tickets",
    Agent: "Assigned Tickets",
    Manager: "Team Tickets",
    Admin: "All Tickets",
  };
  return map[role.value] || "Tickets";
});

const subtitle = computed(() => {
  const map = {
    Employee: "Track all tickets you have submitted",
    Agent: "Tickets currently assigned to you",
    Manager: "Manage and assign department tickets",
    Admin: "View and manage all system tickets",
  };
  return map[role.value] || "";
});

const unassignedTickets = computed(() =>
  ticketStore.tickets.filter(
    (t) => !t.assignedTo || t.assignedTo === "Unassigned"
  )
);

const tabs = computed(() => [
  { key: "all", label: "All Tickets", count: ticketStore.tickets.length },
  {
    key: "unassigned",
    label: "Unassigned",
    count: unassignedTickets.value.length,
  },
]);

const filteredTickets = computed(() => {
  let result = ticketStore.tickets;
  const { search, status, priority, category, isEscalated, isUnassigned } =
    localFilters.value;

  if (search) {
    const q = search.toLowerCase();
    result = result.filter(
      (t) =>
        (t.title ?? "").toLowerCase().includes(q) ||
        (t.referenceNumber ?? "").toLowerCase().includes(q) ||
        (t.category ?? "").toLowerCase().includes(q) ||
        (t.createdBy ?? "").toLowerCase().includes(q)
    );
  }

  if (status) result = result.filter((t) => t.status === status);
  if (priority) result = result.filter((t) => t.priority === priority);
  if (category) result = result.filter((t) => t.category === category);
  if (isEscalated) result = result.filter((t) => t.isEscalated === true);
  if (isUnassigned)
    result = result.filter(
      (t) => !t.assignedTo || t.assignedTo === "Unassigned"
    );

  return result;
});

const displayedTickets = computed(() => {
  if (isManager.value && activeTab.value === "unassigned") {
    return filteredTickets.value.filter(
      (t) => !t.assignedTo || t.assignedTo === "Unassigned"
    );
  }
  return filteredTickets.value;
});

const { navLinks } = useNavLinks();

onMounted(async () => {
  await Promise.all([ticketStore.fetchTickets(), ticketStore.fetchLookups()]);
});

function onFilterChange(filters) {
  localFilters.value = { ...filters };
}
</script>
