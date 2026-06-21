<template>
  <AppLayout
    :navLinks="navLinks"
    pageTitle="Dashboard"
    :notificationCount="data?.recentActivity?.length ?? 0"
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
      <!-- Page header -->
      <div class="flex flex-wrap items-start justify-between gap-4 mb-6">
        <div>
          <div class="flex items-center gap-2.5 mb-1">
            <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">
              Dashboard
            </h1>
          </div>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Overview of system health and ticket performance
          </p>
        </div>
        <div class="flex items-center gap-2 flex-shrink-0">
          <button
            class="px-4 py-2 rounded-lg border border-gray-200 dark:border-white/10 text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors flex items-center gap-2"
          >
            <CalendarDays :size="14" />
            Last 7 Days
          </button>
          <button
            class="px-4 py-2 rounded-lg bg-[#0F172A] dark:bg-white/10 text-white text-sm font-semibold hover:bg-gray-800 dark:hover:bg-white/20 transition-colors flex items-center gap-2"
          >
            <Download :size="14" />
            Export Report
          </button>
        </div>
      </div>

      <!-- Stat cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
        <StatCard
          title="Total Users"
          :value="data?.totalUsers ?? 0"
          color="#10B981"
          :icon="Users"
        />
        <StatCard
          title="Total Tickets"
          :value="data?.totalTickets ?? 0"
          color="#3B82F6"
          :icon="FileStack"
        />
        <StatCard
          title="Open Tickets"
          :value="data?.openTickets ?? 0"
          color="#F59E0B"
          :icon="Inbox"
        />
        <StatCard
          title="Resolved Today"
          :value="data?.resolvedToday ?? 0"
          color="#10B981"
          :icon="CheckCircle2"
        />
      </div>

      <!-- Charts row -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 mb-6">
        <!-- Line chart: ticket trend (wide) -->
        <div
          class="lg:col-span-2 bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5"
        >
          <div class="flex items-center justify-between mb-5">
            <div>
              <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
                Ticket Trend
              </h3>
              <p class="text-xs text-gray-400 mt-0.5">
                Created vs resolved over last 7 days
              </p>
            </div>
            <span
              class="text-xs text-gray-400 bg-gray-100 dark:bg-white/5 px-2.5 py-1 rounded-full"
            >
              7 days
            </span>
          </div>
          <div style="position: relative; height: 210px">
            <Line :data="lineChartData" :options="lineChartOptions" />
          </div>
        </div>

        <!-- Bar chart: ticket types / categories -->
        <div
          class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5"
        >
          <div class="mb-4">
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
              Ticket Types
            </h3>
            <p class="text-xs text-gray-400 mt-0.5">Tickets by category</p>
          </div>
          <div style="position: relative; height: 240px">
            <Bar :data="categoryBarData" :options="categoryBarOptions" />
          </div>
        </div>
      </div>

      <!-- Activity + priority pie row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
        <!-- Recent activity -->
        <div
          class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5"
        >
          <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm mb-4">
            Recent Activity
          </h3>
          <div v-if="data?.recentActivity?.length" class="space-y-4">
            <div
              v-for="(item, i) in data.recentActivity.slice(0, 6)"
              :key="i"
              class="flex items-start gap-3"
            >
              <div
                class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5"
                :style="{ backgroundColor: activityColor(item.action).bg }"
              >
                <component
                  :is="activityIcon(item.action)"
                  :size="14"
                  :style="{ color: activityColor(item.action).fg }"
                />
              </div>
              <div class="min-w-0 flex-1">
                <p
                  class="text-[13px] text-[#0F172A] dark:text-gray-200 font-medium leading-snug"
                >
                  {{ item.action }}
                </p>
                <p
                  class="text-[11px] text-gray-400 dark:text-gray-500 mt-0.5 truncate"
                >
                  {{ item.userName }}
                  <span class="mx-1">·</span>
                  {{ timeAgo(item.loggedAt) }}
                </p>
              </div>
            </div>
          </div>
          <div
            v-else
            class="py-8 text-center text-gray-400 dark:text-gray-500 text-sm"
          >
            No recent activity
          </div>
        </div>

        <!-- Priority pie chart -->
        <div
          class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5"
        >
          <div class="mb-4">
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
              By Priority
            </h3>
            <p class="text-xs text-gray-400 mt-0.5">Priority distribution</p>
          </div>
          <div style="position: relative; height: 240px">
            <Doughnut :data="priorityDoughnutData" :options="doughnutOptions" />
          </div>
        </div>
      </div>

      <!-- Recent tickets table -->
      <TicketTable
        title="Recent Tickets"
        :tickets="processedTickets"
        :columns="ticketColumns"
        @row-click="(t) => router.push(`/tickets/${t.id}`)"
        @action-click="(t) => router.push(`/tickets/${t.id}`)"
      />
    </template>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from "chart.js";
