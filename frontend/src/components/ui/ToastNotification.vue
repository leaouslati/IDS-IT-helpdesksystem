<template>
  <Teleport to="body">
    <div
      class="fixed bottom-5 right-5 z-[9999] flex flex-col gap-2 pointer-events-none"
    >
      <TransitionGroup name="toast">
        <div
          v-for="toast in toastStore.toasts"
          :key="toast.id"
          class="pointer-events-auto flex items-start gap-3 min-w-[300px] max-w-[380px] px-4 py-3.5 rounded-xl shadow-lg border text-sm font-medium"
          :class="toastClass(toast.type)"
        >
          <component
            :is="toastIcon(toast.type)"
            :size="16"
            class="flex-shrink-0 mt-0.5"
          />
          <span class="flex-1 leading-snug">{{ toast.message }}</span>
          <button
            @click="toastStore.remove(toast.id)"
            class="flex-shrink-0 opacity-60 hover:opacity-100 transition-opacity"
          >
            <X :size="14" />
          </button>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup>
import { CheckCircle2, XCircle, AlertTriangle, Info, X } from "lucide-vue-next";
import { useToastStore } from "../../store/toast";

const toastStore = useToastStore();

function toastIcon(type) {
  return (
    {
      success: CheckCircle2,
      error: XCircle,
      warning: AlertTriangle,
      info: Info,
    }[type] || Info
  );
}

function toastClass(type) {
  return (
    {
      success:
        "bg-white dark:bg-[#1A1D2E] border-green-200 dark:border-green-800 text-green-800 dark:text-green-300",
      error:
        "bg-white dark:bg-[#1A1D2E] border-red-200 dark:border-red-800 text-red-800 dark:text-red-300",
      warning:
        "bg-white dark:bg-[#1A1D2E] border-yellow-200 dark:border-yellow-800 text-yellow-800 dark:text-yellow-300",
      info: "bg-white dark:bg-[#1A1D2E] border-blue-200 dark:border-blue-800 text-blue-800 dark:text-blue-300",
    }[type] ||
    "bg-white dark:bg-[#1A1D2E] border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300"
  );
}
</script>

<style scoped>
.toast-enter-active {
  transition: all 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.toast-leave-active {
  transition: all 0.2s ease-in;
}
.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
.toast-move {
  transition: transform 0.2s ease;
}
</style>
