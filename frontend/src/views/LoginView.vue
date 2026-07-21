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

      <!-- Lockout error -->
      <div
        v-if="isLocked"
        class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-start gap-2"
      >
        <LockIcon :size="15" class="text-red-500 flex-shrink-0 mt-0.5" />
        <p class="text-sm text-red-600 dark:text-red-400">
          Account locked. Too many failed attempts.<br />
          <span class="font-semibold"
            >Try again in {{ minutesRemaining }} minute{{
              minutesRemaining !== 1 ? "s" : ""
            }}.</span
          >
        </p>
      </div>

      <!-- General error -->
      <div
        v-else-if="errorMessage"
        class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
      >
        <p class="text-sm text-red-600 dark:text-red-400">{{ errorMessage }}</p>
      </div>

      <!-- Attempts warning (close to lockout) -->
      <div
        v-if="attemptsRemaining !== null && !isLocked"
        class="mb-4 p-3 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-700 rounded-lg flex items-center gap-2"
      >
        <AlertTriangle :size="14" class="text-amber-600 flex-shrink-0" />
        <p class="text-sm text-amber-700 dark:text-amber-400">
          {{ attemptsRemaining }} attempt{{
            attemptsRemaining !== 1 ? "s" : ""
          }}
          remaining before account is locked.
        </p>
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
            :disabled="isLocked"
            class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6] disabled:opacity-50"
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
              :disabled="isLocked"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6] pr-10 disabled:opacity-50"
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
          :disabled="loading || isLocked"
          class="w-full bg-[#14B8A6] text-white py-2 rounded-lg font-medium hover:bg-teal-600 transition disabled:opacity-50 disabled:cursor-not-allowed"
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
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "../store/auth";
import { getRoleDashboard } from "../router";
import { Eye, EyeOff, AlertTriangle, Lock as LockIcon } from "lucide-vue-next";

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const email = ref("");
const password = ref("");
const errorMessage = ref("");
const loading = ref(false);
const showPassword = ref(false);
const isLocked = ref(false);
const minutesRemaining = ref(0);
const attemptsRemaining = ref(null);

async function handleLogin() {
  loading.value = true;

  try {
    const role = await authStore.login(email.value, password.value);
    const redirect = route.query.redirect;
    // Only follow same-origin relative paths to avoid an open redirect.
    if (typeof redirect === "string" && redirect.startsWith("/") && !redirect.startsWith("//")) {
      router.push(redirect);
    } else {
      router.push(getRoleDashboard(role));
    }
  } catch (error) {
    const status = error.response?.status;
    const data = error.response?.data;

    if (status === 403 && data?.error === "ACCOUNT_DISABLED") {
      isLocked.value = false;
      errorMessage.value = data.message;
      attemptsRemaining.value = null;
    } else if (status === 423) {
      isLocked.value = true;
      errorMessage.value = "";
      attemptsRemaining.value = null;
      minutesRemaining.value = data?.minutesRemaining ?? 15;
    } else if (status === 401) {
      isLocked.value = false;
      errorMessage.value = "Invalid credentials.";
      attemptsRemaining.value = data?.attemptsRemaining ?? null;
    } else {
      errorMessage.value = "Unable to connect to the server.";
    }
  } finally {
    loading.value = false;
  }
}
</script>
