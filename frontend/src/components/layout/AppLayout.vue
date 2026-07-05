<template>
  <div class="flex h-screen overflow-hidden">
    <!-- Mobile overlay -->
    <Transition name="fade">
      <div
        v-if="sidebarOpen"
        class="fixed inset-0 bg-black/60 z-20 md:hidden"
        @click="sidebarOpen = false"
      />
    </Transition>

    <!-- Sidebar -->
    <aside
      class="fixed inset-y-0 left-0 w-[220px] bg-[#1A1D2E] flex flex-col z-30 transform transition-transform duration-300 ease-in-out md:translate-x-0 shadow-xl"
      :class="sidebarOpen ? 'translate-x-0' : '-translate-x-full'"
    >
      <!-- Brand -->
      <div
        class="flex items-center gap-3 px-5 py-5 border-b border-white/[0.08]"
      >
        <div
          class="w-8 h-8 bg-[#14B8A6] rounded-lg flex items-center justify-center shadow-sm flex-shrink-0"
        >
          <Headphones :size="15" class="text-white" />
        </div>
        <div>
          <h1
            class="text-sm font-bold text-[#14B8A6] leading-tight tracking-wide"
          >
            HelpDesk
          </h1>
          <p class="text-[10px] text-gray-500 leading-tight">IT Management</p>
        </div>
      </div>

      <!-- Nav links -->
      <nav class="flex-1 px-3 py-4 space-y-0.5 overflow-y-auto">
        <router-link
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="relative flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-200"
          :class="
            isActive(link.to)
              ? 'bg-[#14B8A6]/[0.15] text-[#14B8A6]'
              : 'text-gray-400 hover:bg-white/[0.06] hover:text-gray-200'
          "
        >
          <span
            v-if="isActive(link.to)"
            class="absolute left-0 inset-y-[7px] w-[3px] bg-[#14B8A6] rounded-r-full"
          />
          <component :is="link.icon" :size="17" class="flex-shrink-0" />
          <span>{{ link.label }}</span>
        </router-link>
      </nav>

      <!-- Bottom actions -->
      <div class="px-3 pb-4 pt-2 border-t border-white/[0.08] space-y-1.5">
        <button
          v-if="showCreateTicket"
          @click="router.push('/tickets/create')"
          class="w-full flex items-center justify-center gap-2 bg-[#14B8A6] hover:bg-teal-500 active:bg-teal-600 text-white px-4 py-2.5 rounded-lg text-sm font-semibold transition-colors duration-200 shadow-sm"
        >
          <PlusCircle :size="15" />
          <span>Create Ticket</span>
        </button>
        <button
          @click="handleLogout"
          class="w-full flex items-center justify-center gap-2 text-gray-500 hover:text-gray-200 hover:bg-white/[0.06] px-4 py-2 rounded-lg text-sm transition-colors duration-200"
        >
          <LogOut :size="15" />
          <span>Logout</span>
        </button>
      </div>
    </aside>

    <!-- Main wrapper -->
    <div class="flex flex-col flex-1 min-w-0 md:ml-[220px] overflow-hidden">
      <!-- Top navbar -->
      <header
        class="h-[60px] bg-white dark:bg-[#1A1D2E] border-b border-gray-200 dark:border-white/[0.08] flex items-center px-4 md:px-6 gap-3 flex-shrink-0 shadow-sm"
      >
        <!-- Mobile menu toggle -->
        <button
          @click="sidebarOpen = !sidebarOpen"
          class="md:hidden p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
        >
          <Menu :size="20" />
        </button>

        <!-- Title -->
        <div class="flex-1 min-w-0">
          <h2
            class="font-bold text-[#0F172A] dark:text-white text-[15px] leading-none truncate"
          >
            {{ pageTitle }}
          </h2>
        </div>

        <!-- Right actions -->
        <div class="flex items-center gap-1">
          <!-- Notification bell + dropdown -->
          <div ref="notifRef" class="relative">
            <button
              @click="toggleNotif"
              class="relative p-2 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/[0.06] transition-colors"
              title="Notifications"
            >
              <Bell :size="17" />
              <span
                v-if="notifStore.unreadCount > 0"
                class="absolute top-1 right-1 min-w-[16px] h-4 bg-red-500 text-white text-[9px] font-bold rounded-full flex items-center justify-center px-[3px] leading-none"
              >
                {{ notifStore.unreadCount > 9 ? "9+" : notifStore.unreadCount }}
              </span>
            </button>

            <!-- Dropdown panel -->
            <Transition name="dropdown">
              <div
                v-if="notifOpen"
                class="absolute right-0 top-full mt-2 w-[340px] bg-white dark:bg-[#1A1D2E] rounded-xl shadow-xl border border-gray-100 dark:border-white/[0.08] overflow-hidden z-50"
              >
                <!-- Header -->
                <div
                  class="flex items-center justify-between px-4 py-3 border-b border-gray-100 dark:border-white/[0.06]"
                >
                  <span
                    class="text-[13px] font-semibold text-[#0F172A] dark:text-white"
                    >Notifications</span
                  >
                  <button
                    v-if="notifStore.unreadCount > 0"
                    @click="notifStore.markAllAsRead()"
                    class="text-[11px] text-[#14B8A6] font-semibold hover:text-teal-600 transition-colors"
                  >
                    Mark all read
                  </button>
                </div>

                <!-- Notification items -->
                <div
                  class="divide-y divide-gray-100 dark:divide-white/[0.04] max-h-[320px] overflow-y-auto"
                >
                  <!-- No notifications at all -->
                  <div
                    v-if="!notifStore.notifications.length"
                    class="py-10 text-center text-sm text-gray-400 dark:text-gray-500"
                  >
                    No notifications yet
                  </div>

                  <!-- All caught up — nothing unread -->
                  <div
                    v-else-if="notifStore.unreadCount === 0"
                    class="py-9 flex flex-col items-center gap-2 px-4"
                  >
                    <div
                      class="w-10 h-10 rounded-full bg-teal-100 dark:bg-teal-900/30 flex items-center justify-center"
                    >
                      <CheckCheck
                        :size="18"
                        class="text-teal-600 dark:text-teal-400"
                      />
                    </div>
                    <p
                      class="text-[13px] font-semibold text-[#0F172A] dark:text-white"
                    >
                      All caught up!
                    </p>
                    <p class="text-[11px] text-gray-400">
                      You've read all your notifications
                    </p>
                  </div>

                  <!-- Unread notifications only -->
                  <template v-else>
                    <div
                      v-for="n in notifStore.notifications.filter(n => !n.isRead).slice(0, 5)"
                      :key="n.id"
                      @click="handleNotifClick(n)"
                      class="flex items-start gap-3 px-4 py-3 cursor-pointer bg-teal-50/40 dark:bg-teal-900/5 hover:bg-teal-50 dark:hover:bg-teal-900/10 transition-colors"
                    >
                      <div
                        class="w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5"
                        :class="notifIconBg(n.type)"
                      >
                        <component
                          :is="notifIcon(n.type)"
                          :size="13"
                          :class="notifIconColor(n.type)"
                        />
                      </div>
                      <div class="flex-1 min-w-0">
                        <p
                          class="text-[12px] font-semibold text-[#0F172A] dark:text-white leading-snug truncate"
                        >
                          {{ n.title }}
                        </p>
                        <p
                          class="text-[11px] text-gray-400 mt-0.5 leading-snug line-clamp-2"
                        >
                          {{ n.message }}
                        </p>
                        <p class="text-[10px] text-gray-400 mt-1">
                          {{ timeAgo(n.createdAt) }}
                        </p>
                      </div>
                      <div class="w-2 h-2 rounded-full bg-[#14B8A6] flex-shrink-0 mt-2" />
                    </div>
                  </template>
                </div>

                <!-- Footer -->
                <div
                  class="px-4 py-3 border-t border-gray-100 dark:border-white/[0.06]"
                >
                  <router-link
                    to="/notifications"
                    @click="notifOpen = false"
                    class="text-[12px] font-semibold text-[#14B8A6] hover:text-teal-600 transition-colors flex items-center justify-center gap-1"
                  >
                    View all notifications
                    <ChevronRight :size="12" />
                  </router-link>
                </div>
              </div>
            </Transition>
          </div>

          <button
            @click="toggleDark"
            class="p-2 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/[0.06] transition-colors"
            :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
          >
            <Sun v-if="isDark" :size="17" />
            <Moon v-else :size="17" />
          </button>

          <!-- User -->
          <div
            class="flex items-center gap-2.5 pl-3 ml-1 border-l border-gray-200 dark:border-white/[0.08]"
          >
            <router-link
              to="/profile"
              class="w-[34px] h-[34px] rounded-full bg-[#14B8A6]/20 flex items-center justify-center flex-shrink-0 ring-2 ring-[#14B8A6]/30 overflow-hidden"
              title="My Profile"
            >
              <img
                v-if="authStore.avatarUrl"
                :src="authStore.avatarUrl"
                alt="Profile"
                class="w-full h-full object-cover"
              />
              <span v-else class="text-[#14B8A6] font-bold text-xs">{{
                userInitials
              }}</span>
            </router-link>
            <div class="hidden md:block leading-tight">
              <p
                class="text-[13px] font-semibold text-[#0F172A] dark:text-white leading-none mb-0.5"
              >
                {{ authStore.userName || "User" }}
              </p>
              <p
                class="text-[11px] text-gray-400 dark:text-gray-500 capitalize leading-none"
              >
                {{ authStore.userRole || "Role" }}
              </p>
            </div>
          </div>
        </div>
      </header>

      <!-- Scrollable content -->
      <main class="flex-1 overflow-y-auto bg-[#F1F5F9] dark:bg-[#0F172A]">
        <div class="p-4 md:p-6">
          <slot />
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "../../store/auth";
import { useNotificationStore } from "../../store/notification";
import { profileApi } from "../../api/profileApi";
import {
  Headphones,
  PlusCircle,
  LogOut,
  Menu,
  Bell,
  Sun,
  Moon,
  FileText,
  UserCheck,
  AlertTriangle,
  CheckCircle2,
  MessageSquare,
  Paperclip,
  ChevronRight,
  CheckCheck,
} from "lucide-vue-next";

