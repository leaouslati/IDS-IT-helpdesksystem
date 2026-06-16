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

        <!-- Title + greeting -->
        <div class="flex-1 min-w-0">
          <h2
            class="font-bold text-[#0F172A] dark:text-white text-[15px] leading-none truncate"
          >
            {{ pageTitle }}
          </h2>
        </div>

        <!-- Right actions -->
        <div class="flex items-center gap-1">
          <button
            class="relative p-2 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/[0.06] transition-colors"
            title="Notifications"
          >
            <Bell :size="17" />
            <span
              v-if="notificationCount > 0"
              class="absolute top-1 right-1 min-w-[16px] h-4 bg-red-500 text-white text-[9px] font-bold rounded-full flex items-center justify-center px-[3px] leading-none"
            >
              {{ notificationCount > 9 ? "9+" : notificationCount }}
            </span>
          </button>

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
            <div
              class="w-[34px] h-[34px] rounded-full bg-[#14B8A6]/20 flex items-center justify-center flex-shrink-0 ring-2 ring-[#14B8A6]/30"
            >
              <span class="text-[#14B8A6] font-bold text-xs">{{
                userInitials
              }}</span>
            </div>
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
import { ref, computed, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "../../store/auth";
import {
  Headphones,
  PlusCircle,
  LogOut,
  Menu,
  Bell,
  Sun,
  Moon,
} from "lucide-vue-next";

defineProps({
  navLinks: { type: Array, default: () => [] },
  pageTitle: { type: String, default: "Dashboard" },
  notificationCount: { type: Number, default: 0 },
});

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const showCreateTicket = computed(() =>
  ["Employee", "Manager"].includes(authStore.userRole)
);

const sidebarOpen = ref(false);
const isDark = ref(false);

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
});

function toggleDark() {
  isDark.value = !isDark.value;
  document.documentElement.classList.toggle("dark", isDark.value);
  localStorage.setItem("theme", isDark.value ? "dark" : "light");
}

function isActive(path) {
  if (route.path === path) return true;
  // match /tickets and all sub-routes (/tickets/create, /tickets/:id)
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
</style>
