<template>
  <AppLayout
    :navLinks="navLinks"
    :pageTitle="`Welcome back, ${authStore.user?.firstName || 'Manager'}`"
    :showCreateTicket="false"
  >
    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-[60vh]">
      <div class="flex flex-col items-center gap-4">
        <div
          class="w-12 h-12 border-4 border-[#14B8A6] border-t-transparent rounded-full animate-spin"
        />
        <p class="text-sm text-gray-400">Loading dashboard...</p>
      </div>
    </div>

    <!-- Error -->
    <div
      v-else-if="error"
      class="flex items-center justify-center min-h-[60vh]"
    >
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl p-10 text-center max-w-sm shadow-sm border border-gray-100 dark:border-white/[0.05]"
      >
        <AlertCircle :size="44" class="text-red-500 mx-auto mb-3" />
        <h3 class="font-semibold text-[#0F172A] dark:text-white mb-1 text-base">
          Failed to load
        </h3>
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">{{ error }}</p>
        <button
          @click="fetchData"
          class="px-5 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors"
        >
          Try Again
        </button>
      </div>
    </div>

    <!-- Content -->
    <template v-else>
      <!-- Header -->
      <div class="flex flex-wrap items-start justify-between gap-4 mb-6">
        <div>
          <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">
            Dashboard
          </h1>
          <p class="text-sm text-gray-400 mt-0.5">
            {{ departmentLabel }}
          </p>
        </div>
        <div class="flex items-center gap-2">
          <!-- Days selector -->
          <div class="flex items-center gap-1 bg-gray-100 dark:bg-white/5 rounded-lg p-1">
            <button
              v-for="d in daysOptions"
              :key="d"
              @click="days = d"
              class="px-3 py-1.5 rounded-md text-xs font-semibold transition-colors"
              :class="
                days === d
                  ? 'bg-white dark:bg-white/10 text-[#0F172A] dark:text-white shadow-sm'
                  : 'text-gray-400 hover:text-gray-600 dark:hover:text-gray-300'
              "
            >
              {{ d }}d
            </button>
          </div>
          <button
            @click="router.push('/tickets/create')"
            class="flex-shrink-0 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 transition-colors flex items-center gap-2"
          >
            <Plus :size="14" />
            New Ticket
          </button>
        </div>
      </div>

      <!-- Stat cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
        <StatCard
          title="Unassigned Tickets"
          :value="data?.unassignedTickets ?? 0"
          color="#F59E0B"
          :icon="Inbox"
          :badge="(data?.unassignedTickets ?? 0) > 0 ? 'NEEDS ACTION' : ''"
          badgeVariant="yellow"
        />
        <StatCard
          title="Resolved This Week"
          :value="data?.resolvedThisWeek ?? 0"
          color="#10B981"
          :icon="CheckCircle2"
        />
        <StatCard
          title="Avg Resolution Time"
          :value="formatResolutionTime(data?.avgResolutionHours)"
          color="#14B8A6"
          :icon="Clock"
        />
        <StatCard
          title="Escalated"
          :value="data?.escalatedTickets ?? 0"
          color="#EF4444"
          :icon="TrendingUp"
          :badge="(data?.escalatedTickets ?? 0) > 0 ? 'NEEDS REVIEW' : ''"
          badgeVariant="red"
        />
      </div>

      <!-- Unassigned Tickets Section -->
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] overflow-hidden mb-6"
      >
        <div
          class="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-white/[0.06]"
        >
          <div>
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
              Unassigned Tickets
            </h3>
            <p class="text-xs text-gray-400 mt-0.5">
              Click a ticket or "Assign" to open and assign an agent
            </p>
          </div>
          <div class="flex items-center gap-3">
            <span
              v-if="(data?.unassignedTicketsList?.length ?? 0) > 0"
              class="text-xs font-semibold text-amber-600 dark:text-amber-400"
            >
              {{ data.unassignedTicketsList.length }} pending
            </span>
            <button
              @click="router.push('/tickets?filter=unassigned')"
              class="text-xs font-semibold text-[#14B8A6] hover:text-teal-600 transition-colors"
            >
              View all →
            </button>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="bg-gray-50 dark:bg-white/[0.03]">
                <th
                  v-for="col in unassignedColumns"
                  :key="col.key"
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider whitespace-nowrap"
                >
                  {{ col.label }}
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-white/[0.04]">
              <tr
                v-for="ticket in data?.unassignedTicketsList ?? []"
                :key="ticket.id"
                class="hover:bg-gray-50 dark:hover:bg-white/[0.03] transition-colors duration-150 cursor-pointer"
                @click="router.push(`/tickets/${ticket.id}`)"
              >
                <td
                  class="px-5 py-3.5 whitespace-nowrap text-[13px] text-[#0F172A] dark:text-gray-300"
                >
                  {{ ticket.referenceNumber }}
                </td>
                <td
                  class="px-5 py-3.5 text-[13px] text-[#0F172A] dark:text-gray-300 max-w-[220px] truncate"
                >
                  {{ ticket.title }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span :class="priorityClass(ticket.priority)">{{
                    ticket.priority
                  }}</span>
                </td>
                <td
                  class="px-5 py-3.5 whitespace-nowrap text-[13px] text-gray-500 dark:text-gray-400"
                >
                  {{ ticket.createdBy }}
                </td>
                <td
                  class="px-5 py-3.5 whitespace-nowrap text-[13px] text-gray-500 dark:text-gray-400"
                >
                  {{ formatDate(ticket.createdAt) }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <button
                    @click.stop="router.push(`/tickets/${ticket.id}`)"
                    class="px-3 py-1.5 rounded-lg text-xs font-semibold bg-[#14B8A6] text-white hover:bg-teal-600 active:bg-teal-700 transition-colors"
                  >
                    Assign
                  </button>
                </td>
              </tr>
              <tr v-if="!data?.unassignedTicketsList?.length">
                <td
                  colspan="6"
                  class="text-center py-12 text-gray-400 dark:text-gray-500 text-sm"
                >
                  No unassigned tickets — all clear!
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Escalated Tickets — Needs Reassignment -->
      <div
        v-if="(data?.escalatedTicketsList?.length ?? 0) > 0"
        class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-orange-200 dark:border-orange-900/40 overflow-hidden mb-6"
      >
        <div
          class="flex items-center justify-between px-5 py-4 border-b border-orange-100 dark:border-orange-900/30 bg-orange-50/60 dark:bg-orange-900/10"
        >
          <div class="flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-orange-100 dark:bg-orange-900/30 flex items-center justify-center"
            >
              <Flame :size="15" class="text-orange-500" />
            </div>
            <div>
              <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
                Escalated — Needs Reassignment
              </h3>
              <p class="text-xs text-orange-500 dark:text-orange-400 mt-0.5">
                {{ data.escalatedTicketsList.length }} ticket{{
                  data.escalatedTicketsList.length !== 1 ? "s" : ""
                }}
                escalated and awaiting a new agent
              </p>
            </div>
          </div>
          <button
            @click="router.push('/tickets?filter=escalated')"
            class="text-xs font-semibold text-orange-500 hover:text-orange-600 dark:text-orange-400 transition-colors"
          >
            View all →
          </button>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="bg-gray-50 dark:bg-white/[0.03]">
                <th
                  v-for="col in escalatedColumns"
                  :key="col.key"
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider whitespace-nowrap"
                >
                  {{ col.label }}
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-white/[0.04]">
              <tr
                v-for="ticket in data.escalatedTicketsList"
                :key="ticket.id"
                class="hover:bg-orange-50/40 dark:hover:bg-orange-900/5 transition-colors cursor-pointer"
                @click="router.push(`/tickets/${ticket.id}`)"
              >
                <td
                  class="px-5 py-3.5 whitespace-nowrap text-[13px] font-mono text-gray-500 dark:text-gray-400"
                >
                  {{ ticket.referenceNumber }}
                </td>
                <td
                  class="px-5 py-3.5 text-[13px] text-[#0F172A] dark:text-gray-300 max-w-[220px] truncate"
                >
                  {{ ticket.title }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span :class="priorityClass(ticket.priority)">{{
                    ticket.priority
                  }}</span>
                </td>
                <td
                  class="px-5 py-3.5 whitespace-nowrap text-[13px] text-gray-500 dark:text-gray-400"
                >
                  {{ ticket.createdBy }}
                </td>
                <td
                  class="px-5 py-3.5 whitespace-nowrap text-[13px] text-gray-500 dark:text-gray-400"
                >
                  {{ formatDate(ticket.createdAt) }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <button
                    @click.stop="router.push(`/tickets/${ticket.id}`)"
                    class="px-3 py-1.5 rounded-lg text-xs font-semibold bg-orange-500 text-white hover:bg-orange-600 transition-colors"
                  >
                    Reassign
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Agent Availability + Performance row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
        <!-- Agent Availability -->
        <div
          class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5"
        >
          <div class="mb-4">
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
              Agent Availability
            </h3>
            <p class="text-xs text-gray-400 mt-0.5">
              Current workload per agent
            </p>
          </div>
          <div v-if="data?.agentAvailability?.length" class="space-y-3">
            <div
              v-for="agent in data.agentAvailability"
              :key="agent.userId"
              class="flex items-center justify-between gap-3 p-3 rounded-xl border border-gray-100 dark:border-white/[0.06] bg-gray-50/50 dark:bg-white/[0.02]"
            >
              <div class="flex items-center gap-3 min-w-0">
                <div
                  class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 text-xs font-bold"
                  :class="
                    agent.isAvailable
                      ? 'bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400'
                      : 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
                  "
                >
                  {{ agentInitials(agent.agentName) }}
                </div>
                <div class="min-w-0">
                  <p
                    class="text-[13px] font-semibold text-[#0F172A] dark:text-white leading-none truncate"
                  >
                    {{ agent.agentName }}
                  </p>
                  <p class="text-xs text-gray-400 mt-0.5">
                    {{ agent.openTickets }} open ·
                    {{ agent.resolvedThisWeek }} resolved this week
                  </p>
                </div>
              </div>
              <span
                :class="
                  agent.isAvailable
                    ? 'text-green-600 dark:text-green-400'
                    : 'text-red-500 dark:text-red-400'
                "
                class="text-[12px] font-semibold flex-shrink-0"
              >
                {{ agent.isAvailable ? "Available" : "Busy" }}
              </span>
            </div>
          </div>
          <div
            v-else
            class="flex items-center justify-center h-[140px] text-gray-400 dark:text-gray-500 text-sm"
          >
            No agents in your department
          </div>
        </div>

        <!-- Agent Performance chart -->
        <div
          class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5"
        >
          <div class="flex items-center justify-between mb-5">
            <div>
              <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
                Agent Performance
              </h3>
              <p class="text-xs text-gray-400 mt-0.5">
                Tickets resolved per agent
              </p>
            </div>
            <span
              class="text-xs text-gray-400 bg-gray-100 dark:bg-white/5 px-2.5 py-1 rounded-full"
            >
              All time
            </span>
          </div>
          <div
            v-if="data?.agentPerformance?.length"
            style="position: relative; height: 220px"
          >
            <Bar :data="barChartData" :options="barChartOptions" />
          </div>
          <div
            v-else
            class="flex items-center justify-center h-[220px] text-gray-400 dark:text-gray-500 text-sm"
          >
            No performance data available
          </div>
        </div>
      </div>

      <!-- Team tickets table -->
      <TicketTable
        title="Department Recent Tickets"
        :tickets="processedTickets"
        :columns="ticketColumns"
        @row-click="(t) => router.push(`/tickets/${t.id}`)"
        @action-click="(t) => router.push(`/tickets/${t.id}`)"
      />
    </template>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { Bar } from "vue-chartjs";
import {
  LayoutDashboard,
  FileText,
  BarChart2,
  Bell,
  User,
  Inbox,
  Clock,
  CheckCircle2,
  TrendingUp,
  AlertCircle,
  Plus,
  Flame,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import StatCard from "../../components/dashboard/StatCard.vue";
import TicketTable from "../../components/dashboard/TicketTable.vue";
import { useAuthStore } from "../../store/auth";
import api from "../../api/axios";

const router = useRouter();
const authStore = useAuthStore();

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

const navLinks = [
  { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/manager" },
  { icon: FileText, label: "All Tickets", to: "/tickets" },
  { icon: BarChart2, label: "Reports", to: "/reports" },
  { icon: Bell, label: "Notifications", to: "/notifications" },
  { icon: User, label: "Profile", to: "/profile" },
];

const ticketColumns = [
  { key: "referenceNumber", label: "Ref#" },
  { key: "title", label: "Title" },
  { key: "priority", label: "Priority", type: "priority" },
  { key: "status", label: "Status", type: "status" },
  { key: "assignedTo", label: "Agent" },
  { key: "createdAt", label: "Date" },
  { key: "view", label: "", type: "action", actionLabel: "View" },
];

const unassignedColumns = [
  { key: "referenceNumber", label: "Ref#" },
  { key: "title", label: "Title" },
  { key: "priority", label: "Priority" },
  { key: "createdBy", label: "Submitted By" },
  { key: "createdAt", label: "Date" },
  { key: "action", label: "Action" },
];

const escalatedColumns = [
  { key: "referenceNumber", label: "Ref#" },
  { key: "title", label: "Title" },
  { key: "priority", label: "Priority" },
  { key: "createdBy", label: "Submitted By" },
  { key: "createdAt", label: "Escalated" },
  { key: "action", label: "Action" },
];

const loading = ref(true);
const error = ref("");
const data = ref(null);
const days = ref(30);
const daysOptions = [7, 14, 30];

async function fetchData() {
  loading.value = true;
  error.value = "";
  try {
    const res = await api.get("/dashboard/manager", { params: { days: days.value } });
    data.value = res.data;
  } catch (e) {
    error.value =
      e.response?.data?.message || "Unable to connect to the server.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchData);
watch(days, fetchData);

const departmentLabel = computed(() => {
  const count = data.value?.unassignedTickets ?? 0;
  return count > 0
    ? `${count} ticket${count !== 1 ? "s" : ""} awaiting assignment`
    : "All tickets assigned";
});

const processedTickets = computed(() =>
  (data.value?.recentTickets ?? []).map((t) => ({
    ...t,
    assignedTo: t.assignedTo || "Unassigned",
    createdAt: formatDate(t.createdAt),
  }))
);

const barChartData = computed(() => ({
  labels: data.value?.agentPerformance?.map((d) => d.agentName) ?? [],
  datasets: [
    {
      label: "Resolved",
      data: data.value?.agentPerformance?.map((d) => d.resolvedTickets) ?? [],
      backgroundColor: "#14B8A6",
      borderRadius: 6,
      borderSkipped: false,
      hoverBackgroundColor: "#0D9488",
    },
  ],
}));

const barChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: "#1A1D2E",
      titleColor: "#F9FAFB",
      bodyColor: "#D1D5DB",
      borderColor: "#374151",
      borderWidth: 1,
      padding: 12,
      cornerRadius: 8,
    },
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { color: "#9CA3AF", font: { size: 11 } },
      border: { display: false },
    },
    y: {
      grid: { color: "rgba(156,163,175,0.12)" },
      ticks: { color: "#9CA3AF", font: { size: 11 }, padding: 8 },
      border: { display: false },
      beginAtZero: true,
    },
  },
};

function agentInitials(name) {
  return name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function priorityClass(priority) {
  const base = "px-2.5 py-0.5 rounded-full text-[11px] font-semibold";
  const map = {
    Low: "bg-gray-100 text-gray-600 dark:bg-gray-700/50 dark:text-gray-400",
    Medium:
      "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    High: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
    Critical: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
  };
  return `${base} ${map[priority] || map["Low"]}`;
}

function formatResolutionTime(hours) {
  if (!hours || hours === 0) return "—";
  return `${hours.toFixed(1)}h`;
}

function formatDate(dateStr) {
  if (!dateStr) return "—";
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr)
    ? dateStr
    : dateStr + "Z";
  return new Date(normalized).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}
</script>
