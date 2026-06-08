<template>
  <div class="relative">
    <div
      v-if="!activityLog.length"
      class="py-6 text-center text-gray-400 dark:text-gray-500 text-sm"
    >
      No activity yet
    </div>

    <div v-else class="space-y-0">
      <div
        v-for="(entry, i) in activityLog"
        :key="i"
        class="relative flex gap-3 pb-5 last:pb-0"
      >
        <!-- Connector line -->
        <div
          v-if="i < activityLog.length - 1"
          class="absolute left-[13px] top-7 bottom-0 w-px bg-gray-200 dark:bg-white/[0.06]"
        />

        <!-- Dot -->
        <div
          class="relative z-10 w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5"
          :class="entryColor(entry.action).bg"
        >
          <component
            :is="entryIcon(entry.action)"
            :size="12"
            :class="entryColor(entry.action).fg"
          />
        </div>

        <!-- Content -->
        <div class="flex-1 min-w-0 pt-0.5">
          <p
            class="text-[13px] font-medium text-[#0F172A] dark:text-gray-200 leading-snug"
          >
            {{ entry.action }}
          </p>

          <!-- Old → New value change -->
          <div
            v-if="entry.fromValue || entry.toValue"
            class="flex items-center gap-1.5 mt-1"
          >
            <span
              class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-medium bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 line-through"
            >
              {{ entry.fromValue || "—" }}
            </span>
            <svg
              width="10"
              height="10"
              viewBox="0 0 10 10"
              fill="none"
              class="text-gray-400 flex-shrink-0"
            >
              <path
                d="M1 5h8M6 2l3 3-3 3"
                stroke="currentColor"
                stroke-width="1.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
            </svg>
            <span
              class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-medium"
              :class="entryColor(entry.action).chip"
            >
              {{ entry.toValue || "—" }}
            </span>
          </div>

          <!-- Actor + relative time -->
          <p class="text-[11px] text-gray-400 dark:text-gray-500 mt-0.5">
            <span class="font-medium text-gray-500 dark:text-gray-400">{{
              entry.userName
            }}</span>
            <span class="mx-1">·</span>
            <span :title="formatFullDate(entry.loggedAt)">{{
              timeAgo(entry.loggedAt)
            }}</span>
          </p>

          <!-- Full date/time -->
          <p class="text-[10px] text-gray-300 dark:text-gray-600 mt-0.5">
            {{ formatFullDate(entry.loggedAt) }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import {
  PlusCircle,
  UserCheck,
  RefreshCw,
  AlertTriangle,
  MessageSquare,
  CheckCircle,
  XCircle,
  Activity,
} from "lucide-vue-next";

defineProps({
  activityLog: { type: Array, default: () => [] },
});

function entryColor(action) {
  const a = (action || "").toLowerCase();
  if (a.includes("creat") || a.includes("open"))
    return {
      bg: "bg-blue-100 dark:bg-blue-900/30",
      fg: "text-blue-600 dark:text-blue-400",
      chip: "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400",
    };
  if (a.includes("assign"))
    return {
      bg: "bg-purple-100 dark:bg-purple-900/30",
      fg: "text-purple-600 dark:text-purple-400",
      chip: "bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-400",
    };
  if (a.includes("resolv"))
    return {
      bg: "bg-green-100 dark:bg-green-900/30",
      fg: "text-green-600 dark:text-green-400",
      chip: "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400",
    };
  if (a.includes("clos"))
    return {
      bg: "bg-gray-100 dark:bg-white/10",
      fg: "text-gray-500 dark:text-gray-400",
      chip: "bg-gray-100 dark:bg-white/10 text-gray-600 dark:text-gray-400",
    };
  if (a.includes("escalat"))
    return {
      bg: "bg-orange-100 dark:bg-orange-900/30",
      fg: "text-orange-600 dark:text-orange-400",
      chip: "bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-400",
    };
  if (a.includes("comment"))
    return {
      bg: "bg-gray-100 dark:bg-white/10",
      fg: "text-gray-500 dark:text-gray-400",
      chip: "bg-gray-100 dark:bg-white/10 text-gray-600 dark:text-gray-400",
    };
  if (a.includes("status"))
    return {
      bg: "bg-teal-100 dark:bg-teal-900/30",
      fg: "text-teal-600 dark:text-teal-400",
      chip: "bg-teal-100 dark:bg-teal-900/30 text-teal-700 dark:text-teal-400",
    };
  return {
    bg: "bg-gray-100 dark:bg-white/10",
    fg: "text-gray-500 dark:text-gray-400",
    chip: "bg-gray-100 dark:bg-white/10 text-gray-600 dark:text-gray-400",
  };
}

function entryIcon(action) {
  const a = (action || "").toLowerCase();
  if (a.includes("creat") || a.includes("open")) return PlusCircle;
  if (a.includes("assign")) return UserCheck;
  if (a.includes("resolv")) return CheckCircle;
  if (a.includes("clos")) return XCircle;
  if (a.includes("escalat")) return AlertTriangle;
  if (a.includes("comment")) return MessageSquare;
  if (a.includes("status")) return RefreshCw;
  return Activity;
}

function timeAgo(dateStr) {
  if (!dateStr) return "";
  const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 1000);
  if (diff < 60) return "just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 86400 * 7) return `${Math.floor(diff / 86400)}d ago`;
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function formatFullDate(dateStr) {
  if (!dateStr) return "";
  return new Date(dateStr).toLocaleString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
</script>
