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
          Reset your password
        </p>
      </div>

      <!-- Step 1: Enter email -->
      <template v-if="!resetToken">
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-6 text-center">
          Enter your account email and we'll generate a reset link for you.
        </p>

        <!-- Error -->
        <div
          v-if="errorMessage"
          class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
        >
          <p class="text-sm text-red-600 dark:text-red-400">
            {{ errorMessage }}
          </p>
        </div>

        <form @submit.prevent="handleSubmit">
          <div class="mb-5">
            <label
              class="block text-sm font-medium text-[#0F172A] dark:text-gray-200 mb-1"
            >
              Email Address
            </label>
            <input
              v-model="email"
              type="email"
              placeholder="your-email@example.com"
              required
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]"
            />
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-[#14B8A6] text-white py-2 rounded-lg font-medium hover:bg-teal-600 transition disabled:opacity-50"
          >
            {{ loading ? "Generating link..." : "Generate Reset Link" }}
          </button>
        </form>
      </template>

      <!-- Step 2: Show reset link -->
      <template v-else>
        <div class="text-center mb-6">
          <div
            class="w-14 h-14 bg-teal-100 dark:bg-teal-900/30 rounded-full flex items-center justify-center mx-auto mb-4"
          >
            <CheckCircle :size="28" class="text-[#14B8A6]" />
          </div>
          <h2 class="text-base font-bold text-[#0F172A] dark:text-white mb-1">
            Reset link ready
          </h2>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            This link expires in
            <span class="font-semibold text-amber-600 dark:text-amber-400">
              {{ countdown }}
            </span>
          </p>
        </div>

        <button
          @click="goToReset"
          class="w-full bg-[#14B8A6] text-white py-2.5 rounded-lg font-semibold hover:bg-teal-600 transition flex items-center justify-center gap-2"
        >
          <KeyRound :size="16" />
          Click here to reset your password
        </button>

        <p class="text-center text-xs text-gray-400 dark:text-gray-500 mt-4">
          Do not close or share this page with anyone.
        </p>
      </template>

      <!-- Back to login -->
      <div class="text-center mt-6">
        <router-link to="/login" class="text-sm text-[#14B8A6] hover:underline">
          ← Back to Login
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { CheckCircle, KeyRound } from "lucide-vue-next";
import api from "../api/axios";

const router = useRouter();

const email = ref("");
const loading = ref(false);
const errorMessage = ref("");
const resetToken = ref("");
const countdown = ref("15:00");

let countdownInterval = null;

async function handleSubmit() {
  errorMessage.value = "";
  loading.value = true;
  try {
    const res = await api.post("/auth/forgot-password", { email: email.value });
    resetToken.value = res.data.resetToken;
    startCountdown(15 * 60);
  } catch (e) {
    if (e.response?.status === 404) {
      errorMessage.value = "No active account found with that email address.";
    } else {
      errorMessage.value = "Something went wrong. Please try again.";
    }
  } finally {
    loading.value = false;
  }
}

function startCountdown(seconds) {
  let remaining = seconds;
  countdownInterval = setInterval(() => {
    remaining--;
    const m = String(Math.floor(remaining / 60)).padStart(2, "0");
    const s = String(remaining % 60).padStart(2, "0");
    countdown.value = `${m}:${s}`;
    if (remaining <= 0) {
      clearInterval(countdownInterval);
      countdown.value = "expired";
    }
  }, 1000);
}

function goToReset() {
  router.push(`/reset-password?token=${resetToken.value}`);
}

onUnmounted(() => {
  if (countdownInterval) clearInterval(countdownInterval);
});
</script>