import { Line, Bar, Doughnut } from "vue-chartjs";
import {
  LayoutDashboard,
  FileText,
  FileStack,
  Users,
  BarChart2,
  Settings,
  User,
  Inbox,
  CheckCircle2,
  AlertTriangle,
  AlertCircle,
  CalendarDays,
  Download,
  PlusCircle,
  CheckCircle,
  MessageSquare,
  UserCheck,
  Activity,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import StatCard from "../../components/dashboard/StatCard.vue";
import TicketTable from "../../components/dashboard/TicketTable.vue";
import api from "../../api/axios";

const router = useRouter();

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

const navLinks = [
  { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/admin" },
  { icon: FileText, label: "All Tickets", to: "/tickets" },
  { icon: Users, label: "Users", to: "/admin/users" },
  { icon: BarChart2, label: "Reports", to: "/reports" },
  { icon: Settings, label: "Settings", to: "/settings" },
  { icon: User, label: "Profile", to: "/profile" },
];

const ticketColumns = [
  { key: "referenceNumber", label: "Ref#" },
  { key: "title", label: "Title" },
  { key: "category", label: "Category" },
  { key: "priority", label: "Priority", type: "priority" },
  { key: "status", label: "Status", type: "status" },
  { key: "assignedTo", label: "Assigned To" },
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
    const res = await api.get("/dashboard/admin");
    data.value = res.data;
  } catch (e) {
    error.value =
      e.response?.data?.message || "Unable to connect to the server.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchData);

const processedTickets = computed(() =>
  (data.value?.recentTickets ?? []).map((t) => ({
    ...t,
    assignedTo: t.assignedTo || "Unassigned",
    createdAt: formatDate(t.createdAt),
  }))
);

// Chart data
const lineChartData = computed(() => ({
  labels: data.value?.ticketTrend?.map((d) => d.date) ?? [],
  datasets: [
    {
      label: "Created",
      data: data.value?.ticketTrend?.map((d) => d.created) ?? [],
      borderColor: "#3B82F6",
      backgroundColor: "rgba(59,130,246,0.08)",
      tension: 0.4,
      fill: true,
      pointRadius: 4,
      pointHoverRadius: 6,
      pointBackgroundColor: "#3B82F6",
      borderWidth: 2,
    },
    {
      label: "Resolved",
      data: data.value?.ticketTrend?.map((d) => d.resolved) ?? [],
      borderColor: "#14B8A6",
      backgroundColor: "rgba(20,184,166,0.08)",
      tension: 0.4,
      fill: true,
      pointRadius: 4,
      pointHoverRadius: 6,
      pointBackgroundColor: "#14B8A6",
      borderWidth: 2,
    },
  ],
}));

const lineChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: "index", intersect: false },
  plugins: {
    legend: {
      position: "top",
      align: "end",
      labels: {
        usePointStyle: true,
        pointStyleWidth: 8,
        boxHeight: 8,
        padding: 20,
        font: { size: 11 },
        color: "#9CA3AF",
      },
    },
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

const categoryBarData = computed(() => {
  const sorted = [...(data.value?.categoryBreakdown ?? [])].sort((a, b) => {
    if (a.category === "Other") return 1;
    if (b.category === "Other") return -1;
    return a.category.localeCompare(b.category);
  });
  return {
    labels: sorted.map((d) => d.category),
    datasets: [
      {
        label: "Tickets",
        data: sorted.map((d) => d.count),
        backgroundColor: "#14B8A6",
        hoverBackgroundColor: "#0D9488",
        borderRadius: 5,
        borderSkipped: false,
      },
    ],
  };
});

const categoryBarOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      backgroundColor: "#1A1D2E",
      titleColor: "#F9FAFB",
      bodyColor: "#D1D5DB",
      padding: 10,
      cornerRadius: 8,
    },
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: {
        color: "#9CA3AF",
        font: { size: 10 },
        maxRotation: 45,
        minRotation: 45,
      },
      border: { display: false },
    },
    y: {
      grid: { color: "rgba(156,163,175,0.12)" },
      ticks: { color: "#9CA3AF", font: { size: 10 }, padding: 6 },
      border: { display: false },
      beginAtZero: true,
    },
  },
};

