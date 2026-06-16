<template>
  <div
    class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-4"
  >
    <div class="flex flex-wrap gap-3 items-center">
      <!-- Search -->
      <div class="relative flex-1 min-w-[200px]">
        <Search
          :size="15"
          class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
        />
        <input
          v-model="localFilters.search"
          type="text"
          placeholder="Search tickets..."
          class="w-full pl-9 pr-4 py-2 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
          @input="onSearchInput"
        />
      </div>

      <!-- Status filter -->
      <select
        v-model="localFilters.status"
        class="py-2 pl-3 pr-8 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all cursor-pointer appearance-none"
        @change="emitChange"
      >
        <option value="">All Statuses</option>
        <option v-for="s in statuses" :key="s.id || s.name" :value="s.name">
          {{ s.name }}
        </option>
      </select>

      <!-- Priority filter -->
      <select
        v-model="localFilters.priority"
        class="py-2 pl-3 pr-8 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all cursor-pointer appearance-none"
        @change="emitChange"
      >
        <option value="">All Priorities</option>
        <option v-for="p in priorities" :key="p.id || p.name" :value="p.name">
          {{ p.name }}
        </option>
      </select>

      <!-- Category filter -->
      <select
        v-model="localFilters.category"
        class="py-2 pl-3 pr-8 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all cursor-pointer appearance-none"
        @change="emitChange"
      >
        <option value="">All Categories</option>
        <option v-for="c in categories" :key="c.id || c.name" :value="c.name">
          {{ c.name }}
        </option>
      </select>

      <!-- Escalated toggle (hidden for Employee role) -->
      <button
        v-if="showEscalated"
        @click="toggleEscalated"
        class="inline-flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium border transition-all duration-150"
        :class="
          localFilters.isEscalated
            ? 'bg-orange-100 border-orange-300 text-orange-700 dark:bg-orange-900/30 dark:border-orange-700 dark:text-orange-400'
            : 'bg-gray-50 border-gray-200 text-gray-600 dark:bg-[#0F172A] dark:border-gray-700 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5'
        "
      >
        <Flame :size="13" />
        Escalated
      </button>

      <!-- Unassigned toggle (manager only) -->
      <button
        v-if="showUnassigned"
        @click="toggleUnassigned"
        class="inline-flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium border transition-all duration-150"
        :class="
          localFilters.isUnassigned
            ? 'bg-yellow-100 border-yellow-300 text-yellow-700 dark:bg-yellow-900/30 dark:border-yellow-700 dark:text-yellow-400'
            : 'bg-gray-50 border-gray-200 text-gray-600 dark:bg-[#0F172A] dark:border-gray-700 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5'
        "
      >
        <UserX :size="13" />
        Unassigned
      </button>

      <!-- Reset -->
      <button
        v-if="hasActiveFilters"
        @click="resetFilters"
        class="inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-all"
      >
        <RotateCcw :size="13" />
        Reset
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { Search, Flame, UserX, RotateCcw } from "lucide-vue-next";

let searchDebounceTimer = null;

defineProps({
  showUnassigned: { type: Boolean, default: false },
  showEscalated: { type: Boolean, default: true },
  statuses: { type: Array, default: () => [] },
  priorities: { type: Array, default: () => [] },
  categories: { type: Array, default: () => [] },
});

const emit = defineEmits(["filter-change"]);

const localFilters = ref({
  search: "",
  status: "",
  priority: "",
  category: "",
  isEscalated: false,
  isUnassigned: false,
});

const hasActiveFilters = computed(
  () =>
    localFilters.value.search ||
    localFilters.value.status ||
    localFilters.value.priority ||
    localFilters.value.category ||
    localFilters.value.isEscalated ||
    localFilters.value.isUnassigned
);

function emitChange() {
  emit("filter-change", { ...localFilters.value });
}

function onSearchInput() {
  clearTimeout(searchDebounceTimer);
  searchDebounceTimer = setTimeout(emitChange, 300);
}

function toggleEscalated() {
  localFilters.value.isEscalated = !localFilters.value.isEscalated;
  emitChange();
}

function toggleUnassigned() {
  localFilters.value.isUnassigned = !localFilters.value.isUnassigned;
  emitChange();
}

function resetFilters() {
  localFilters.value = {
    search: "",
    status: "",
    priority: "",
    category: "",
    isEscalated: false,
    isUnassigned: false,
  };
  emitChange();
}
</script>
