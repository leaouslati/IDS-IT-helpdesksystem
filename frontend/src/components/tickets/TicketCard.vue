<template>
  <div
    @click="navigate"
    class="group bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 cursor-pointer overflow-hidden"
    :class="ticket.isEscalated ? 'border-l-4 border-l-orange-400' : ''"
  >
    <!-- Top section -->
    <div class="p-4">
      <!-- Ref + badges row -->
      <div class="flex items-start justify-between gap-2 mb-3">
        <div class="flex items-center gap-1.5">
          <span
            class="font-mono text-[11px] font-semibold text-gray-400 dark:text-gray-500 bg-gray-100 dark:bg-white/5 px-2 py-0.5 rounded"
          >
            {{ ticket.referenceNumber }}
          </span>
          <button
            @click.stop="copyRef"
            class="p-0.5 rounded text-gray-300 dark:text-gray-600 hover:text-gray-500 dark:hover:text-gray-400 transition-colors opacity-0 group-hover:opacity-100"
            title="Copy reference"
          >
            <Copy :size="11" />
          </button>
        </div>
        <EscalatedBadge :isEscalated="ticket.isEscalated" />
      </div>

      <!-- Title -->
      <h3
        class="text-[14px] font-semibold text-[#0F172A] dark:text-white leading-snug mb-3 line-clamp-2 group-hover:text-[#14B8A6] transition-colors"
      >
        {{ ticket.title }}
      </h3>

      <!-- Status + Priority badges -->
      <div class="flex flex-wrap items-center gap-1.5 mb-3">
        <StatusBadge :status="ticket.status" />
        <PriorityBadge :priority="ticket.priority" />
        <span
          v-if="ticket.category"
          class="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-medium bg-gray-100 text-gray-600 dark:bg-white/5 dark:text-gray-400"
        >
          {{ ticket.category }}
        </span>
      </div>
    </div>

    <!-- Bottom section -->
    <div
      class="px-4 py-3 bg-gray-50/50 dark:bg-white/[0.02] border-t border-gray-100 dark:border-white/[0.04] flex items-center justify-between gap-2"
    >
      <div class="flex items-center gap-1.5 min-w-0">
        <User :size="12" class="text-gray-400 flex-shrink-0" />
        <span
          class="text-[12px] truncate"
          :class="
            ticket.assignedTo
              ? 'text-gray-600 dark:text-gray-400'
              : 'text-gray-400 italic dark:text-gray-600'
          "
        >
          {{ ticket.assignedTo || "Unassigned" }}
        </span>
      </div>
      <div class="flex items-center gap-3 flex-shrink-0">
        <span class="flex items-center gap-1 text-[12px] text-gray-400">
          <MessageSquare :size="12" />
          {{ ticket.commentCount || 0 }}
        </span>
        <span class="text-[11px] text-gray-400 dark:text-gray-500">
          {{ formatDate(ticket.createdAt) }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from "vue-router";
import { Copy, User, MessageSquare } from "lucide-vue-next";
import StatusBadge from "./StatusBadge.vue";
import PriorityBadge from "./PriorityBadge.vue";
import EscalatedBadge from "./EscalatedBadge.vue";

const props = defineProps({
  ticket: { type: Object, required: true },
});

const router = useRouter();

function navigate() {
  router.push(`/tickets/${props.ticket.id}`);
}

function copyRef() {
  navigator.clipboard.writeText(props.ticket.referenceNumber).catch(() => {});
}

function formatDate(dateStr) {
  if (!dateStr) return "—";
  const d = new Date(dateStr);
  const now = new Date();
  const diff = Math.floor((now - d) / 1000);
  if (diff < 60) return "Just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 86400 * 7) return `${Math.floor(diff / 86400)}d ago`;
  return d.toLocaleDateString("en-US", { month: "short", day: "numeric" });
}
</script>
