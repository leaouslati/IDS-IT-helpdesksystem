<template>
  <AppLayout :navLinks="navLinks" pageTitle="Create Ticket">
    <div class="max-w-2xl mx-auto">
      <!-- Header -->
      <div class="flex items-center gap-3 mb-6">
        <button
          @click="router.back()"
          class="p-2 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
        >
          <ArrowLeft :size="18" />
        </button>
        <div>
          <h1 class="text-2xl font-bold text-[#0F172A] dark:text-white">
            Create New Ticket
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Report an IT issue or submit a service request
          </p>
        </div>
      </div>

      <!-- Form card -->
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-6 space-y-5"
      >
        <!-- Title -->
        <div>
          <label
            class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
          >
            Title <span class="text-red-500">*</span>
          </label>
          <input
            v-model="form.title"
            type="text"
            maxlength="150"
            placeholder="Brief description of the issue..."
            class="w-full px-4 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border rounded-lg text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
            :class="
              errors.title
                ? 'border-red-400'
                : 'border-gray-200 dark:border-gray-700'
            "
          />
          <div class="flex justify-between mt-1">
            <span v-if="errors.title" class="text-xs text-red-500">{{
              errors.title
            }}</span>
            <span v-else class="text-xs text-transparent">·</span>
            <span class="text-[11px] text-gray-400"
              >{{ form.title.length }}/150</span
            >
          </div>
        </div>

        <!-- Category + Priority row -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label
              class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
            >
              Category <span class="text-red-500">*</span>
            </label>
            <div class="relative">
              <Tag
                :size="14"
                class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
              />
              <select
                v-model="form.categoryId"
                class="w-full pl-9 pr-8 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all appearance-none cursor-pointer"
                :class="
                  errors.categoryId
                    ? 'border-red-400'
                    : 'border-gray-200 dark:border-gray-700'
                "
              >
                <option value="">Select category</option>
                <option
                  v-for="c in ticketStore.categories"
                  :key="c.id"
                  :value="c.id"
                >
                  {{ c.name }}
                </option>
              </select>
            </div>
            <span
              v-if="errors.categoryId"
              class="text-xs text-red-500 mt-1 block"
              >{{ errors.categoryId }}</span
            >
          </div>

          <div>
            <label
              class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
            >
              Priority <span class="text-red-500">*</span>
            </label>
            <div class="relative">
              <AlertCircle
                :size="14"
                class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
              />
              <select
                v-model="form.priorityId"
                class="w-full pl-9 pr-8 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all appearance-none cursor-pointer"
                :class="
                  errors.priorityId
                    ? 'border-red-400'
                    : 'border-gray-200 dark:border-gray-700'
                "
              >
                <option value="">Select priority</option>
                <option
                  v-for="p in ticketStore.priorities"
                  :key="p.id"
                  :value="p.id"
                >
                  {{ p.name }}
                </option>
              </select>
            </div>
            <span
              v-if="errors.priorityId"
              class="text-xs text-red-500 mt-1 block"
              >{{ errors.priorityId }}</span
            >
          </div>
        </div>

        <!-- Description -->
        <div>
          <label
            class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
          >
            Description <span class="text-red-500">*</span>
          </label>
          <RichTextEditor
            ref="descEditor"
            v-model="form.description"
            placeholder="Describe the issue in detail..."
            min-height="180px"
          />
          <span
            v-if="errors.description"
            class="text-xs text-red-500 mt-1 block"
            >{{ errors.description }}</span
          >
        </div>

        <!-- Attachments -->
        <div>
          <label
            class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
          >
            Attachments
            <span class="text-gray-300 font-normal normal-case"
              >(optional)</span
            >
          </label>

          <!-- Drop zone -->
          <div
            class="border-2 border-dashed rounded-xl p-6 text-center transition-all cursor-pointer"
            :class="
              dragging
                ? 'border-[#14B8A6] bg-teal-50/50 dark:bg-teal-900/10'
                : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600 bg-gray-50/50 dark:bg-white/[0.02]'
            "
            @click="fileInput.click()"
            @dragover.prevent="dragging = true"
            @dragleave="dragging = false"
            @drop.prevent="onDrop"
          >
            <Upload :size="22" class="mx-auto mb-2 text-gray-400" />
            <p class="text-sm text-gray-500 dark:text-gray-400">
              <span class="font-semibold text-[#14B8A6]">Click to upload</span>
              or drag & drop
            </p>
            <p class="text-xs text-gray-400 mt-1">Max 10MB per file</p>
            <input
              ref="fileInput"
              type="file"
              multiple
              class="hidden"
              @change="onFilesSelected"
            />
          </div>

          <!-- File chips -->
          <div v-if="selectedFiles.length" class="flex flex-wrap gap-2 mt-3">
            <div
              v-for="(file, i) in selectedFiles"
              :key="i"
              class="flex items-center gap-2 px-3 py-1.5 rounded-full bg-teal-50 dark:bg-teal-900/20 text-teal-700 dark:text-teal-400 text-xs font-medium border border-teal-200 dark:border-teal-800"
            >
              <Paperclip :size="11" />
              <span class="max-w-[160px] truncate">{{ file.name }}</span>
              <span class="text-teal-500 text-[10px]">{{
                formatSize(file.size)
              }}</span>
              <button
                @click.stop="removeFile(i)"
                class="hover:text-red-500 transition-colors ml-0.5"
              >
                <X :size="12" />
              </button>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div
          class="flex items-center justify-end gap-3 pt-2 border-t border-gray-100 dark:border-white/[0.06]"
        >
          <button
            type="button"
            @click="router.back()"
            class="px-5 py-2.5 rounded-lg text-sm font-semibold text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
          >
            Cancel
          </button>
          <button
            type="button"
            @click="submit"
            :disabled="submitting"
            class="inline-flex items-center gap-2 px-6 py-2.5 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 active:bg-teal-700 disabled:opacity-60 disabled:cursor-not-allowed transition-all shadow-sm"
          >
            <LoadingSpinner v-if="submitting" size="sm" />
            <Send v-else :size="14" />
            {{ submitting ? "Submitting..." : "Submit Ticket" }}
          </button>
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import {
  ArrowLeft,
  Tag,
  AlertCircle,
  Upload,
  Paperclip,
  X,
  Send,
  LayoutDashboard,
  FileText,
  Bell,
  User,
  BarChart2,
  Settings,
  Users,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import RichTextEditor from "../../components/ui/RichTextEditor.vue";
import LoadingSpinner from "../../components/ui/LoadingSpinner.vue";
import { useTicketStore } from "../../store/ticket";
import { useAuthStore } from "../../store/auth";
import { useToastStore } from "../../store/toast";
import { ticketApi } from "../../api/ticketApi";

const router = useRouter();
const ticketStore = useTicketStore();
const authStore = useAuthStore();
const toastStore = useToastStore();

const descEditor = ref(null);
const fileInput = ref(null);
const dragging = ref(false);
const submitting = ref(false);
const selectedFiles = ref([]);

const form = ref({
  title: "",
  description: "",
  categoryId: "",
  priorityId: "",
});
const errors = ref({});

const role = computed(() => authStore.userRole);

const navLinks = computed(() => {
  const map = {
    Admin: [
      { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/admin" },
      { icon: FileText, label: "Tickets", to: "/tickets" },
      { icon: Users, label: "Users", to: "/users" },
      { icon: BarChart2, label: "Reports", to: "/reports" },
      { icon: Settings, label: "Settings", to: "/settings" },
      { icon: User, label: "Profile", to: "/profile" },
    ],
    Manager: [
      { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/manager" },
      { icon: FileText, label: "All Tickets", to: "/tickets" },
      { icon: BarChart2, label: "Reports", to: "/reports" },
      { icon: Bell, label: "Notifications", to: "/notifications" },
      { icon: User, label: "Profile", to: "/profile" },
    ],
    Employee: [
      { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/employee" },
      { icon: FileText, label: "My Tickets", to: "/tickets" },
      { icon: Bell, label: "Notifications", to: "/notifications" },
      { icon: User, label: "Profile", to: "/profile" },
    ],
  };
  return map[role.value] || map.Employee;
});

onMounted(() => ticketStore.fetchLookups());

function validate() {
  errors.value = {};
  if (!form.value.title.trim()) errors.value.title = "Title is required";
  if (!form.value.categoryId) errors.value.categoryId = "Category is required";
  if (!form.value.priorityId) errors.value.priorityId = "Priority is required";
  if (!form.value.description.trim())
    errors.value.description = "Description is required";
  return Object.keys(errors.value).length === 0;
}

async function submit() {
  if (!validate() || submitting.value) return;
  submitting.value = true;
  try {
    const ticket = await ticketStore.createTicket({
      title: form.value.title,
      description: form.value.description,
      categoryId: form.value.categoryId,
      priorityId: form.value.priorityId,
    });

    for (const file of selectedFiles.value) {
      await ticketApi.uploadAttachment(ticket.id, file);
    }

    toastStore.show("Ticket submitted successfully!");
    router.push(`/tickets/${ticket.id}`);
  } catch (e) {
    toastStore.show(
      e.response?.data?.message || "Failed to create ticket",
      "error"
    );
  } finally {
    submitting.value = false;
  }
}

function onFilesSelected(e) {
  addFiles(Array.from(e.target.files));
  e.target.value = "";
}

function onDrop(e) {
  dragging.value = false;
  addFiles(Array.from(e.dataTransfer.files));
}

function addFiles(files) {
  const valid = files.filter((f) => f.size <= 10 * 1024 * 1024);
  const oversized = files.filter((f) => f.size > 10 * 1024 * 1024);
  if (oversized.length)
    toastStore.show(`${oversized.length} file(s) exceed 10MB limit`, "warning");
  selectedFiles.value = [...selectedFiles.value, ...valid];
}

function removeFile(i) {
  selectedFiles.value.splice(i, 1);
}

function formatSize(bytes) {
  if (bytes < 1024) return `${bytes}B`;
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)}KB`;
  return `${(bytes / 1048576).toFixed(1)}MB`;
}
</script>
