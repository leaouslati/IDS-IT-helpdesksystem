<template>
  <AppLayout :navLinks="navLinks" pageTitle="System Management">
    <div class="mb-6 flex flex-wrap items-start justify-between gap-4">
      <div>
        <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">
          System Management
        </h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
          System health at a glance and the company holiday calendar
        </p>
      </div>
    </div>

    <!-- ── System Info ────────────────────────────────────────────────────── -->
    <div
      class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-6 mb-6"
    >
      <div class="flex items-center justify-between mb-5">
        <h2 class="text-sm font-bold text-[#0F172A] dark:text-white">
          System Info
        </h2>
        <button
          @click="fetchSystemInfo"
          :disabled="infoLoading"
          class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-gray-100 dark:bg-white/5 text-xs font-semibold text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-white/10 transition-colors disabled:opacity-50"
        >
          <RefreshCw :size="13" :class="infoLoading ? 'animate-spin' : ''" />
          Refresh
        </button>
      </div>

      <div v-if="infoLoading" class="py-10 flex justify-center">
        <LoadingSpinner />
      </div>

      <template v-else-if="systemInfo">
        <!-- Role breakdown -->
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-5">
          <StatCard title="Admins" :value="systemInfo.adminCount" color="#8B5CF6" :icon="ShieldCheck" />
          <StatCard title="Managers" :value="systemInfo.managerCount" color="#3B82F6" :icon="Users" />
          <StatCard title="Agents" :value="systemInfo.agentCount" color="#10B981" :icon="Headphones" />
          <StatCard title="Employees" :value="systemInfo.employeeCount" color="#F59E0B" :icon="User" />
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-3 gap-4 pt-4 border-t border-gray-100 dark:border-white/[0.06] text-sm">
          <div>
            <p class="text-xs text-gray-400 mb-0.5">Total Users</p>
            <p class="font-semibold text-[#0F172A] dark:text-white">{{ systemInfo.totalUsers }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400 mb-0.5">Total Tickets</p>
            <p class="font-semibold text-[#0F172A] dark:text-white">{{ systemInfo.totalTickets }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400 mb-0.5">Storage Used</p>
            <p class="font-semibold text-[#0F172A] dark:text-white">{{ formatBytes(systemInfo.storageUsedBytes) }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400 mb-0.5">App Version</p>
            <p class="font-semibold text-[#0F172A] dark:text-white">{{ systemInfo.appVersion }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400 mb-0.5">Database</p>
            <p class="font-semibold text-[#0F172A] dark:text-white">{{ systemInfo.databaseName }}</p>
          </div>
          <div>
            <p class="text-xs text-gray-400 mb-0.5">Server Time (UTC)</p>
            <p class="font-semibold text-[#0F172A] dark:text-white">{{ formatDateTime(systemInfo.serverTimeUtc) }}</p>
          </div>
        </div>
      </template>
    </div>

    <!-- ── Holiday Calendar ───────────────────────────────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Calendar -->
      <div
        class="lg:col-span-2 bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-6"
      >
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <button
              @click="changeMonth(-1)"
              class="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
            >
              <ChevronLeft :size="16" />
            </button>
            <h2 class="text-sm font-bold text-[#0F172A] dark:text-white w-36 text-center">
              {{ monthLabel }}
            </h2>
            <button
              @click="changeMonth(1)"
              class="p-1.5 rounded-lg text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
            >
              <ChevronRight :size="16" />
            </button>
          </div>
          <button
            @click="openAddModal()"
            class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#14B8A6] text-white text-xs font-semibold hover:bg-teal-600 transition-colors"
          >
            <Plus :size="13" />
            Add Holiday
          </button>
        </div>

        <!-- Weekday header -->
        <div class="grid grid-cols-7 gap-1 mb-1">
          <div
            v-for="d in weekdayLabels"
            :key="d"
            class="text-center text-[10px] font-semibold text-gray-400 uppercase py-1"
          >
            {{ d }}
          </div>
        </div>

        <!-- Grid -->
        <div class="grid grid-cols-7 gap-1">
          <button
            v-for="cell in calendarCells"
            :key="cell.key"
            @click="cell.holiday ? selectHoliday(cell.holiday) : null"
            class="aspect-square rounded-lg text-xs p-1 flex flex-col items-center justify-start transition-colors"
            :class="[
              cell.inCurrentMonth ? 'text-gray-700 dark:text-gray-300' : 'text-gray-300 dark:text-gray-600',
              cell.holiday
                ? 'bg-[#14B8A6]/10 hover:bg-[#14B8A6]/20 cursor-pointer font-semibold text-[#14B8A6]'
                : 'hover:bg-gray-50 dark:hover:bg-white/[0.03]',
            ]"
          >
            <span class="mt-0.5">{{ cell.dayNumber }}</span>
            <span
              v-if="cell.holiday"
              class="w-1 h-1 rounded-full bg-[#14B8A6] mt-0.5"
            />
          </button>
        </div>

        <!-- Selected holiday detail -->
        <div
          v-if="selectedHoliday"
          class="mt-4 p-4 rounded-xl bg-gray-50 dark:bg-white/[0.03] border border-gray-100 dark:border-white/[0.06]"
        >
          <div class="flex items-start justify-between gap-3">
            <div>
              <p class="text-sm font-semibold text-[#0F172A] dark:text-white">
                {{ selectedHoliday.name }}
              </p>
              <p class="text-xs text-gray-400 mt-0.5">
                {{ formatDate(selectedHoliday.date) }}
                <span v-if="selectedHoliday.isRecurring" class="ml-1.5 px-1.5 py-0.5 rounded-full bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 text-[10px] font-bold">
                  Recurring
                </span>
              </p>
            </div>
            <div class="flex items-center gap-1 flex-shrink-0">
              <button
                @click="openEditModal(selectedHoliday)"
                class="p-1.5 rounded-lg text-gray-400 hover:text-[#14B8A6] hover:bg-[#14B8A6]/10 transition-colors"
              >
                <Pencil :size="13" />
              </button>
              <button
                @click="confirmDelete(selectedHoliday)"
                class="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/10 transition-colors"
              >
                <Trash2 :size="13" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Holiday list -->
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-6"
      >
        <h2 class="text-sm font-bold text-[#0F172A] dark:text-white mb-4">
          {{ currentYear }} Holidays
        </h2>

        <div v-if="holidaysLoading" class="py-10 flex justify-center">
          <LoadingSpinner />
        </div>
        <div
          v-else-if="!yearHolidays.length"
          class="py-10 text-center text-gray-400 text-sm"
        >
          No holidays added yet
        </div>
        <div v-else class="space-y-2 max-h-[420px] overflow-y-auto">
          <div
            v-for="h in yearHolidays"
            :key="h.id"
            @click="openEditModal(h)"
            class="flex items-center justify-between gap-2 p-3 rounded-lg bg-gray-50/60 dark:bg-white/[0.02] hover:bg-gray-100 dark:hover:bg-white/[0.05] cursor-pointer transition-colors group"
          >
            <div class="min-w-0">
              <p class="text-sm font-semibold text-[#0F172A] dark:text-white truncate">
                {{ h.name }}
              </p>
              <p class="text-xs text-gray-400 mt-0.5 flex items-center gap-1.5">
                {{ formatDate(h.date) }}
                <span v-if="h.isRecurring" class="px-1.5 py-0.5 rounded-full bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 text-[9px] font-bold">
                  Recurring
                </span>
              </p>
            </div>
            <button
              @click.stop="confirmDelete(h)"
              class="p-1.5 rounded-lg text-gray-300 opacity-0 group-hover:opacity-100 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/10 transition-all flex-shrink-0"
            >
              <Trash2 :size="13" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Add/Edit Holiday Modal ─────────────────────────────────────────── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showFormModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showFormModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-sm border border-gray-100 dark:border-white/[0.08] p-6"
          >
            <h3 class="font-bold text-[#0F172A] dark:text-white mb-4">
              {{ editingHoliday ? "Edit Holiday" : "Add Holiday" }}
            </h3>

            <div v-if="formError" class="mb-3 p-2.5 rounded-lg bg-red-50 dark:bg-red-900/20 text-xs text-red-600 dark:text-red-400">
              {{ formError }}
            </div>

            <div class="space-y-3">
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1">Name</label>
                <input
                  v-model="holidayForm.name"
                  type="text"
                  maxlength="100"
                  class="w-full px-3 py-2 text-sm bg-white dark:bg-[#0F172A] border border-gray-200 dark:border-white/[0.08] rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                />
              </div>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1">Date</label>
                <input
                  v-model="holidayForm.date"
                  type="date"
                  class="w-full px-3 py-2 text-sm bg-white dark:bg-[#0F172A] border border-gray-200 dark:border-white/[0.08] rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                />
              </div>
              <label class="flex items-center gap-2 cursor-pointer select-none">
                <input v-model="holidayForm.isRecurring" type="checkbox" class="rounded" />
                <span class="text-sm text-gray-600 dark:text-gray-300">Repeats every year</span>
              </label>
            </div>

            <div class="flex justify-end gap-2 mt-5">
              <button
                @click="showFormModal = false"
                class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                Cancel
              </button>
              <button
                @click="saveHoliday"
                :disabled="formSaving"
                class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-60 transition-colors"
              >
                <LoadingSpinner v-if="formSaving" size="sm" />
                {{ formSaving ? "Saving..." : "Save" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ── Delete Confirmation Modal ──────────────────────────────────────── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showDeleteModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showDeleteModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-sm border border-gray-100 dark:border-white/[0.08] p-6"
          >
            <div class="flex items-center gap-3 mb-3">
              <div class="w-10 h-10 rounded-full bg-red-100 dark:bg-red-900/20 flex items-center justify-center flex-shrink-0">
                <Trash2 :size="18" class="text-red-500" />
              </div>
              <div>
                <h3 class="font-bold text-[#0F172A] dark:text-white">Delete Holiday?</h3>
                <p class="text-xs text-gray-400">This cannot be undone.</p>
              </div>
            </div>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">
              Remove <span class="font-semibold">{{ deleteTarget?.name }}</span> from the holiday calendar?
            </p>
            <div class="flex justify-end gap-2">
              <button
                @click="showDeleteModal = false"
                class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                Cancel
              </button>
              <button
                @click="handleDelete"
                :disabled="deleting"
                class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-red-500 text-white text-sm font-semibold hover:bg-red-600 disabled:opacity-60 transition-colors"
              >
                <LoadingSpinner v-if="deleting" size="sm" />
                {{ deleting ? "Deleting..." : "Delete" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import {
  Users,
  User,
  RefreshCw,
  ShieldCheck,
  Headphones,
  ChevronLeft,
  ChevronRight,
  Plus,
  Pencil,
  Trash2,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import StatCard from "../../components/dashboard/StatCard.vue";
import LoadingSpinner from "../../components/ui/LoadingSpinner.vue";
import { adminApi } from "../../api/adminApi";
import { holidayApi } from "../../api/holidayApi";
import { useToastStore } from "../../store/toast";
import { getNavLinks } from "../../config/navLinks";

const toastStore = useToastStore();

const navLinks = getNavLinks("Admin");

// ── System Info ─────────────────────────────────────────────────────────────
const systemInfo = ref(null);
const infoLoading = ref(true);

async function fetchSystemInfo() {
  infoLoading.value = true;
  try {
    const res = await adminApi.getSystemInfo();
    systemInfo.value = res.data;
  } catch {
    toastStore.show("Failed to load system info.", "error");
  } finally {
    infoLoading.value = false;
  }
}

function formatBytes(bytes) {
  if (!bytes) return "0 B";
  const units = ["B", "KB", "MB", "GB"];
  let i = 0;
  let val = bytes;
  while (val >= 1024 && i < units.length - 1) {
    val /= 1024;
    i++;
  }
  return `${val.toFixed(val < 10 && i > 0 ? 1 : 0)} ${units[i]}`;
}

function formatDateTime(dateStr) {
  if (!dateStr) return "";
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr) ? dateStr : dateStr + "Z";
  return new Date(normalized).toLocaleString("en-US", {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

// ── Calendar ─────────────────────────────────────────────────────────────────
const today = new Date();
const currentMonth = ref(today.getMonth());
const currentYear = ref(today.getFullYear());
const weekdayLabels = ["S", "M", "T", "W", "T", "F", "S"];

const monthLabel = computed(() =>
  new Date(currentYear.value, currentMonth.value, 1).toLocaleDateString("en-US", {
    month: "long",
    year: "numeric",
  })
);

function changeMonth(delta) {
  let m = currentMonth.value + delta;
  let y = currentYear.value;
  if (m < 0) { m = 11; y -= 1; }
  if (m > 11) { m = 0; y += 1; }
  currentMonth.value = m;
  currentYear.value = y;
  selectedHoliday.value = null;
}

const yearHolidays = ref([]);
const holidaysLoading = ref(true);

async function fetchHolidays() {
  holidaysLoading.value = true;
  try {
    const res = await holidayApi.getHolidays(currentYear.value);
    yearHolidays.value = [...res.data].sort(
      (a, b) => new Date(a.date) - new Date(b.date)
    );
  } catch {
    toastStore.show("Failed to load holidays.", "error");
  } finally {
    holidaysLoading.value = false;
  }
}

watch(currentYear, fetchHolidays);

function parseDateOnly(dateStr) {
  // Backend sends date-only values without a timezone marker — parse the
  // calendar components directly so no local-timezone shift occurs.
  const [y, m, d] = dateStr.split("T")[0].split("-").map(Number);
  return new Date(y, m - 1, d);
}

const holidaysByDay = computed(() => {
  const map = new Map();
  for (const h of yearHolidays.value) {
    const d = parseDateOnly(h.date);
    if (d.getMonth() === currentMonth.value && d.getFullYear() === currentYear.value) {
      map.set(d.getDate(), h);
    }
  }
  return map;
});

const calendarCells = computed(() => {
  const firstOfMonth = new Date(currentYear.value, currentMonth.value, 1);
  const startPadding = firstOfMonth.getDay();
  const daysInMonth = new Date(currentYear.value, currentMonth.value + 1, 0).getDate();
  const daysInPrevMonth = new Date(currentYear.value, currentMonth.value, 0).getDate();

  const cells = [];

  for (let i = startPadding - 1; i >= 0; i--) {
    cells.push({
      key: `prev-${i}`,
      dayNumber: daysInPrevMonth - i,
      inCurrentMonth: false,
      holiday: null,
    });
  }

  for (let day = 1; day <= daysInMonth; day++) {
    cells.push({
      key: `cur-${day}`,
      dayNumber: day,
      inCurrentMonth: true,
      holiday: holidaysByDay.value.get(day) || null,
    });
  }

  let nextDay = 1;
  while (cells.length % 7 !== 0) {
    cells.push({
      key: `next-${nextDay}`,
      dayNumber: nextDay,
      inCurrentMonth: false,
      holiday: null,
    });
    nextDay++;
  }

  return cells;
});

const selectedHoliday = ref(null);
function selectHoliday(h) {
  selectedHoliday.value = selectedHoliday.value?.id === h.id ? null : h;
}

function formatDate(dateStr) {
  return parseDateOnly(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

// ── Add / Edit modal ──────────────────────────────────────────────────────
const showFormModal = ref(false);
const editingHoliday = ref(null);
const holidayForm = ref({ name: "", date: "", isRecurring: false });
const formError = ref("");
const formSaving = ref(false);

function openAddModal() {
  editingHoliday.value = null;
  holidayForm.value = { name: "", date: "", isRecurring: false };
  formError.value = "";
  showFormModal.value = true;
}

function openEditModal(h) {
  editingHoliday.value = h;
  holidayForm.value = {
    name: h.name,
    date: parseDateOnly(h.date).toISOString().split("T")[0],
    isRecurring: h.isRecurring,
  };
  formError.value = "";
  showFormModal.value = true;
}

async function saveHoliday() {
  formError.value = "";
  if (!holidayForm.value.name.trim() || !holidayForm.value.date) {
    formError.value = "Name and date are required.";
    return;
  }

  formSaving.value = true;
  try {
    if (editingHoliday.value) {
      await holidayApi.updateHoliday(editingHoliday.value.id, holidayForm.value);
      toastStore.show("Holiday updated.", "success");
    } else {
      const res = await holidayApi.createHoliday(holidayForm.value);
      toastStore.show(
        res.data.isPastDate ? "Holiday added (date is in the past)." : "Holiday added.",
        "success"
      );
    }
    showFormModal.value = false;
    selectedHoliday.value = null;
    await fetchHolidays();
  } catch (e) {
    formError.value = e.response?.data?.message || "Failed to save holiday.";
  } finally {
    formSaving.value = false;
  }
}

// ── Delete ─────────────────────────────────────────────────────────────────
const showDeleteModal = ref(false);
const deleteTarget = ref(null);
const deleting = ref(false);

function confirmDelete(h) {
  deleteTarget.value = h;
  showDeleteModal.value = true;
}

async function handleDelete() {
  deleting.value = true;
  try {
    await holidayApi.deleteHoliday(deleteTarget.value.id);
    showDeleteModal.value = false;
    selectedHoliday.value = null;
    toastStore.show("Holiday deleted.", "success");
    await fetchHolidays();
  } catch {
    toastStore.show("Failed to delete holiday.", "error");
  } finally {
    deleting.value = false;
  }
}

onMounted(() => {
  fetchSystemInfo();
  fetchHolidays();
});
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
