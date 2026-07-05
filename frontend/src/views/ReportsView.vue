<template>
  <AppLayout :navLinks="navLinks" pageTitle="Reports">

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-[60vh]">
      <div class="flex flex-col items-center gap-4">
        <div class="w-12 h-12 border-4 border-[#14B8A6] border-t-transparent rounded-full animate-spin" />
        <p class="text-sm text-gray-400">Loading report data...</p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex items-center justify-center min-h-[60vh]">
      <div class="bg-white dark:bg-[#1A1D2E] rounded-xl p-10 text-center max-w-sm shadow-sm border border-gray-100 dark:border-white/[0.05]">
        <AlertCircle :size="44" class="text-red-500 mx-auto mb-3" />
        <h3 class="font-semibold text-[#0F172A] dark:text-white mb-1 text-base">Failed to load</h3>
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">{{ error }}</p>
        <button @click="fetchData"
          class="px-5 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors">
          Try Again
        </button>
      </div>
    </div>

    <!-- Content -->
    <template v-else>
      <!-- ── Page header ────────────────────────────────────────────── -->
      <div class="flex flex-wrap items-start justify-between gap-4 mb-6">
        <div>
          <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">Reports</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">Ticket analytics and performance metrics</p>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <!-- Presets -->
          <div class="flex items-center gap-1 bg-gray-100 dark:bg-white/5 rounded-lg p-1">
            <button v-for="p in presets" :key="p.days"
              @click="applyPreset(p.days)"
              class="px-3 py-1.5 rounded-md text-xs font-semibold transition-colors"
              :class="activePreset === p.days
                ? 'bg-white dark:bg-white/10 text-[#0F172A] dark:text-white shadow-sm'
                : 'text-gray-400 hover:text-gray-600 dark:hover:text-gray-300'">
              {{ p.label }}
            </button>
          </div>

          <!-- Custom date inputs -->
          <div class="flex items-center gap-1.5">
            <input type="date" v-model="fromDate"
              class="px-2.5 py-1.5 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-white/5 text-[#0F172A] dark:text-white text-xs font-medium focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40" />
            <span class="text-xs text-gray-400">to</span>
            <input type="date" v-model="toDate"
              class="px-2.5 py-1.5 rounded-lg border border-gray-200 dark:border-white/10 bg-white dark:bg-white/5 text-[#0F172A] dark:text-white text-xs font-medium focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40" />
          </div>

          <!-- Export buttons -->
          <button @click="handleExportPdf" :disabled="reportsStore.pdfExporting"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#14B8A6] text-white text-xs font-semibold hover:bg-teal-600 active:bg-teal-700 transition-colors disabled:opacity-60">
            <Loader2 v-if="reportsStore.pdfExporting" :size="13" class="animate-spin" />
            <FileDown v-else :size="13" />
            Export PDF
          </button>
          <button @click="handleExportExcel" :disabled="reportsStore.excelExporting"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#10B981] text-white text-xs font-semibold hover:bg-emerald-600 active:bg-emerald-700 transition-colors disabled:opacity-60">
            <Loader2 v-if="reportsStore.excelExporting" :size="13" class="animate-spin" />
            <TableIcon v-else :size="13" />
            Export Excel
          </button>
        </div>
      </div>

      <!-- ── Stat cards ─────────────────────────────────────────────── -->
      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
        <StatCard title="Total Opened"    :value="summary?.volume?.totalOpened ?? 0"   color="#3B82F6" :icon="InboxIcon" />
        <StatCard title="Total Closed"    :value="summary?.volume?.totalClosed ?? 0"   color="#10B981" :icon="CheckCircle2" />
        <StatCard title="Net Change"      :value="summary?.volume?.netChange ?? 0"     color="#F59E0B" :icon="TrendingUp" />
        <StatCard title="Avg Resolution"  :value="avgResolutionLabel"                  color="#14B8A6" :icon="Clock" />
      </div>

      <!-- ── Ticket Volume line chart ────────────────────────────────── -->
      <div class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5 mb-6">
        <div class="flex items-center justify-between mb-5">
          <div>
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">Ticket Volume Trend</h3>
            <p class="text-xs text-gray-400 mt-0.5">Opened vs closed per day</p>
          </div>
        </div>
        <div v-if="hasVolumeData" style="position: relative; height: 220px">
          <Line :data="lineChartData" :options="lineChartOptions" />
        </div>
        <div v-else class="flex items-center justify-center h-[220px] text-sm text-gray-400 dark:text-gray-500">
          No tickets in this date range
        </div>
      </div>

      <!-- ── Resolution time charts ────────────────────────────────── -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
        <!-- By Category -->
        <div class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5">
          <div class="mb-4">
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">Avg Resolution by Category</h3>
            <p class="text-xs text-gray-400 mt-0.5">Hours to resolve, grouped by ticket category</p>
          </div>
          <div v-if="hasResolutionData && summary?.resolutionTime?.byCategory?.length" style="position: relative; height: 240px">
            <Bar :data="categoryBarData" :options="hoursBarOptions" />
          </div>
          <div v-else class="flex items-center justify-center h-[240px] text-sm text-gray-400 dark:text-gray-500">
            No resolved tickets in this period
          </div>
        </div>

        <!-- By Priority -->
        <div class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-5">
          <div class="mb-4">
            <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">Avg Resolution by Priority</h3>
            <p class="text-xs text-gray-400 mt-0.5">Hours to resolve, grouped by ticket priority</p>
          </div>
          <div v-if="hasResolutionData && summary?.resolutionTime?.byPriority?.length" style="position: relative; height: 240px">
            <Bar :data="priorityBarData" :options="hoursBarOptions" />
          </div>
          <div v-else class="flex items-center justify-center h-[240px] text-sm text-gray-400 dark:text-gray-500">
            No resolved tickets in this period
          </div>
        </div>
      </div>

      <!-- ── Employee / Agent breakdown table ────────────────────────── -->
      <div class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] overflow-hidden mb-6">
        <div class="px-5 py-4 border-b border-gray-100 dark:border-white/[0.06]">
          <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">{{ empTableTitle }}</h3>
          <p class="text-xs text-gray-400 mt-0.5">Ticket counts by status for each person in the date range</p>
        </div>

        <div v-if="hasEmployeeData">
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="bg-gray-50 dark:bg-white/[0.03]">
                  <th v-for="col in empColumns" :key="col.key"
                    class="text-left px-4 py-3 text-[11px] font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider cursor-pointer select-none hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
                    @click="toggleSort(col.key)">
                    <span class="flex items-center gap-1">
                      {{ col.label }}
                      <ArrowUpDown v-if="sortKey !== col.key" :size="10" class="opacity-40" />
                      <ArrowUp    v-else-if="sortDir === 'asc'"  :size="10" />
                      <ArrowDown  v-else                          :size="10" />
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100 dark:divide-white/[0.04]">
                <tr v-for="emp in paginatedEmployees" :key="emp.userId"
                  class="hover:bg-gray-50 dark:hover:bg-white/[0.02] transition-colors">
                  <td class="px-4 py-3.5 font-medium text-[13px] text-[#0F172A] dark:text-gray-200">{{ emp.name }}</td>
                  <td class="px-4 py-3.5">
                    <span class="px-2 py-0.5 rounded-full text-[10px] font-semibold"
                      :class="emp.role === 'Agent' ? 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400' : 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400'">
                      {{ emp.role }}
                    </span>
                  </td>
                  <td class="px-4 py-3.5 font-semibold text-[13px] text-[#0F172A] dark:text-white">{{ emp.totalTickets }}</td>
                  <td class="px-4 py-3.5 text-[13px] text-gray-600 dark:text-gray-400">{{ emp.open }}</td>
                  <td class="px-4 py-3.5 text-[13px] text-gray-600 dark:text-gray-400">{{ emp.inProgress }}</td>
                  <td class="px-4 py-3.5 text-[13px] text-gray-600 dark:text-gray-400">{{ emp.pending }}</td>
                  <td class="px-4 py-3.5 text-[13px] text-gray-600 dark:text-gray-400">{{ emp.resolved }}</td>
                  <td class="px-4 py-3.5 text-[13px] text-gray-600 dark:text-gray-400">{{ emp.closed }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="empTotalPages > 1"
            class="flex items-center justify-between px-5 py-3 border-t border-gray-100 dark:border-white/[0.06]">
            <p class="text-xs text-gray-400">
              Showing {{ (empPage - 1) * EMP_PAGE_SIZE + 1 }}–{{ Math.min(empPage * EMP_PAGE_SIZE, sortedEmployees.length) }}
              of {{ sortedEmployees.length }}
            </p>
            <div class="flex items-center gap-1">
              <button @click="empPage--" :disabled="empPage === 1"
                class="px-2.5 py-1 rounded-md text-xs font-medium border border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                Prev
              </button>
              <span class="px-3 py-1 text-xs text-gray-500">{{ empPage }} / {{ empTotalPages }}</span>
              <button @click="empPage++" :disabled="empPage === empTotalPages"
                class="px-2.5 py-1 rounded-md text-xs font-medium border border-gray-200 dark:border-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                Next
              </button>
            </div>
          </div>
        </div>

        <div v-else class="flex items-center justify-center h-24 text-sm text-gray-400 dark:text-gray-500">
          No employee or agent data for this period
        </div>
      </div>
    </template>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from "chart.js";
import { Line, Bar } from "vue-chartjs";
import {
  AlertCircle,
  FileDown,
  Table as TableIcon,
  Loader2,
  Inbox as InboxIcon,
  CheckCircle2,
  TrendingUp,
  Clock,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
} from "lucide-vue-next";
import AppLayout from "../components/layout/AppLayout.vue";
import StatCard   from "../components/dashboard/StatCard.vue";
import api        from "../api/axios";
import { useAuthStore }    from "../store/auth";
import { useReportsStore } from "../store/reports";
import { useNavLinks }     from "../composables/useNavLinks";

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, Title, Tooltip, Legend, Filler);

