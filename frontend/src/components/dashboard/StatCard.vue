<template>
  <div
    class="bg-white dark:bg-[#1A1D2E] rounded-xl p-5 shadow-sm border-l-4 border border-gray-100 dark:border-white/[0.05] hover:-translate-y-1 transition-transform duration-200 cursor-default overflow-hidden relative"
    :style="{ borderLeftColor: color }"
  >
    <div class="flex items-start justify-between">
      <div class="min-w-0 flex-1">
        <p
          class="text-[11px] font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-widest truncate"
        >
          {{ title }}
        </p>
        <p
          class="text-3xl font-bold text-[#0F172A] dark:text-white mt-1.5 tabular-nums leading-none"
        >
          {{ value }}
        </p>
        <span
          v-if="badge"
          class="inline-flex items-center mt-2.5 px-2 py-0.5 rounded-full text-[10px] font-bold tracking-wide"
          :class="badgeClass"
        >
          {{ badge }}
        </span>
      </div>
      <div
        class="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0 ml-4"
        :style="{ backgroundColor: `${color}1a` }"
      >
        <component :is="icon" :size="20" :style="{ color }" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  title: { type: String, required: true },
  value: { type: [String, Number], default: 0 },
  color: { type: String, default: "#14B8A6" },
  icon: { type: Object, required: true },
  badge: { type: String, default: "" },
  badgeVariant: { type: String, default: "red" },
});

const badgeClass = computed(() => {
  const map = {
    red: "bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400",
    yellow:
      "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-600 dark:text-yellow-400",
    green:
      "bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400",
    blue: "bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400",
  };
  return map[props.badgeVariant] ?? map.red;
});
</script>
