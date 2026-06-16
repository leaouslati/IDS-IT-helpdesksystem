<template>
  <div>
    <div
      v-if="!activityLog.length"
      class="py-8 text-center text-gray-400 dark:text-gray-500 text-sm"
    >
      No activity recorded yet
    </div>

    <div v-else class="space-y-6">
      <div v-for="group in groupedEntries" :key="group.dateKey">
        <!-- Date separator -->
        <div class="flex items-center gap-3 mb-3">
          <span
            class="text-[11px] font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 whitespace-nowrap"
          >
            {{ group.label }}
          </span>
          <div class="flex-1 h-px bg-gray-100 dark:bg-white/[0.06]" />
        </div>

        <!-- Entries -->
        <div class="space-y-0">
          <div
            v-for="(entry, i) in group.entries"
            :key="i"
            class="relative flex gap-3 pb-4 last:pb-0"
          >
            <!-- Connector line -->
            <div
              v-if="i < group.entries.length - 1"
              class="absolute left-[13px] top-7 bottom-0 w-px bg-gray-100 dark:bg-white/[0.06]"
            />

            <!-- Icon dot -->
            <div
              class="relative z-10 w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5"
              :class="entryStyle(entry).bg"
            >
              <component
                :is="entryIcon(entry)"
                :size="12"
                :class="entryStyle(entry).fg"
              />
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0 pt-0.5">
              <p
                class="text-[13px] font-medium text-[#0F172A] dark:text-gray-200 leading-snug"
              >
                {{ describe(entry) }}
              </p>

              <!-- fromValue → toValue change pills -->
              <div
                v-if="entry.fromValue || entry.toValue"
                class="flex items-center gap-1.5 mt-1.5 flex-wrap"
              >
                <span
                  class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 line-through"
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
                  class="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-medium"
                  :class="entryStyle(entry).chip"
                >
                  {{ entry.toValue || "—" }}
                </span>
              </div>

              <!-- Time -->
              <p class="text-[11px] text-gray-400 dark:text-gray-500 mt-0.5">
                {{ formatTime(entry.loggedAt) }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from "vue";
import {
  PlusCircle,
  UserCheck,
  RefreshCw,
  AlertTriangle,
  MessageSquare,
  CheckCircle,
  XCircle,
  Paperclip,
  Pencil,
  Activity,
} from "lucide-vue-next";

const props = defineProps({
  activityLog: { type: Array, default: () => [] },
});

function normalize(dateStr) {
  if (!dateStr) return "";
  return /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr) ? dateStr : dateStr + "Z";
}

function getLocalDateKey(dateStr) {
  const d = new Date(normalize(dateStr));
  return `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
}

function getDateLabel(dateStr) {
  const d = new Date(normalize(dateStr));
  const today = new Date();
  const yesterday = new Date();
  yesterday.setDate(today.getDate() - 1);
  if (d.toDateString() === today.toDateString()) return "Today";
  if (d.toDateString() === yesterday.toDateString()) return "Yesterday";
  return d.toLocaleDateString("en-US", {
    weekday: "long",
    month: "long",
    day: "numeric",
    year: "numeric",
  });
}

function formatTime(dateStr) {
  if (!dateStr) return "";
  return new Date(normalize(dateStr)).toLocaleTimeString("en-US", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

const groupedEntries = computed(() => {
  // Sort all entries newest-first so within each date group items are newest at top
  const sorted = [...props.activityLog].sort(
    (a, b) => new Date(normalize(b.loggedAt)) - new Date(normalize(a.loggedAt))
  );

  const map = new Map();
  for (const entry of sorted) {
    const key = getLocalDateKey(entry.loggedAt);
    if (!map.has(key)) {
      map.set(key, {
        dateKey: key,
        label: getDateLabel(entry.loggedAt),
        entries: [],
      });
    }
    map.get(key).entries.push(entry);
  }

  // Map preserves insertion order which is newest-date-first (since we sorted desc)
  return [...map.values()];
});

function describe(entry) {
  const u = entry.userName || "Someone";
  switch (entry.action) {
    case "Ticket Created":
      return `${u} submitted the ticket`;
    case "Ticket Updated": {
      const m = (entry.details || "").match(/Updated (.+?) on ticket/);
      return m ? `${u} updated ${m[1]}` : `${u} updated the ticket`;
    }
    case "Status Changed":
      return `${u} changed the status`;
    case "Agent Assigned":
      if (!entry.fromValue || entry.fromValue === "Unassigned") {
        return `${u} assigned the ticket to ${entry.toValue}`;
      }
      return `${u} reassigned the ticket from ${entry.fromValue} to ${entry.toValue}`;
    case "Comment Added":
      return `${u} added a comment`;
    case "Attachment Uploaded": {
      const m = (entry.details || "").match(/File '(.+?)' uploaded/);
      return m ? `${u} uploaded "${m[1]}"` : `${u} uploaded a file`;
    }
    case "Ticket Escalated":
      return `${u} escalated the ticket`;
    case "Ticket Deleted":
      return `${u} deleted the ticket`;
    default:
      return `${u} — ${entry.action}`;
  }
}

function entryStyle(entry) {
  const a = (entry.action || "").toLowerCase();
  if (a.includes("creat"))
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
      bg: "bg-teal-100 dark:bg-teal-900/30",
      fg: "text-teal-600 dark:text-teal-400",
      chip: "bg-teal-100 dark:bg-teal-900/30 text-teal-700 dark:text-teal-400",
    };
  if (a.includes("status"))
    return {
      bg: "bg-yellow-100 dark:bg-yellow-900/30",
      fg: "text-yellow-600 dark:text-yellow-400",
      chip: "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-400",
    };
  if (a.includes("upload") || a.includes("attach"))
    return {
      bg: "bg-indigo-100 dark:bg-indigo-900/30",
      fg: "text-indigo-600 dark:text-indigo-400",
      chip: "bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400",
    };
  if (a.includes("updat"))
    return {
      bg: "bg-gray-100 dark:bg-white/10",
      fg: "text-gray-500 dark:text-gray-400",
      chip: "bg-gray-100 dark:bg-white/10 text-gray-600 dark:text-gray-400",
    };
  return {
    bg: "bg-gray-100 dark:bg-white/10",
    fg: "text-gray-500 dark:text-gray-400",
    chip: "bg-gray-100 dark:bg-white/10 text-gray-600 dark:text-gray-400",
  };
}

function entryIcon(entry) {
  const a = (entry.action || "").toLowerCase();
  if (a.includes("creat")) return PlusCircle;
  if (a.includes("assign")) return UserCheck;
  if (a.includes("resolv")) return CheckCircle;
  if (a.includes("clos")) return XCircle;
  if (a.includes("escalat")) return AlertTriangle;
  if (a.includes("comment")) return MessageSquare;
  if (a.includes("status")) return RefreshCw;
  if (a.includes("upload") || a.includes("attach")) return Paperclip;
  if (a.includes("updat")) return Pencil;
  return Activity;
}
</script>