defineProps({
  navLinks: { type: Array, default: () => [] },
  pageTitle: { type: String, default: "Dashboard" },
  notificationCount: { type: Number, default: 0 },
});

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const notifStore = useNotificationStore();

const showCreateTicket = computed(() =>
  ["Employee", "Manager"].includes(authStore.userRole)
);

const sidebarOpen = ref(false);
const isDark = ref(false);
const notifOpen = ref(false);
const notifRef = ref(null);

onMounted(() => {
  const saved = localStorage.getItem("theme");
  if (saved === "dark") {
    isDark.value = true;
  } else if (saved === "light") {
    isDark.value = false;
  } else {
    isDark.value = window.matchMedia("(prefers-color-scheme: dark)").matches;
  }
  document.documentElement.classList.toggle("dark", isDark.value);

  notifStore.connectToHub();
  document.addEventListener("click", onClickOutside, true);

  if (!authStore.avatarUrl) {
    profileApi
      .getPictureBlob()
      .then((res) => authStore.setAvatar(URL.createObjectURL(res.data)))
      .catch(() => {
        // No profile picture set — fall back to initials
      });
  }
});

onUnmounted(() => {
  document.removeEventListener("click", onClickOutside, true);
});

function onClickOutside(e) {
  if (notifRef.value && !notifRef.value.contains(e.target)) {
    notifOpen.value = false;
  }
}

