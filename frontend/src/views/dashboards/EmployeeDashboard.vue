<template>
  <AppLayout
    :navLinks="navLinks"
    pageTitle="Dashboard"
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
        </div>
        <button
          @click="router.push('/tickets/create')"
          class="flex-shrink-0 flex items-center gap-2 px-4 py-2.5 bg-[#14B8A6] text-white text-sm font-semibold rounded-lg hover:bg-teal-600 active:bg-teal-700 transition-colors shadow-sm"
        >
          <Plus :size="15" />
          Create New Ticket
        </button>
      </div>

      <!-- Stat cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
        <StatCard
          title="My Open Tickets"
          :value="data?.myOpenTickets ?? 0"
          color="#10B981"
          :icon="Inbox"
        />
        <StatCard
          title="In Progress"
          :value="data?.myInProgressTickets ?? 0"
          color="#F59E0B"
          :icon="Clock"
        />
        <StatCard
          title="Resolved"
          :value="data?.myResolvedTickets ?? 0"
          color="#10B981"
          :icon="CheckCircle2"
        />
        <StatCard
          title="Total Submitted"
          :value="data?.myTotalTickets ?? 0"
          color="#3B82F6"
          :icon="FileStack"
        />
      </div>

      <!-- Tickets + quick links row -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <!-- My recent tickets -->
        <div class="lg:col-span-2">
          <TicketTable
            title="My Recent Tickets"
            :tickets="processedTickets"
            :columns="ticketColumns"
            @row-click="(t) => router.push(`/tickets/${t.id}`)"
            @action-click="(t) => router.push(`/tickets/${t.id}`)"
          />
        </div>

        <!-- Quick Links -->
        <div class="space-y-3">
          <h3
            class="font-semibold text-[#0F172A] dark:text-white text-sm px-0.5"
          >
            Quick Links
          </h3>
          <button
            v-for="link in quickLinks"
            :key="link.title"
            @click="link.onClick()"
            class="w-full flex items-center gap-3 bg-white dark:bg-[#1A1D2E] rounded-xl p-3 shadow-sm border border-gray-100 dark:border-white/[0.05] transition-colors duration-200 text-left group hover:bg-gray-50 dark:hover:bg-white/[0.04]"
          >
            <div
              class="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0"
              :style="{ backgroundColor: `${link.color}1a` }"
            >
              <component
                :is="link.icon"
                :size="15"
                :style="{ color: link.color }"
              />
            </div>
            <div class="min-w-0 flex-1">
              <p
                class="text-xs font-semibold text-[#0F172A] dark:text-white leading-none mb-0.5 group-hover:text-[#14B8A6] transition-colors"
              >
                {{ link.title }}
              </p>
              <p
                class="text-[11px] text-gray-400 dark:text-gray-500 leading-snug"
              >
                {{ link.description }}
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
  FileStack,
  AlertCircle,
  Plus,
  Search,
  Headphones,
  ChevronRight,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import StatCard from "../../components/dashboard/StatCard.vue";
import TicketTable from "../../components/dashboard/TicketTable.vue";
import api from "../../api/axios";

const router = useRouter();

const navLinks = [
  { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/employee" },
  { icon: FileText, label: "My Tickets", to: "/tickets" },
  { icon: Bell, label: "Notifications", to: "/notifications" },
  { icon: User, label: "Profile", to: "/profile" },
];

const ticketColumns = [
  { key: "referenceNumber", label: "Ref#" },
  { key: "title", label: "Title" },
  { key: "category", label: "Category" },
  { key: "priority", label: "Priority", type: "priority" },
  { key: "status", label: "Status", type: "status" },
  { key: "createdAt", label: "Date" },
  { key: "view", label: "", type: "action", actionLabel: "View" },
];

const loading = ref(true);
const error = ref("");
const data = ref(null);

async function fetchData() {
  loading.value = true;
  error.value = "";
  try {
    const res = await api.get("/dashboard/employee");
    data.value = res.data;
  } catch (e) {
    error.value =
      e.response?.data?.message || "Unable to connect to the server.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchData);

const quickLinks = [
  {
    icon: Plus,
    title: "Submit a Ticket",
    description: "Report a new IT issue or request",
    color: "#14B8A6",
    onClick: () => router.push("/tickets/create"),
  },
  {
    icon: Search,
    title: "Check Ticket Status",
    description: "Track the progress of your tickets",
    color: "#3B82F6",
    onClick: () => router.push("/tickets"),
  },
  {
    icon: Headphones,
    title: "IT Support FAQs",
    description: "Reach the helpdesk team directly",
    color: "#8B5CF6",
    onClick: () => router.push("/knowledge-base"),
  },
];

const processedTickets = computed(() =>
  (data.value?.myRecentTickets ?? []).map((t) => ({
    ...t,
    assignedTo: t.assignedTo || "Unassigned",
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
