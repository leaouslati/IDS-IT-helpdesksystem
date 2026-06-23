<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="open"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/75" @click="$emit('close')" />

        <!-- Panel -->
        <div
          class="relative z-10 bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-4xl flex flex-col overflow-hidden"
          style="max-height: 90vh"
        >
          <!-- Header -->
          <div
            class="flex items-center justify-between px-5 py-3.5 border-b border-gray-100 dark:border-white/[0.06] flex-shrink-0"
          >
            <div class="flex items-center gap-3 min-w-0">
              <Paperclip :size="14" class="text-gray-400 flex-shrink-0" />
              <span
                class="text-[13px] font-medium text-[#0F172A] dark:text-white truncate"
              >
                {{ attachment?.fileName }}
              </span>
              <span class="text-[11px] text-gray-400 flex-shrink-0">
                {{ formatSize(attachment?.fileSize) }}
              </span>
            </div>
            <div class="flex items-center gap-1 flex-shrink-0 ml-3">
              <button
                @click="download"
                class="p-2 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
                title="Download"
              >
                <Download :size="15" />
              </button>
              <button
                @click="$emit('close')"
                class="p-2 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                <X :size="15" />
              </button>
            </div>
          </div>

          <!-- Content area -->
          <div
            class="flex-1 overflow-auto flex items-center justify-center bg-gray-50 dark:bg-[#0F172A] min-h-[300px] p-6"
          >
            <!-- Loading -->
            <div v-if="loading" class="text-center">
              <div
                class="w-10 h-10 border-2 border-[#14B8A6] border-t-transparent rounded-full animate-spin mx-auto mb-3"
              />
              <p class="text-sm text-gray-400">Loading preview...</p>
            </div>

            <!-- Error / non-previewable -->
            <div v-else-if="loadError || !canPreview" class="text-center">
              <File :size="44" class="text-gray-200 dark:text-gray-700 mx-auto mb-3" />
              <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">
                {{ loadError ? "Preview unavailable" : "No preview for this file type" }}
              </p>
              <button
                @click="download"
                class="inline-flex items-center gap-2 px-5 py-2.5 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors"
              >
                <Download :size="14" />
                Download File
              </button>
            </div>

            <!-- Image preview -->
            <img
              v-else-if="isImage && blobUrl"
              :src="blobUrl"
              :alt="attachment?.fileName"
              class="max-w-full max-h-[70vh] object-contain rounded-lg shadow-sm"
            />

            <!-- PDF preview -->
            <iframe
              v-else-if="isPdf && blobUrl"
              :src="blobUrl"
              class="w-full rounded"
              style="height: 70vh; border: none"
            />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { Paperclip, Download, X, File } from "lucide-vue-next";
import api from "../../api/axios";

const props = defineProps({
  open: { type: Boolean, default: false },
  attachment: { type: Object, default: null },
  ticketId: { type: Number, default: null },
});

defineEmits(["close"]);

const blobUrl = ref(null);
const loading = ref(false);
const loadError = ref(false);

const fileExt = computed(() =>
  (props.attachment?.fileName || "").split(".").pop().toLowerCase()
);
const isImage = computed(() =>
  ["jpg", "jpeg", "png", "gif", "webp", "svg"].includes(fileExt.value)
);
const isPdf = computed(() => fileExt.value === "pdf");
const canPreview = computed(() => isImage.value || isPdf.value);

watch(
  () => props.open,
  async (open) => {
    if (open && props.attachment && props.ticketId && canPreview.value) {
      await loadPreview();
    } else if (!open) {
      cleanup();
    }
  }
);

async function loadPreview() {
  loading.value = true;
  loadError.value = false;
  try {
    const res = await api.get(
      `/ticket/${props.ticketId}/attachments/${props.attachment.id}/preview`,
      { responseType: "blob" }
    );
    if (blobUrl.value) URL.revokeObjectURL(blobUrl.value);
    blobUrl.value = URL.createObjectURL(res.data);
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

async function download() {
  if (!props.attachment || !props.ticketId) return;
  try {
    const res = await api.get(
      `/ticket/${props.ticketId}/attachments/${props.attachment.id}/download`,
      { responseType: "blob" }
    );
    const url = URL.createObjectURL(res.data);
    const a = document.createElement("a");
    a.href = url;
    a.download = props.attachment.fileName;
    a.click();
    URL.revokeObjectURL(url);
  } catch {
    // download failed silently
  }
}

function cleanup() {
  if (blobUrl.value) {
    URL.revokeObjectURL(blobUrl.value);
    blobUrl.value = null;
  }
  loadError.value = false;
}

function formatSize(bytes) {
  if (!bytes) return "";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1048576).toFixed(1)} MB`;
}
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