const priorityColorMap = {
  Low: "#9CA3AF",
  Medium: "#F59E0B",
  High: "#F97316",
  Critical: "#EF4444",
};

const priorityDoughnutData = computed(() => {
  const items = data.value?.priorityBreakdown ?? [];
  return {
    labels: items.map((d) => d.priority),
    datasets: [
      {
        data: items.map((d) => d.count),
        backgroundColor: items.map(
          (d) => priorityColorMap[d.priority] ?? "#9CA3AF"
        ),
        borderWidth: 0,
        hoverOffset: 6,
      },
    ],
  };
});

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: "bottom",
      labels: {
        usePointStyle: true,
        pointStyleWidth: 8,
        boxHeight: 8,
        padding: 14,
        font: { size: 11 },
        color: "#9CA3AF",
      },
    },
    tooltip: {
      backgroundColor: "#1A1D2E",
      titleColor: "#F9FAFB",
      bodyColor: "#D1D5DB",
      padding: 10,
      cornerRadius: 8,
    },
  },
  cutout: "68%",
};

// Activity helpers
function activityColor(action) {
  const a = action?.toLowerCase() ?? "";
  if (a.includes("resolv") || a.includes("close"))
    return { bg: "#10B98120", fg: "#10B981" };
  if (a.includes("escalat")) return { bg: "#F9731620", fg: "#F97316" };
  if (a.includes("comment")) return { bg: "#14B8A620", fg: "#14B8A6" };
  if (a.includes("assign")) return { bg: "#8B5CF620", fg: "#8B5CF6" };
  if (a.includes("creat") || a.includes("open"))
    return { bg: "#3B82F620", fg: "#3B82F6" };
  return { bg: "#6B728020", fg: "#6B7280" };
}

function activityIcon(action) {
  const a = action?.toLowerCase() ?? "";
  if (a.includes("resolv") || a.includes("close")) return CheckCircle;
  if (a.includes("escalat")) return AlertTriangle;
  if (a.includes("comment")) return MessageSquare;
  if (a.includes("assign")) return UserCheck;
  if (a.includes("creat") || a.includes("open")) return PlusCircle;
  return Activity;
}

function normalizeUtc(dateStr) {
  return /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr) ? dateStr : dateStr + "Z";
}

function timeAgo(dateStr) {
  if (!dateStr) return "";
  const d = new Date(normalizeUtc(dateStr));
  const diff = Math.floor((Date.now() - d.getTime()) / 1000);
  if (diff < 60) return "Just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 86400 * 7) return `${Math.floor(diff / 86400)}d ago`;
  return d.toLocaleDateString("en-US", { month: "short", day: "numeric" });
}

function formatDate(dateStr) {
  if (!dateStr) return "—";
  return new Date(normalizeUtc(dateStr)).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}
</script>