const authStore    = useAuthStore();
const reportsStore = useReportsStore();

// ── Date range ─────────────────────────────────────────────────────────────
const EMP_PAGE_SIZE = 10;

function toIso(d) { return d.toISOString().split("T")[0]; }
function daysAgo(n) { return toIso(new Date(Date.now() - n * 86400000)); }

const fromDate    = ref(daysAgo(29));
const toDate      = ref(toIso(new Date()));
const activePreset = ref(30);

const presets = [
  { days: 7,  label: "7d"  },
  { days: 30, label: "30d" },
  { days: 90, label: "90d" },
];

function applyPreset(days) {
  activePreset.value = days;
  fromDate.value     = daysAgo(days - 1);
  toDate.value       = toIso(new Date());
}

// ── Data fetching ──────────────────────────────────────────────────────────
const loading = ref(true);
const error   = ref("");
const summary = ref(null);

async function fetchData() {
  loading.value = true;
  error.value   = "";
  try {
    const res  = await api.get("/reports/summary", { params: { from: fromDate.value, to: toDate.value } });
    summary.value = res.data;
    empPage.value = 1;
  } catch (e) {
    error.value = e.response?.data?.message || "Unable to connect to the server.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchData);
watch([fromDate, toDate], fetchData);

// ── Exports ────────────────────────────────────────────────────────────────
async function handleExportPdf() {
  try { await reportsStore.exportPdf(fromDate.value, toDate.value); }
  catch { /* server error — user can retry */ }
}

async function handleExportExcel() {
  try { await reportsStore.exportExcel(fromDate.value, toDate.value); }
  catch { /* server error — user can retry */ }
}

const { navLinks } = useNavLinks();

// ── Computed display values ────────────────────────────────────────────────
const avgResolutionLabel = computed(() => {
  const h = summary.value?.resolutionTime?.avgHoursOverall ?? 0;
  return `${h.toFixed(1)} hrs`;
});

const hasVolumeData    = computed(() => (summary.value?.volume?.totalOpened ?? 0) + (summary.value?.volume?.totalClosed ?? 0) > 0);
const hasResolutionData = computed(() => (summary.value?.resolutionTime?.totalResolved ?? 0) > 0);
const hasEmployeeData  = computed(() => (summary.value?.byEmployee?.length ?? 0) > 0);

const isEmployee     = computed(() => authStore.userRole === "Employee");
const empTableTitle  = computed(() => isEmployee.value ? "Your Ticket Summary" : "Employee / Agent Breakdown");

// ── Chart data ──────────────────────────────────────────────────────────────
const lineChartData = computed(() => ({
  labels: summary.value?.volume?.daily?.map(d => d.date) ?? [],
  datasets: [
    {
      label: "Opened",
      data: summary.value?.volume?.daily?.map(d => d.opened) ?? [],
      borderColor: "#3B82F6",
      backgroundColor: "rgba(59,130,246,0.08)",
      tension: 0.4,
      fill: true,
      pointRadius: 3,
      pointHoverRadius: 5,
      pointBackgroundColor: "#3B82F6",
      borderWidth: 2,
    },
    {
      label: "Closed",
      data: summary.value?.volume?.daily?.map(d => d.closed) ?? [],
      borderColor: "#14B8A6",
      backgroundColor: "rgba(20,184,166,0.08)",
      tension: 0.4,
      fill: true,
      pointRadius: 3,
      pointHoverRadius: 5,
      pointBackgroundColor: "#14B8A6",
      borderWidth: 2,
    },
  ],
}));

const categoryBarData = computed(() => ({
  labels: summary.value?.resolutionTime?.byCategory?.map(d => d.name) ?? [],
  datasets: [{
    label: "Avg Hours",
    data: summary.value?.resolutionTime?.byCategory?.map(d => d.avgHours) ?? [],
    backgroundColor: "#14B8A6",
    hoverBackgroundColor: "#0D9488",
    borderRadius: 5,
    borderSkipped: false,
  }],
}));

const priorityColorMap = { Low: "#9CA3AF", Medium: "#F59E0B", High: "#F97316", Critical: "#EF4444" };

const priorityBarData = computed(() => ({
  labels: summary.value?.resolutionTime?.byPriority?.map(d => d.name) ?? [],
  datasets: [{
    label: "Avg Hours",
    data: summary.value?.resolutionTime?.byPriority?.map(d => d.avgHours) ?? [],
    backgroundColor: summary.value?.resolutionTime?.byPriority?.map(d => priorityColorMap[d.name] ?? "#6B7280") ?? [],
    borderRadius: 5,
    borderSkipped: false,
  }],
}));

const lineChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: { mode: "index", intersect: false },
  plugins: {
    legend: {
      position: "top", align: "end",
      labels: { usePointStyle: true, pointStyleWidth: 8, boxHeight: 8, padding: 20, font: { size: 11 }, color: "#9CA3AF" },
    },
    tooltip: { backgroundColor: "#1A1D2E", titleColor: "#F9FAFB", bodyColor: "#D1D5DB", borderColor: "#374151", borderWidth: 1, padding: 12, cornerRadius: 8 },
  },
  scales: {
    x: { grid: { display: false }, ticks: { color: "#9CA3AF", font: { size: 10 }, maxTicksLimit: 15 }, border: { display: false } },
    y: { grid: { color: "rgba(156,163,175,0.12)" }, ticks: { color: "#9CA3AF", font: { size: 11 }, padding: 8 }, border: { display: false }, beginAtZero: true },
  },
};

const hoursBarOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: { backgroundColor: "#1A1D2E", titleColor: "#F9FAFB", bodyColor: "#D1D5DB", padding: 10, cornerRadius: 8,
      callbacks: { label: ctx => ` ${ctx.parsed.y.toFixed(1)} hrs` } },
  },
  scales: {
    x: { grid: { display: false }, ticks: { color: "#9CA3AF", font: { size: 10 }, maxRotation: 40, minRotation: 0 }, border: { display: false } },
    y: { grid: { color: "rgba(156,163,175,0.12)" }, ticks: { color: "#9CA3AF", font: { size: 10 }, padding: 6, callback: v => `${v}h` }, border: { display: false }, beginAtZero: true },
  },
};

