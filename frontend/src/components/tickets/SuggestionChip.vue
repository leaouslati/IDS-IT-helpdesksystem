<template>
  <div
    class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-medium border mt-1.5"
    :class="chipClass"
  >
    <Sparkles :size="11" />
    <span>AI suggests: <strong>{{ value }}</strong></span>
    <button
      v-if="!applied"
      type="button"
      @click="$emit('apply')"
      class="ml-0.5 font-semibold underline underline-offset-2 hover:opacity-70 transition-opacity"
    >
      Apply
    </button>
    <Check v-else :size="11" class="ml-0.5" />
  </div>
</template>

<script setup>
import { computed } from "vue";
import { Sparkles, Check } from "lucide-vue-next";

const props = defineProps({
  value: { type: String, required: true },
  confidence: { type: Number, default: 0 },
  applied: { type: Boolean, default: false },
});

defineEmits(["apply"]);

const chipClass = computed(() => {
  if (props.applied)
    return "bg-green-50 text-green-700 border-green-200 dark:bg-green-900/20 dark:text-green-400 dark:border-green-800";
  if (props.confidence < 0.6)
    return "bg-gray-100 text-gray-500 border-gray-200 dark:bg-gray-700/30 dark:text-gray-400 dark:border-gray-700";
  return "bg-teal-50 text-teal-700 border-teal-200 dark:bg-teal-900/20 dark:text-teal-400 dark:border-teal-800";
});
</script>
