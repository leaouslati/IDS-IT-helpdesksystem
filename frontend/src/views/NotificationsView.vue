<template>
  <AppLayout :navLinks="navLinks" pageTitle="Notifications">
    <div class="max-w-2xl mx-auto">
      <!-- Header -->
      <div class="flex items-center justify-between mb-5">
        <div>
          <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">
            Notifications
          </h1>
          <p class="text-sm text-gray-400 mt-0.5">
            {{
              notifStore.unreadCount > 0
                ? `${notifStore.unreadCount} unread`
                : "All caught up"
            }}
          </p>
        </div>
        <button
          v-if="notifStore.unreadCount > 0"
          @click="notifStore.markAllAsRead()"
          class="px-3 py-1.5 text-xs font-semibold text-[#14B8A6] border border-[#14B8A6]/30 rounded-lg hover:bg-[#14B8A6]/5 transition-colors flex items-center gap-1.5"
        >
          <CheckCheck :size="13" />
          Mark all read
        </button>
      </div>

      <!-- Filter tabs -->
      <div
        class="flex gap-1 bg-gray-100 dark:bg-white/5 rounded-lg p-1 mb-5 w-fit"
      >
        <button
          v-for="tab in tabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          class="px-4 py-1.5 rounded-md text-sm font-medium transition-colors"
          :class="
            activeTab === tab.key
              ? 'bg-white dark:bg-white/10 text-[#0F172A] dark:text-white shadow-sm'
              : 'text-gray-400 hover:text-gray-600 dark:hover:text-gray-200'
          "
        >
          {{ tab.label }}
          <span
            v-if="tab.key === 'unread' && notifStore.unreadCount > 0"
            class="ml-1.5 px-1.5 py-0.5 bg-red-500 text-white text-[10px] font-bold rounded-full"
          >
            {{ notifStore.unreadCount }}
          </span>
        </button>
      </div>

      <!-- Notification list -->
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.05] shadow-sm overflow-hidden"
      >
        <!-- Loading skeleton -->
        <template v-if="notifStore.loading && !notifStore.notifications.length">
          <div
            v-for="i in 5"
            :key="i"
            class="flex items-start gap-3 px-5 py-4 border-b border-gray-100 dark:border-white/[0.04] animate-pulse"
          >
            <div
              class="w-9 h-9 rounded-full bg-gray-200 dark:bg-white/10 flex-shrink-0"
            />
            <div class="flex-1 space-y-2">
              <div class="h-3 bg-gray-200 dark:bg-white/10 rounded w-40" />
              <div class="h-3 bg-gray-200 dark:bg-white/10 rounded w-64" />
              <div class="h-2 bg-gray-200 dark:bg-white/10 rounded w-20" />
            </div>
          </div>
        </template>

        <!-- Empty state -->
        <div
          v-else-if="!filteredNotifications.length"
          class="py-20 text-center"
        >
          <BellOff
            :size="40"
            class="text-gray-200 dark:text-gray-700 mx-auto mb-3"
          />
          <p class="text-sm font-medium text-gray-400 dark:text-gray-500">
            {{
              activeTab === "unread" ? "No unread notifications" : "No notifications yet"
            }}
          </p>
        </div>

        <!-- Items grouped by day -->
        <template v-else>
          <template
            v-for="group in groupedNotifications"
            :key="group.label"
          >
            <!-- Day separator -->
            <div
              class="px-5 py-2 bg-gray-50 dark:bg-white/[0.02] border-b border-gray-100 dark:border-white/[0.04]"
            >
              <span
                class="text-[11px] font-semibold text-gray-400 uppercase tracking-wider"
                >{{ group.label }}</span
              >
            </div>

            <!-- Notifications in this group -->
            <div
              v-for="n in group.items"
              :key="n.id"
              @click="handleClick(n)"
              class="flex items-start gap-4 px-5 py-4 cursor-pointer hover:bg-gray-50 dark:hover:bg-white/[0.02] transition-colors group border-b border-gray-100 dark:border-white/[0.04] last:border-b-0"
              :class="!n.isRead ? 'bg-teal-50/30 dark:bg-teal-900/5' : ''"
            >
              <!-- Type icon -->
              <div
                class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5"
                :class="iconBg(n.type)"
              >
                <component
                  :is="typeIcon(n.type)"
                  :size="15"
                  :class="iconColor(n.type)"
                />
              </div>

              <!-- Content -->
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-2">
                  <p
                    class="text-[13px] font-semibold text-[#0F172A] dark:text-white leading-snug"
                    :class="!n.isRead ? '' : 'font-medium'"
                  >
                    {{ n.title }}
                  </p>
                  <span class="text-[11px] text-gray-400 flex-shrink-0 mt-0.5">{{
                    timeAgo(n.createdAt)
                  }}</span>
                </div>
                <p
                  class="text-[12px] text-gray-500 dark:text-gray-400 mt-0.5 leading-relaxed"
                >
                  {{ n.message }}
                </p>
                <div
                  v-if="n.ticketId"
                  class="mt-1.5 text-[11px] text-[#14B8A6] font-medium opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  Click to view ticket →
                </div>
              </div>

              <!-- Unread indicator -->
              <div class="flex-shrink-0 flex flex-col items-end gap-2 mt-1">
                <div
                  v-if="!n.isRead"
                  class="w-2.5 h-2.5 rounded-full bg-[#14B8A6]"
                />
                <button
                  v-if="!n.isRead"
                  @click.stop="notifStore.markAsRead(n.id)"
                  class="text-[10px] text-gray-400 hover:text-[#14B8A6] transition-colors opacity-0 group-hover:opacity-100 whitespace-nowrap"
                >
                  Mark read
                </button>
              </div>
            </div>
          </template>
        </template>
      </div>

      <!-- Load more -->
      <div
        v-if="notifStore.hasMore && filteredNotifications.length"
        class="mt-4 text-center"
      >
        <button
          @click="loadMore"
          :disabled="notifStore.loading"
          class="px-5 py-2.5 rounded-lg border border-gray-200 dark:border-white/10 text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors disabled:opacity-50"
        >
          <span v-if="notifStore.loading">Loading...</span>
          <span v-else>Load more</span>
        </button>
      </div>
    </div>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import {
  FileText,
  Bell,
  BellOff,
  UserCheck,
  AlertTriangle,
  CheckCircle2,
  MessageSquare,
  Paperclip,
  CheckCheck,
  ShieldAlert,
  Flame,
} from "lucide-vue-next";
import { useRouter } from "vue-router";
import { useNotificationStore } from "../store/notification";
import AppLayout from "../components/layout/AppLayout.vue";
import { useNavLinks } from "../composables/useNavLinks";