// ── Employee table sort + pagination ───────────────────────────────────────
const empColumns = [
  { key: "name",         label: "Name" },
  { key: "role",         label: "Role" },
  { key: "totalTickets", label: "Total" },
  { key: "open",         label: "Open" },
  { key: "inProgress",   label: "In Progress" },
  { key: "pending",      label: "Pending" },
  { key: "resolved",     label: "Resolved" },
  { key: "closed",       label: "Closed" },
];

const sortKey = ref("totalTickets");
const sortDir = ref("desc");
const empPage = ref(1);

function toggleSort(key) {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === "asc" ? "desc" : "asc";
  } else {
    sortKey.value = key;
    sortDir.value = "desc";
  }
  empPage.value = 1;
}

const sortedEmployees = computed(() => {
  const data = [...(summary.value?.byEmployee ?? [])];
  data.sort((a, b) => {
    const av = a[sortKey.value];
    const bv = b[sortKey.value];
    if (typeof av === "string") return sortDir.value === "asc" ? av.localeCompare(bv) : bv.localeCompare(av);
    return sortDir.value === "asc" ? av - bv : bv - av;
  });
  return data;
});

const empTotalPages = computed(() => Math.max(1, Math.ceil(sortedEmployees.value.length / EMP_PAGE_SIZE)));

const paginatedEmployees = computed(() => {
  const start = (empPage.value - 1) * EMP_PAGE_SIZE;
  return sortedEmployees.value.slice(start, start + EMP_PAGE_SIZE);
});
</script>
