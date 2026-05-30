<template>
  <div class="min-h-screen bg-[#F1F5F9] flex items-center justify-center">
    <div class="text-center">
      <h1 class="text-2xl font-bold text-[#0F172A]">
        Welcome to the Dashboard
      </h1>
      <p class="text-gray-500 mt-2">
        You are logged in as {{ authStore.userRole }}
      </p>
      <button
        @click="handleLogout"
        class="mt-6 px-6 py-2 bg-[#14B8A6] text-white rounded-lg hover:bg-teal-600"
      >
        Logout
      </button>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from "../store/auth";
import { useRouter } from "vue-router";

const authStore = useAuthStore();
const router = useRouter();

function handleLogout() {
  authStore.logout();
  router.push("/login");
}
</script>