const router = useRouter();
const notifStore = useNotificationStore();

const activeTab = ref("all");

const tabs = [
  { key: "all", label: "All" },
  { key: "unread", label: "Unread" },
];

const { navLinks } = useNavLinks();

const filteredNotifications = computed(() => {
  if (activeTab.value === "unread") {
    return notifStore.notifications.filter((n) => !n.isRead);
  }
  return notifStore.notifications;
});

const groupedNotifications = computed(() => {
  const groups = new Map();
  filteredNotifications.value.forEach((n) => {
    const label = dayLabel(n.createdAt);
    if (!groups.has(label)) groups.set(label, []);
    groups.get(label).push(n);
  });
  return Array.from(groups.entries()).map(([label, items]) => ({ label, items }));
});

function dayLabel(dateStr) {
  if (!dateStr) return "Unknown";
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr)
    ? dateStr
    : dateStr + "Z";
  const d = new Date(normalized);
  const now = new Date();
  const toDay = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const yesterday = new Date(toDay);
  yesterday.setDate(toDay.getDate() - 1);
  const dDay = new Date(d.getFullYear(), d.getMonth(), d.getDate());
  if (dDay.getTime() === toDay.getTime()) return "Today";
  if (dDay.getTime() === yesterday.getTime()) return "Yesterday";
  const opts =
    d.getFullYear() === now.getFullYear()
      ? { weekday: "short", month: "short", day: "numeric" }
      : { month: "short", day: "numeric", year: "numeric" };
  return d.toLocaleDateString("en-US", opts);
}

onMounted(async () => {
  if (!notifStore.notifications.length) {
    await notifStore.fetchNotifications(1);
  }
});

async function loadMore() {
  await notifStore.fetchNotifications(notifStore.page + 1);
}

async function handleClick(n) {
  if (!n.isRead) await notifStore.markAsRead(n.id);
  if (n.ticketId) router.push(`/tickets/${n.ticketId}`);
}

function typeIcon(type) {
  const map = {
    TicketCreated:   FileText,
    TicketAssigned:  UserCheck,
    TicketEscalated: AlertTriangle,
    TicketClosed:    CheckCircle2,
    CommentAdded:    MessageSquare,
    AttachmentAdded: Paperclip,
    CriticalTicket:  Flame,
    EscalationAlert: AlertTriangle,
    AccountLocked:   ShieldAlert,
  };
  return map[type] || Bell;
}

function iconBg(type) {
  const map = {
    TicketCreated:   "bg-blue-100 dark:bg-blue-900/30",
    TicketAssigned:  "bg-green-100 dark:bg-green-900/30",
    TicketEscalated: "bg-orange-100 dark:bg-orange-900/30",
    TicketClosed:    "bg-teal-100 dark:bg-teal-900/30",
    CommentAdded:    "bg-blue-100 dark:bg-blue-900/30",
    AttachmentAdded: "bg-purple-100 dark:bg-purple-900/30",
    CriticalTicket:  "bg-red-100 dark:bg-red-900/30",
    EscalationAlert: "bg-orange-100 dark:bg-orange-900/30",
    AccountLocked:   "bg-red-100 dark:bg-red-900/30",
  };
  return map[type] || "bg-gray-100 dark:bg-white/5";
}

function iconColor(type) {
  const map = {
    TicketCreated:   "text-blue-600 dark:text-blue-400",
    TicketAssigned:  "text-green-600 dark:text-green-400",
    TicketEscalated: "text-orange-600 dark:text-orange-400",
    TicketClosed:    "text-teal-600 dark:text-teal-400",
    CommentAdded:    "text-blue-600 dark:text-blue-400",
    AttachmentAdded: "text-purple-600 dark:text-purple-400",
    CriticalTicket:  "text-red-600 dark:text-red-400",
    EscalationAlert: "text-orange-600 dark:text-orange-400",
    AccountLocked:   "text-red-600 dark:text-red-400",
  };
  return map[type] || "text-gray-500 dark:text-gray-400";
}

function timeAgo(dateStr) {
  if (!dateStr) return "";
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr)
    ? dateStr
    : dateStr + "Z";
  const diff = Math.floor((Date.now() - new Date(normalized).getTime()) / 1000);
  if (diff < 60) return "Just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 86400 * 7) return `${Math.floor(diff / 86400)}d ago`;
  return new Date(normalized).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
}
</script>
