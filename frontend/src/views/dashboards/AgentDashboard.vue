<template>
  <AppLayout
    :navLinks="navLinks"
    pageTitle="Dashboard"
    :notificationCount="data?.inProgress ?? 0"
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
        </div>
      </div>

      <!-- Stat cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
        <StatCard
          title="Assigned to Me"
          :value="data?.assignedToMe ?? 0"
          color="#10B981"
          :icon="Inbox"
        />
        <StatCard
          title="Resolved Today"
          :value="data?.resolvedToday ?? 0"
          color="#10B981"
          :icon="CheckCircle2"
        />
        <StatCard
          title="In Progress"
          :value="data?.inProgress ?? 0"
          color="#14B8A6"
          :icon="Clock"
        />
        <StatCard
          title="Escalated"
          :value="data?.escalatedCount ?? 0"
          color="#EF4444"
          :icon="TrendingUp"
        />
      </div>

      <!--active tickets + quick actions -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div class="lg:col-span-2">
          <TicketTable
            title="Tickets Assigned to Me"
            :tickets="processedTickets"
            :columns="ticketColumns"
            @row-click="startWorking"
            @action-click="startWorking"
          />
        </div>

        <!-- Quick actions -->
        <div class="space-y-3">
          <h3
            class="font-semibold text-[#0F172A] dark:text-white text-sm px-0.5"
          >
            Quick Actions
          </h3>

          <button
            v-for="action in quickActions"
            :key="action.title"
            @click="action.onClick()"
            class="w-full flex items-center gap-3 bg-white dark:bg-[#1A1D2E] rounded-xl p-3 shadow-sm border border-gray-100 dark:border-white/[0.05] transition-colors duration-200 text-left group hover:bg-gray-50 dark:hover:bg-white/[0.04]"
          >
            <div
              class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
              :style="{ backgroundColor: `${action.color}1a` }"
            >
              <component
                :is="action.icon"
                :size="15"
                :style="{ color: action.color }"
              />
            </div>
            <div class="min-w-0 flex-1">
              <p
                class="text-xs font-semibold text-[#0F172A] dark:text-white leading-none mb-0.5 group-hover:text-[#14B8A6] transition-colors"
              >
                {{ action.title }}
              </p>
              <p
                class="text-[11px] text-gray-400 dark:text-gray-500 leading-snug"
              >
                {{ action.description }}
              </p>
            </div>
            <ChevronRight
              :size="14"
              class="text-gray-300 dark:text-gray-600 flex-shrink-0 ml-auto group-hover:text-[#14B8A6] transition-colors"
            />
          </button>
        </div>
      </div>
    </template>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import {
  LayoutDashboard,
  FileText,
  Bell,
  User,
  Inbox,
  CheckCircle2,
  Clock,
  TrendingUp,
  AlertCircle,
  RefreshCw,
  MessageSquare,
  AlertTriangle,
  ChevronRight,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import StatCard from "../../components/dashboard/StatCard.vue";
import TicketTable from "../../components/dashboard/TicketTable.vue";
import api from "../../api/axios";

const router = useRouter();

const navLinks = [
  { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/agent" },
  { icon: FileText, label: "My Tickets", to: "/tickets" },
  { icon: Bell, label: "Notifications", to: "/notifications" },
  { icon: User, label: "Profile", to: "/profile" },
];

const quickActions = [
  {
    icon: RefreshCw,
    title: "Update Ticket Status",
    description: "Click a ticket below to update its status",
    color: "#14B8A6",
    onClick: () => router.push("/tickets"),
  },
  {
    icon: MessageSquare,
    title: "Add Comment",
    description: "Open a ticket to post an update or note",
    color: "#3B82F6",
    onClick: () => router.push("/tickets"),
  },
  {
    icon: AlertTriangle,
    title: "Escalate Ticket",
    description: "Open a ticket to flag it as urgent",
    color: "#EF4444",
    onClick: () => router.push("/tickets"),
  },
];

const ticketColumns = [
  { key: "referenceNumber", label: "Ref#" },
  { key: "title", label: "Title" },
  { key: "createdBy", label: "Submitted By" },
  { key: "priority", label: "Priority", type: "priority" },
  { key: "status", label: "Status", type: "status" },
  { key: "createdAt", label: "Date" },
  {
    key: "start",
    label: "Action",
    type: "action",
    showIf: (ticket) =>
      ticket.status === "Open" || ticket.status === "In Progress",
    labelFn: (ticket) => (ticket.status === "Open" ? "Start" : "Continue"),
  },
];

const loading = ref(true);
const error = ref("");
const data = ref(null);

async function fetchData() {
  loading.value = true;
  error.value = "";
  try {
    const res = await api.get("/dashboard/agent");
    data.value = res.data;
  } catch (e) {
    error.value =
      e.response?.data?.message || "Unable to connect to the server.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchData);

async function startWorking(ticket) {
  router.push(`/tickets/${ticket.id}`);
}

const processedTickets = computed(() =>
  (data.value?.myTickets ?? []).map((t) => ({
    ...t,
    createdAt: formatDate(t.createdAt),
  }))
);

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