function toggleNotif() {
  notifOpen.value = !notifOpen.value;
}

function toggleDark() {
  isDark.value = !isDark.value;
  document.documentElement.classList.toggle("dark", isDark.value);
  localStorage.setItem("theme", isDark.value ? "dark" : "light");
}

function isActive(path) {
  if (route.path === path) return true;
  if (path === "/tickets" && route.path.startsWith("/tickets")) return true;
  return false;
}

const userInitials = computed(() => {
  const name = authStore.userName || "";
  return name.slice(0, 2).toUpperCase() || "U";
});

function handleLogout() {
  authStore.logout();
  router.push("/login");
}

async function handleNotifClick(n) {
  notifOpen.value = false;
  if (!n.isRead) notifStore.markAsRead(n.id);
  if (n.ticketId) router.push(`/tickets/${n.ticketId}`);
  else router.push("/notifications");
}

function notifIcon(type) {
  const map = {
    TicketCreated: FileText,
    TicketAssigned: UserCheck,
    TicketEscalated: AlertTriangle,
    TicketClosed: CheckCircle2,
    CommentAdded: MessageSquare,
    AttachmentAdded: Paperclip,
  };
  return map[type] || Bell;
}

function notifIconBg(type) {
  const map = {
    TicketCreated: "bg-blue-100 dark:bg-blue-900/30",
    TicketAssigned: "bg-green-100 dark:bg-green-900/30",
    TicketEscalated: "bg-orange-100 dark:bg-orange-900/30",
    TicketClosed: "bg-teal-100 dark:bg-teal-900/30",
    CommentAdded: "bg-blue-100 dark:bg-blue-900/30",
    AttachmentAdded: "bg-purple-100 dark:bg-purple-900/30",
  };
  return map[type] || "bg-gray-100 dark:bg-white/5";
}

function notifIconColor(type) {
  const map = {
    TicketCreated: "text-blue-600 dark:text-blue-400",
    TicketAssigned: "text-green-600 dark:text-green-400",
    TicketEscalated: "text-orange-600 dark:text-orange-400",
    TicketClosed: "text-teal-600 dark:text-teal-400",
    CommentAdded: "text-blue-600 dark:text-blue-400",
    AttachmentAdded: "text-purple-600 dark:text-purple-400",
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
  return `${Math.floor(diff / 86400)}d ago`;
}
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
