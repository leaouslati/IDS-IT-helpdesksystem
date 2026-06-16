<template>
  <div
    @click="navigate"
    class="group bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm cursor-pointer overflow-hidden"
    :class="ticket.isEscalated ? 'border-l-4 border-l-orange-400' : ''"
  >
    <!-- Top section -->
    <div class="p-4">
      <!-- Ref + copy + status row -->
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
        <div class="flex items-center gap-1.5 flex-shrink-0">
          <EscalatedBadge :isEscalated="ticket.isEscalated" />
          <StatusBadge :status="ticket.status" />
        </div>
      </div>

      <!-- Title -->
      <h3
        class="text-[14px] font-semibold text-[#0F172A] dark:text-white leading-snug mb-2 line-clamp-2 group-hover:text-[#14B8A6] transition-colors"
      >
        {{ ticket.title }}
      </h3>

      <!-- Priority + Category row (icon + text, no pills) -->
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-1">
          <component :is="priorityIcon" :size="12" :class="priorityIconClass" />
          <span class="text-[11px] text-gray-400 dark:text-gray-500">{{
            ticket.priority
          }}</span>
        </div>
        <div v-if="ticket.category" class="flex items-center gap-1">
          <Tag
            :size="12"
            class="text-gray-400 dark:text-gray-500 flex-shrink-0"
          />
          <span
            class="text-[11px] text-gray-400 dark:text-gray-500 truncate max-w-[120px]"
            >{{ ticket.category }}</span
          >
        </div>
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
import { computed } from "vue";
import { useRouter } from "vue-router";
import {
  Copy,
  User,
  MessageSquare,
  Flame,
  ArrowUp,
  Minus,
  ArrowDown,
  Tag,
} from "lucide-vue-next";
import StatusBadge from "./StatusBadge.vue";
import EscalatedBadge from "./EscalatedBadge.vue";

const props = defineProps({
  ticket: { type: Object, required: true },
});

const router = useRouter();

const priorityMap = {
  Critical: { icon: Flame, cls: "text-red-500" },
  High: { icon: ArrowUp, cls: "text-orange-500" },
  Medium: { icon: Minus, cls: "text-yellow-500 dark:text-yellow-400" },
  Low: { icon: ArrowDown, cls: "text-gray-400 dark:text-gray-500" },
};

const priorityIcon = computed(
  () => priorityMap[props.ticket.priority]?.icon ?? Minus
);
const priorityIconClass = computed(
  () => priorityMap[props.ticket.priority]?.cls ?? "text-gray-400"
);

function navigate() {
  router.push(`/tickets/${props.ticket.id}`);
}

function copyRef() {
  navigator.clipboard.writeText(props.ticket.referenceNumber).catch(() => {});
}

function formatDate(dateStr) {
  if (!dateStr) return "—";
  // Ensure UTC is parsed correctly when the backend omits the Z suffix
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr)
    ? dateStr
    : dateStr + "Z";
  const d = new Date(normalized);
  const now = new Date();
  const diff = Math.floor((now - d) / 1000);
  if (diff < 60) return "Just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 86400 * 7) return `${Math.floor(diff / 86400)}d ago`;
  return d.toLocaleDateString("en-US", { month: "short", day: "numeric" });
}
</script>
