<template>
  <span :class="badgeClass">{{ status }}</span>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  status: { type: String, default: "" },
});

const colorMap = {
  Open: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
  "In Progress":
    "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
  Pending:
    "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
  Resolved:
    "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
  Escalated:
    "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
};

const badgeClass = computed(() => {
  const base =
    "inline-flex items-center px-2.5 py-0.5 rounded-full text-[11px] font-semibold whitespace-nowrap";
  return `${base} ${colorMap[props.status] || colorMap["Open"]}`;
});
</script>
