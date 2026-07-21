<template>
  <div class="space-y-2">
    <div
      v-if="!attachments.length"
      class="py-6 text-center text-gray-400 dark:text-gray-500 text-sm"
    >
      No attachments
    </div>

    <div
      v-for="att in attachments"
      :key="att.id"
      class="flex items-center gap-3 p-3 rounded-xl border border-gray-100 dark:border-white/[0.06] bg-gray-50/50 dark:bg-white/[0.02] hover:bg-gray-100 dark:hover:bg-white/[0.05] transition-all group"
    >
      <!-- Icon -->
      <div
        class="w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0"
        :class="iconBg(att.fileName)"
      >
        <component
          :is="fileIcon(att.fileName)"
          :size="16"
          :class="iconColor(att.fileName)"
        />
      </div>

      <!-- Info -->
      <div class="flex-1 min-w-0">
        <p
          class="text-[13px] font-medium text-[#0F172A] dark:text-white truncate group-hover:text-[#14B8A6] transition-colors"
        >
          {{ att.fileName }}
        </p>
        <p class="text-[11px] text-gray-400 mt-0.5">
          {{ formatSize(att.fileSize) }}
          <span v-if="att.uploadedBy"> · {{ att.uploadedBy }}</span>
          <span v-if="att.uploadedAt"> · {{ formatDate(att.uploadedAt) }}</span>
        </p>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-1 flex-shrink-0">
        <!-- Preview button (images + PDF only) -->
        <button
          v-if="canPreview(att.fileName)"
          @click="openPreview(att)"
          class="p-1.5 rounded-lg text-gray-300 dark:text-gray-600 hover:text-[#14B8A6] hover:bg-[#14B8A6]/10 transition-colors"
          title="Preview"
        >
          <Eye :size="14" />
        </button>

        <!-- Download button -->
        <button
          @click="downloadFile(att)"
          class="p-1.5 rounded-lg text-gray-300 dark:text-gray-600 hover:text-[#14B8A6] hover:bg-[#14B8A6]/10 transition-colors"
          title="Download"
        >
          <Download :size="14" />
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import {
  FileImage,
  FileText,
  File,
  Archive,
  Download,
  FileCode,
  Eye,
} from "lucide-vue-next";
import api from "../../api/axios";

const props = defineProps({
  attachments: { type: Array, default: () => [] },
  ticketId: { type: Number, default: null },
});

async function openPreview(att) {
  // Open the tab synchronously (within the click handler) so popup blockers
  // don't treat the later async navigation as an unsolicited popup. We need
  // a live reference to navigate it once the blob loads, so "noopener" (which
  // makes window.open return null) can't be used here.
  const newTab = window.open("", "_blank");
  if (props.ticketId && att.id) {
    try {
      const res = await api.get(
        `/ticket/${props.ticketId}/attachments/${att.id}/preview`,
        { responseType: "blob" }
      );
      const url = URL.createObjectURL(res.data);
      if (newTab) newTab.location.href = url;
      // Delay revocation so the new tab has time to load the blob
      setTimeout(() => URL.revokeObjectURL(url), 60000);
      return;
    } catch {
      // fall through to direct URL fallback
    }
  }
  const path = att.filePath || att.fileUrl || "";
  const fallbackUrl = path
    ? path.startsWith("http")
      ? path
      : `https://localhost:7091${path}`
    : "";
  if (newTab && fallbackUrl) newTab.location.href = fallbackUrl;
  else if (newTab) newTab.close();
}

async function downloadFile(att) {
  if (props.ticketId && att.id) {
    try {
      const res = await api.get(
        `/ticket/${props.ticketId}/attachments/${att.id}/download`,
        { responseType: "blob" }
      );
      const url = URL.createObjectURL(res.data);
      const a = document.createElement("a");
      a.href = url;
      a.download = att.fileName;
      a.click();
      URL.revokeObjectURL(url);
      return;
    } catch {
      // fall through to direct URL fallback
    }
  }
  // Fallback: direct URL
  const path = att.filePath || att.fileUrl || "";
  if (path) window.open(path.startsWith("http") ? path : `https://localhost:7091${path}`, "_blank");
}

function ext(fileName) {
  return (fileName || "").split(".").pop().toLowerCase();
}

function canPreview(fileName) {
  const e = ext(fileName);
  return ["jpg", "jpeg", "png", "gif", "webp", "svg", "pdf"].includes(e);
}

function fileIcon(fileName) {
  const e = ext(fileName);
  if (["jpg", "jpeg", "png", "gif", "webp", "svg"].includes(e))
    return FileImage;
  if (["pdf", "doc", "docx", "txt", "xlsx", "xls", "csv"].includes(e))
    return FileText;
  if (["zip", "rar", "7z", "tar", "gz"].includes(e)) return Archive;
  if (["js", "ts", "json", "html", "css", "py", "cs"].includes(e))
    return FileCode;
  return File;
}

function iconBg(fileName) {
  const e = ext(fileName);
  if (["jpg", "jpeg", "png", "gif", "webp", "svg"].includes(e))
    return "bg-purple-100 dark:bg-purple-900/30";
  if (["pdf"].includes(e)) return "bg-red-100 dark:bg-red-900/30";
  if (["doc", "docx"].includes(e)) return "bg-blue-100 dark:bg-blue-900/30";
  if (["xlsx", "xls", "csv"].includes(e))
    return "bg-green-100 dark:bg-green-900/30";
  if (["zip", "rar", "7z", "tar", "gz"].includes(e))
    return "bg-yellow-100 dark:bg-yellow-900/30";
  return "bg-gray-100 dark:bg-white/5";
}

function iconColor(fileName) {
  const e = ext(fileName);
  if (["jpg", "jpeg", "png", "gif", "webp", "svg"].includes(e))
    return "text-purple-600 dark:text-purple-400";
  if (["pdf"].includes(e)) return "text-red-600 dark:text-red-400";
  if (["doc", "docx"].includes(e)) return "text-blue-600 dark:text-blue-400";
  if (["xlsx", "xls", "csv"].includes(e))
    return "text-green-600 dark:text-green-400";
  if (["zip", "rar", "7z", "tar", "gz"].includes(e))
    return "text-yellow-600 dark:text-yellow-400";
  return "text-gray-500 dark:text-gray-400";
}

function formatSize(bytes) {
  if (!bytes) return "—";
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1048576).toFixed(1)} MB`;
}

function formatDate(dateStr) {
  if (!dateStr) return "";
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
}
</script>
