<template>
  <div
    class="min-h-screen flex items-center justify-center bg-[#F1F5F9] dark:bg-[#0F172A] transition-colors duration-300"
  >
    <div
      class="bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-lg p-10 w-full max-w-md mx-4"
    >
      <!-- Title -->
      <div class="mb-8 text-center">
        <h1 class="text-2xl font-bold text-[#14B8A6]">HelpDesk</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          Sign in to your account
        </p>
      </div>

      <!-- Error Message -->
      <div
        v-if="errorMessage"
        class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
      >
        <p class="text-sm text-red-600 dark:text-red-400">{{ errorMessage }}</p>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleLogin">
        <!-- Email -->
        <div class="mb-4">
          <label
            class="block text-sm font-medium text-[#0F172A] dark:text-gray-200 mb-1"
          >
            Email
          </label>
          <input
            v-model="email"
            type="email"
            placeholder="Email Address"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]"
          />
        </div>

        <!-- Password -->
        <div class="mb-2">
          <label
            class="block text-sm font-medium text-[#0F172A] dark:text-gray-200 mb-1"
          >
            Password
          </label>
          <div class="relative">
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Enter your password"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6] pr-10"
            />
            <button
              type="button"
              @click="showPassword = !showPassword"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#14B8A6]"
            >
              <Eye v-if="!showPassword" :size="18" />
              <EyeOff v-else :size="18" />
            </button>
          </div>
        </div>

        <!-- Forgot Password -->
        <div class="text-right mb-6">
          <router-link
            to="/forgot-password"
            class="text-sm text-[#14B8A6] hover:underline"
          >
            Forgot Your Password?
          </router-link>
        </div>

        <!-- Submit Button -->
        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-[#14B8A6] text-white py-2 rounded-lg font-medium hover:bg-teal-600 transition disabled:opacity-50"
        >
          {{ loading ? "Signing in..." : "Sign In" }}
        </button>

        <!-- Footer Note -->
        <p class="text-center text-xs text-gray-400 dark:text-gray-500 mt-6">
          Internal use only. Contact your admin for access
        </p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../store/auth";
import { getRoleDashboard } from "../router";
import { Eye, EyeOff } from "lucide-vue-next";

const router = useRouter();
const authStore = useAuthStore();

const email = ref("");
const password = ref("");
const errorMessage = ref("");
const loading = ref(false);
const showPassword = ref(false);

async function handleLogin() {
  errorMessage.value = "";
  loading.value = true;

  try {
    const role = await authStore.login(email.value, password.value);
    router.push(getRoleDashboard(role));
  } catch (error) {
    errorMessage.value = "Invalid credentials. Please try again.";
  } finally {
    loading.value = false;
  }
}
</script>
