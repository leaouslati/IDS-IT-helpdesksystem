<template>
  <AppLayout :navLinks="navLinks" pageTitle="My Profile">
    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-[60vh]">
      <div class="flex flex-col items-center gap-4">
        <div
          class="w-12 h-12 border-4 border-[#14B8A6] border-t-transparent rounded-full animate-spin"
        />
        <p class="text-sm text-gray-400">Loading profile...</p>
      </div>
    </div>

    <!-- Error -->
    <div
      v-else-if="loadError"
      class="flex items-center justify-center min-h-[60vh]"
    >
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl p-10 text-center max-w-sm shadow-sm border border-gray-100 dark:border-white/[0.05]"
      >
        <AlertCircle :size="44" class="text-red-500 mx-auto mb-3" />
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">
          {{ loadError }}
        </p>
        <button
          @click="fetchProfile"
          class="px-5 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors"
        >
          Try Again
        </button>
      </div>
    </div>

    <!-- Content -->
    <template v-else>
      <div class="mb-6">
        <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">
          My Profile
        </h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
          Manage your account details and password
        </p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
        <!-- ── Left: Summary card ───────────────────────────────────────── -->
        <div
          class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-6 flex flex-col items-center text-center lg:sticky lg:top-6"
        >
          <div
            class="w-24 h-24 rounded-full bg-[#14B8A6]/20 flex items-center justify-center flex-shrink-0 overflow-hidden ring-4 ring-[#14B8A6]/20"
          >
            <img
              v-if="authStore.avatarUrl"
              :src="authStore.avatarUrl"
              alt="Profile"
              class="w-full h-full object-cover"
            />
            <span v-else class="text-[#14B8A6] font-bold text-2xl">{{
              initials
            }}</span>
          </div>

          <h3 class="mt-4 text-base font-bold text-[#0F172A] dark:text-white">
            {{ form.firstName }} {{ form.lastName }}
          </h3>
          <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5 break-all">
            {{ form.email }}
          </p>
          <span
            class="inline-flex items-center mt-3 px-3 py-1 rounded-full bg-[#14B8A6]/10 text-[#14B8A6] text-xs font-semibold"
          >
            {{ profile.role }}
          </span>

          <div class="w-full border-t border-gray-100 dark:border-white/[0.06] my-5" />

          <input
            ref="fileInput"
            type="file"
            accept="image/*"
            class="hidden"
            @change="onPictureSelected"
          />
          <button
            type="button"
            :disabled="pictureBusy"
            @click="fileInput.click()"
            class="w-full inline-flex items-center justify-center gap-1.5 px-3 py-2 rounded-lg bg-gray-100 dark:bg-white/5 text-xs font-semibold text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-white/10 transition-colors disabled:opacity-50"
          >
            <Camera :size="13" />
            Upload picture
          </button>
          <button
            v-if="authStore.avatarUrl"
            type="button"
            :disabled="pictureBusy"
            @click="removePicture"
            class="w-full mt-2 inline-flex items-center justify-center gap-1.5 px-3 py-2 rounded-lg bg-red-50 dark:bg-red-900/10 text-xs font-semibold text-red-500 hover:bg-red-100 dark:hover:bg-red-900/20 transition-colors disabled:opacity-50"
          >
            <Trash2 :size="13" />
            Remove picture
          </button>
        </div>

        <!-- ── Right: Editable sections ─────────────────────────────────── -->
        <div class="lg:col-span-2 space-y-6">
          <!-- Profile Info -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-6"
          >
            <h2 class="text-sm font-bold text-[#0F172A] dark:text-white mb-5">
              Profile Info
            </h2>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >First Name</label
                >
                <input
                  v-model="form.firstName"
                  type="text"
                  class="w-full px-3 py-2 text-sm bg-white dark:bg-[#0F172A] border rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                  :class="
                    fieldErrors.firstName
                      ? 'border-red-400 dark:border-red-600'
                      : 'border-gray-200 dark:border-white/[0.08]'
                  "
                />
                <p v-if="fieldErrors.firstName" class="text-xs text-red-500 mt-1">
                  {{ fieldErrors.firstName }}
                </p>
              </div>
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >Last Name</label
                >
                <input
                  v-model="form.lastName"
                  type="text"
                  class="w-full px-3 py-2 text-sm bg-white dark:bg-[#0F172A] border rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                  :class="
                    fieldErrors.lastName
                      ? 'border-red-400 dark:border-red-600'
                      : 'border-gray-200 dark:border-white/[0.08]'
                  "
                />
                <p v-if="fieldErrors.lastName" class="text-xs text-red-500 mt-1">
                  {{ fieldErrors.lastName }}
                </p>
              </div>
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >Email</label
                >
                <input
                  v-model="form.email"
                  type="email"
                  class="w-full px-3 py-2 text-sm bg-white dark:bg-[#0F172A] border rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                  :class="
                    fieldErrors.email
                      ? 'border-red-400 dark:border-red-600'
                      : 'border-gray-200 dark:border-white/[0.08]'
                  "
                />
                <p v-if="fieldErrors.email" class="text-xs text-red-500 mt-1">
                  {{ fieldErrors.email }}
                </p>
              </div>
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >Department</label
                >
                <select
                  v-model="form.departmentId"
                  class="w-full px-3 py-2 text-sm bg-white dark:bg-[#0F172A] border rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all cursor-pointer"
                  :class="
                    fieldErrors.departmentId
                      ? 'border-red-400 dark:border-red-600'
                      : 'border-gray-200 dark:border-white/[0.08]'
                  "
                >
                  <option :value="null">No department</option>
                  <option v-for="d in departments" :key="d.id" :value="d.id">
                    {{ d.name }}
                  </option>
                </select>
                <p
                  v-if="fieldErrors.departmentId"
                  class="text-xs text-red-500 mt-1"
                >
                  {{ fieldErrors.departmentId }}
                </p>
              </div>
            </div>

            <div class="flex justify-end mt-5">
              <button
                @click="saveProfile"
                :disabled="savingProfile"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-50 transition-colors"
              >
                <LoadingSpinner v-if="savingProfile" size="sm" />
                <Save v-else :size="14" />
                {{ savingProfile ? "Saving..." : "Save Changes" }}
              </button>
            </div>
          </div>

          <!-- Change Password -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] p-6"
          >
            <h2 class="text-sm font-bold text-[#0F172A] dark:text-white mb-5">
              Change Password
            </h2>

            <div
              v-if="pwSuccess"
              class="mb-4 p-3 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg text-sm text-green-600 dark:text-green-400"
            >
              Password changed successfully.
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >Current Password</label
                >
                <div class="relative">
                  <input
                    v-model="pwForm.currentPassword"
                    :type="showCurrent ? 'text' : 'password'"
                    class="w-full px-3 py-2 pr-9 text-sm bg-white dark:bg-[#0F172A] border rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                    :class="
                      pwError
                        ? 'border-red-400 dark:border-red-600'
                        : 'border-gray-200 dark:border-white/[0.08]'
                    "
                  />
                  <button
                    type="button"
                    @click="showCurrent = !showCurrent"
                    class="absolute right-2.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#14B8A6]"
                  >
                    <Eye v-if="!showCurrent" :size="14" />
                    <EyeOff v-else :size="14" />
                  </button>
                </div>
                <p v-if="pwError" class="text-xs text-red-500 mt-1">
                  {{ pwError }}
                </p>
              </div>
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >New Password</label
                >
                <div class="relative">
                  <input
                    v-model="pwForm.newPassword"
                    :type="showNew ? 'text' : 'password'"
                    class="w-full px-3 py-2 pr-9 text-sm bg-white dark:bg-[#0F172A] border border-gray-200 dark:border-white/[0.08] rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                  />
                  <button
                    type="button"
                    @click="showNew = !showNew"
                    class="absolute right-2.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#14B8A6]"
                  >
                    <Eye v-if="!showNew" :size="14" />
                    <EyeOff v-else :size="14" />
                  </button>
                </div>
                <p class="text-[11px] text-gray-400 mt-1">
                  8+ chars, upper, lower, number
                </p>
              </div>
              <div>
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1"
                  >Confirm New Password</label
                >
                <div class="relative">
                  <input
                    v-model="pwForm.confirmNewPassword"
                    :type="showConfirm ? 'text' : 'password'"
                    class="w-full px-3 py-2 pr-9 text-sm bg-white dark:bg-[#0F172A] border rounded-lg text-gray-800 dark:text-gray-200 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all"
                    :class="
                      pwForm.confirmNewPassword && !passwordsMatch
                        ? 'border-red-400 dark:border-red-600'
                        : 'border-gray-200 dark:border-white/[0.08]'
                    "
                  />
                  <button
                    type="button"
                    @click="showConfirm = !showConfirm"
                    class="absolute right-2.5 top-1/2 -translate-y-1/2 text-gray-400 hover:text-[#14B8A6]"
                  >
                    <Eye v-if="!showConfirm" :size="14" />
                    <EyeOff v-else :size="14" />
                  </button>
                </div>
                <p
                  v-if="pwForm.confirmNewPassword && !passwordsMatch"
                  class="text-xs text-red-500 mt-1"
                >
                  Passwords do not match.
                </p>
              </div>
            </div>

            <div class="flex justify-end mt-5">
              <button
                @click="changePassword"
                :disabled="pwSaving || !canSubmitPassword"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-50 transition-colors"
              >
                <LoadingSpinner v-if="pwSaving" size="sm" />
                <Lock v-else :size="14" />
                {{ pwSaving ? "Updating..." : "Change Password" }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>
  </AppLayout>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue";
import {
  AlertCircle,
  Camera,
  Trash2,
  Save,
  Lock,
  Eye,
  EyeOff,
} from "lucide-vue-next";
import AppLayout from "../components/layout/AppLayout.vue";
import LoadingSpinner from "../components/ui/LoadingSpinner.vue";
import { profileApi } from "../api/profileApi";
import { lookupApi } from "../api/lookupApi";
import { useAuthStore } from "../store/auth";
import { useToastStore } from "../store/toast";
import { useNavLinks } from "../composables/useNavLinks";

const authStore = useAuthStore();
const toastStore = useToastStore();

const { navLinks } = useNavLinks();

// ── Load profile ──────────────────────────────────────────────────────────
const profile = ref(null);
const departments = ref([]);
const loading = ref(true);
const loadError = ref("");

const form = reactive({
  firstName: "",
  lastName: "",
  email: "",
  departmentId: null,
});
const fieldErrors = ref({});
const savingProfile = ref(false);

async function fetchProfile() {
  loading.value = true;
  loadError.value = "";
  try {
    const [profileRes, deptRes] = await Promise.all([
      profileApi.getProfile(),
      lookupApi.getDepartments(),
    ]);
    profile.value = profileRes.data;
    departments.value = deptRes.data;

    form.firstName = profile.value.firstName;
    form.lastName = profile.value.lastName;
    form.email = profile.value.email;
    form.departmentId = profile.value.departmentId;
  } catch (e) {
    loadError.value = e.response?.data?.message || "Unable to load profile.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchProfile);

const initials = computed(() => {
  const f = form.firstName?.[0] ?? "";
  const l = form.lastName?.[0] ?? "";
  return (f + l).toUpperCase() || "U";
});

// ── Save profile ──────────────────────────────────────────────────────────
async function saveProfile() {
  fieldErrors.value = {};
  savingProfile.value = true;
  try {
    await profileApi.updateProfile({
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
      departmentId: form.departmentId,
    });
    authStore.updateUserInfo({
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
    });
    toastStore.show("Profile updated successfully.", "success");
  } catch (e) {
    if (e.response?.status === 400 && e.response.data?.errors) {
      fieldErrors.value = e.response.data.errors;
    } else {
      toastStore.show("Failed to update profile.", "error");
    }
  } finally {
    savingProfile.value = false;
  }
}

// ── Profile picture ───────────────────────────────────────────────────────
const fileInput = ref(null);
const pictureBusy = ref(false);

async function onPictureSelected(e) {
  const file = e.target.files?.[0];
  e.target.value = "";
  if (!file) return;

  pictureBusy.value = true;
  try {
    await profileApi.uploadPicture(file);
    const res = await profileApi.getPictureBlob();
    authStore.setAvatar(URL.createObjectURL(res.data));
    toastStore.show("Profile picture updated.", "success");
  } catch (e) {
    toastStore.show(
      e.response?.data?.message || "Failed to upload picture.",
      "error"
    );
  } finally {
    pictureBusy.value = false;
  }
}

async function removePicture() {
  pictureBusy.value = true;
  try {
    await profileApi.deletePicture();
    authStore.clearAvatar();
    toastStore.show("Profile picture removed.", "success");
  } catch {
    toastStore.show("Failed to remove picture.", "error");
  } finally {
    pictureBusy.value = false;
  }
}

// ── Change password ───────────────────────────────────────────────────────
const pwForm = reactive({
  currentPassword: "",
  newPassword: "",
  confirmNewPassword: "",
});
const showCurrent = ref(false);
const showNew = ref(false);
const showConfirm = ref(false);
const pwError = ref("");
const pwSaving = ref(false);
const pwSuccess = ref(false);

const passwordsMatch = computed(
  () => pwForm.newPassword === pwForm.confirmNewPassword
);

const isPasswordStrong = computed(
  () =>
    pwForm.newPassword.length >= 8 &&
    /[A-Z]/.test(pwForm.newPassword) &&
    /[a-z]/.test(pwForm.newPassword) &&
    /[0-9]/.test(pwForm.newPassword)
);

const canSubmitPassword = computed(
  () =>
    pwForm.currentPassword.length > 0 &&
    isPasswordStrong.value &&
    passwordsMatch.value
);

async function changePassword() {
  pwError.value = "";
  pwSuccess.value = false;
  pwSaving.value = true;
  try {
    await profileApi.changePassword({ ...pwForm });
    pwSuccess.value = true;
    pwForm.currentPassword = "";
    pwForm.newPassword = "";
    pwForm.confirmNewPassword = "";
  } catch (e) {
    pwError.value =
      e.response?.data?.message || "Failed to change password.";
  } finally {
    pwSaving.value = false;
  }
}
</script>
