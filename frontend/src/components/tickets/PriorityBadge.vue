<template>
  <span :class="badgeClass">{{ priority }}</span>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  priority: { type: String, default: "" },
});

const colorMap = {
  Low: "bg-gray-100 text-gray-600 dark:bg-gray-700/50 dark:text-gray-400",
  Medium:
    "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
  High: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
  Critical: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
};

const badgeClass = computed(() => {
  const base =
    "inline-flex items-center px-2.5 py-0.5 rounded-full text-[11px] font-semibold whitespace-nowrap";
  return `${base} ${colorMap[props.priority] || colorMap["Low"]}`;
});
</script>
