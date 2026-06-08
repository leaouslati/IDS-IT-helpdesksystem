<template>
  <div :class="wrapperClass">
    <div :class="spinnerClass" />
  </div>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  size: { type: String, default: "md" },
  fullPage: { type: Boolean, default: false },
});

const sizeMap = {
  sm: "w-5 h-5 border-2",
  md: "w-9 h-9 border-[3px]",
  lg: "w-14 h-14 border-4",
};

const spinnerClass = computed(
  () =>
    `${
      sizeMap[props.size] || sizeMap.md
    } border-[#14B8A6] border-t-transparent rounded-full animate-spin`
);

const wrapperClass = computed(() =>
  props.fullPage
    ? "fixed inset-0 z-50 flex items-center justify-center bg-white/60 dark:bg-[#0F172A]/60 backdrop-blur-sm"
    : "flex items-center justify-center"
);
</script>
