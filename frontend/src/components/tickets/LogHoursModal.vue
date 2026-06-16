<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="modelValue"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
      >
        <div
          class="absolute inset-0 bg-black/50 backdrop-blur-sm"
          @click="onDismiss"
        />
        <div
          class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-md p-6 border border-gray-100 dark:border-white/[0.08]"
        >
          <!-- Header -->
          <div class="flex items-center gap-3 mb-5">
            <div
              class="w-10 h-10 rounded-full bg-teal-100 dark:bg-teal-900/30 flex items-center justify-center flex-shrink-0"
            >
              <Clock :size="18" class="text-teal-600 dark:text-teal-400" />
            </div>
            <div>
              <h3 class="font-bold text-[#0F172A] dark:text-white">
                Log Hours Worked
              </h3>
              <p class="text-xs text-gray-400">
                Record the time you spent on this ticket
              </p>
            </div>
            <button
              @click="onDismiss"
              class="ml-auto p-1.5 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
            >
              <X :size="16" />
            </button>
          </div>

          <!-- Form -->
          <div class="space-y-4">
            <!-- Hours worked -->
            <div>
              <label
                class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
              >
                Hours Worked <span class="text-red-500">*</span>
              </label>
              <input
                v-model.number="form.hoursWorked"
                type="number"
                min="0.25"
                max="24"
                step="0.25"
                placeholder="e.g. 2.5"
                class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
                :class="{ 'border-red-400': errors.hoursWorked }"
              />
              <p v-if="errors.hoursWorked" class="text-xs text-red-500 mt-1">
                {{ errors.hoursWorked }}
              </p>
            </div>

            <!-- Date -->
            <div>
              <label
                class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
              >
                Date <span class="text-red-500">*</span>
              </label>
              <input
                v-model="form.logDate"
                type="date"
                :max="todayIso"
                class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
              />
            </div>

            <!-- Notes -->
            <div>
              <label
                class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
              >
                Notes
                <span class="text-gray-400 normal-case font-normal"
                  >(optional)</span
                >
              </label>
              <textarea
                v-model="form.notes"
                rows="3"
                placeholder="What did you work on?"
                class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all resize-none"
              />
            </div>

            <!-- Error -->
            <p v-if="submitError" class="text-sm text-red-500">
              {{ submitError }}
            </p>
          </div>

          <!-- Footer -->
          <div class="flex gap-2 mt-6">
            <button
              @click="onDismiss"
              class="flex-1 px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 border border-gray-200 dark:border-gray-700 transition-colors"
            >
              {{ isDismissable ? "Log Later" : "Cancel" }}
            </button>
            <button
              @click="submit"
              :disabled="submitting"
              class="flex-1 inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-60 transition-colors"
            >
              <span
                v-if="submitting"
                class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"
              />
              <Clock v-else :size="14" />
              {{ submitting ? "Saving..." : "Log Hours" }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { Clock, X } from "lucide-vue-next";
import { ticketApi } from "../../api/ticketApi";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  ticketId: { type: [String, Number], required: true },
  // When true the "Cancel" button is labelled "Log Later" (shown after ticket close)
  isDismissable: { type: Boolean, default: false },
});

const emit = defineEmits(["update:modelValue", "logged"]);

const todayIso = computed(() => new Date().toISOString().split("T")[0]);

const form = ref({
  hoursWorked: null,
  logDate: todayIso.value,
  notes: "",
});

const errors = ref({});
const submitError = ref("");
const submitting = ref(false);

// Reset form whenever the modal opens
watch(
  () => props.modelValue,
  (open) => {
    if (open) {
      form.value = { hoursWorked: null, logDate: todayIso.value, notes: "" };
      errors.value = {};
      submitError.value = "";
    }
  }
);

function validate() {
  errors.value = {};
  if (!form.value.hoursWorked || form.value.hoursWorked <= 0) {
    errors.value.hoursWorked = "Hours worked must be greater than 0.";
  } else if (form.value.hoursWorked > 24) {
    errors.value.hoursWorked = "Cannot log more than 24 hours per entry.";
  }
  return Object.keys(errors.value).length === 0;
}

async function submit() {
  if (!validate()) return;
  submitting.value = true;
  submitError.value = "";
  try {
    await ticketApi.logHours(props.ticketId, {
      hoursWorked: form.value.hoursWorked,
      logDate: form.value.logDate,
      notes: form.value.notes || null,
    });
    emit("logged");
    emit("update:modelValue", false);
  } catch (e) {
    submitError.value =
      e.response?.data?.message || "Failed to log hours. Please try again.";
  } finally {
    submitting.value = false;
  }
}

function onDismiss() {
  emit("update:modelValue", false);
}
</script>

<style scoped>
.modal-enter-active {
  transition: all 0.2s ease-out;
}
.modal-leave-active {
  transition: all 0.15s ease-in;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from .relative,
.modal-leave-to .relative {
  transform: scale(0.95) translateY(-8px);
}
</style>
