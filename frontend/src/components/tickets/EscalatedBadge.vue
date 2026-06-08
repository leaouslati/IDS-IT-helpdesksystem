<template>
  <span
    v-if="isEscalated"
    class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400 whitespace-nowrap"
  >
    <Flame :size="10" />
    Escalated
  </span>
</template>

<script setup>
import { Flame } from "lucide-vue-next";

defineProps({
  isEscalated: { type: Boolean, default: false },
});
</script>
