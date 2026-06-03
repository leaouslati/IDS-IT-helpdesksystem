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
          Set a new password
        </p>
      </div>

      <!-- Success state -->
      <template v-if="success">
        <div class="text-center">
          <div
            class="w-14 h-14 bg-teal-100 dark:bg-teal-900/30 rounded-full flex items-center justify-center mx-auto mb-4"
          >
            <CheckCircle :size="28" class="text-[#14B8A6]" />
          </div>
          <h2 class="text-base font-bold text-[#0F172A] dark:text-white mb-2">
            Password updated!
          </h2>
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-6">
            Your password has been reset successfully.
          </p>
          <router-link
            to="/login"
            class="inline-block w-full bg-[#14B8A6] text-white py-2.5 rounded-lg font-semibold hover:bg-teal-600 transition text-center"
          >
            Go to Login
          </router-link>
        </div>
      </template>

      <!-- Form state -->
      <template v-else>
        <!-- Invalid/expired token warning -->
        <div
          v-if="tokenError"
          class="mb-5 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
        >
          <p class="text-sm text-red-600 dark:text-red-400">{{ tokenError }}</p>
        </div>

        <!-- General error -->
        <div
          v-if="errorMessage"
          class="mb-4 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
        >
          <p class="text-sm text-red-600 dark:text-red-400">
            {{ errorMessage }}
          </p>
        </div>

        <form @submit.prevent="handleReset">
          <!-- New Password -->
          <div class="mb-2">
            <label
              class="block text-sm font-medium text-[#0F172A] dark:text-gray-200 mb-1"
            >
              New Password
            </label>
            <div class="relative">
              <input
                v-model="newPassword"
                :type="showNew ? 'text' : 'password'"
                placeholder="Enter new password"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6] pr-10"
              />
              <button
                type="button"
                @click="showNew = !showNew"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#14B8A6]"
              >
                <Eye v-if="!showNew" :size="17" />
                <EyeOff v-else :size="17" />
              </button>
            </div>
          </div>

          <!-- Strength bar -->
          <div class="mb-4">
            <div class="flex gap-1 mb-1.5 mt-2">
              <div
                v-for="i in 5"
                :key="i"
                class="h-1.5 flex-1 rounded-full transition-colors duration-300"
                :class="
                  i <= strengthScore
                    ? strengthBarColor
                    : 'bg-gray-200 dark:bg-gray-700'
                "
              />
            </div>
            <p
              v-if="newPassword"
              class="text-xs font-medium"
              :style="{ color: strengthHex }"
            >
              {{ strengthLabel }}
            </p>
          </div>

          <!-- Confirm Password -->
          <div class="mb-6">
            <label
              class="block text-sm font-medium text-[#0F172A] dark:text-gray-200 mb-1"
            >
              Confirm Password
            </label>
            <div class="relative">
              <input
                v-model="confirmPassword"
                :type="showConfirm ? 'text' : 'password'"
                placeholder="Confirm your new password"
                class="w-full px-4 py-2 border rounded-lg text-sm bg-white dark:bg-[#0F172A] text-[#0F172A] dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6] pr-10 transition-colors"
                :class="
                  confirmPassword && !passwordsMatch
                    ? 'border-red-400 dark:border-red-600'
                    : 'border-gray-300 dark:border-gray-600'
                "
              />
              <button
                type="button"
                @click="showConfirm = !showConfirm"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#14B8A6]"
              >
                <Eye v-if="!showConfirm" :size="17" />
                <EyeOff v-else :size="17" />
              </button>
            </div>
            <p
              v-if="confirmPassword && !passwordsMatch"
              class="text-xs text-red-500 mt-1"
            >
              Passwords do not match.
            </p>
          </div>

          <button
            type="submit"
            :disabled="!canSubmit || loading"
            class="w-full bg-[#14B8A6] text-white py-2 rounded-lg font-medium hover:bg-teal-600 transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ loading ? "Resetting..." : "Reset Password" }}
          </button>
        </form>
      </template>

      <div class="text-center mt-6">
        <router-link to="/login" class="text-sm text-[#14B8A6] hover:underline">
          ← Back to Login
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from "vue";
import { useRoute } from "vue-router";
import { Eye, EyeOff, CheckCircle } from "lucide-vue-next";
import api from "../api/axios";

const route = useRoute();
const token = route.query.token || "";

const newPassword = ref("");
const confirmPassword = ref("");
const showNew = ref(false);
const showConfirm = ref(false);
const loading = ref(false);
const errorMessage = ref("");
const tokenError = ref(!token ? "Reset link is missing or invalid." : "");
const success = ref(false);

const requirements = computed(() => [
  { label: "At least 8 characters", met: newPassword.value.length >= 8 },
  { label: "One uppercase letter (A–Z)", met: /[A-Z]/.test(newPassword.value) },
  { label: "One lowercase letter (a–z)", met: /[a-z]/.test(newPassword.value) },
  { label: "One number (0–9)", met: /[0-9]/.test(newPassword.value) },
  {
    label: "One special character (!@#$...)",
    met: /[^A-Za-z0-9]/.test(newPassword.value),
  },
]);

const strengthScore = computed(
  () => requirements.value.filter((r) => r.met).length
);

const strengthLabel = computed(() => {
  const labels = ["", "Very Weak", "Weak", "Fair", "Good", "Strong"];
  return labels[strengthScore.value] || "";
});

const strengthHex = computed(() => {
  const colors = ["", "#EF4444", "#F97316", "#EAB308", "#3B82F6", "#10B981"];
  return colors[strengthScore.value] || "";
});

const strengthBarColor = computed(() => {
  const classes = [
    "",
    "bg-red-500",
    "bg-orange-500",
    "bg-yellow-500",
    "bg-blue-500",
    "bg-green-500",
  ];
  return classes[strengthScore.value] || "";
});

const passwordsMatch = computed(
  () => newPassword.value === confirmPassword.value
);

const canSubmit = computed(
  () =>
    strengthScore.value >= 3 &&
    passwordsMatch.value &&
    confirmPassword.value.length > 0 &&
    !!token
);

async function handleReset() {
  errorMessage.value = "";
  loading.value = true;
  try {
    await api.post("/auth/reset-password", {
      token,
      newPassword: newPassword.value,
    });
    success.value = true;
  } catch (e) {
    if (e.response?.status === 400) {
      tokenError.value =
        e.response.data?.message || "Reset link is invalid or has expired.";
    } else {
      errorMessage.value = "Something went wrong. Please try again.";
    }
  } finally {
    loading.value = false;
  }
}
</script>
