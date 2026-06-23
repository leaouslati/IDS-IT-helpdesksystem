<template>
  <div class="space-y-4">
    <!-- Skeleton -->
    <template v-if="loading">
      <div v-for="i in 3" :key="i" class="flex gap-3 animate-pulse">
        <div
          class="w-8 h-8 rounded-full bg-gray-200 dark:bg-white/10 flex-shrink-0"
        />
        <div class="flex-1 space-y-2">
          <div class="h-3 bg-gray-200 dark:bg-white/10 rounded w-24" />
          <div class="h-16 bg-gray-200 dark:bg-white/10 rounded-xl" />
        </div>
      </div>
    </template>

    <!-- Comments -->
    <template v-else>
      <div
        v-if="!comments.length"
        class="py-10 text-center text-gray-400 dark:text-gray-500 text-sm"
      >
        No comments yet. Be the first to respond.
      </div>

      <div
        v-for="comment in comments"
        :key="comment.id"
        class="flex gap-3"
        :class="comment.isAttachmentOnly ? 'justify-center' : isRightSide(comment) ? 'flex-row-reverse' : ''"
      >
        <!-- Avatar: hidden for attachment-only entries -->
        <div
          v-if="!comment.isAttachmentOnly"
          class="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 text-[11px] font-bold mt-0.5"
          :class="
            isRightSide(comment)
              ? 'bg-[#14B8A6]/20 text-[#14B8A6]'
              : 'bg-gray-200 text-gray-600 dark:bg-white/10 dark:text-gray-300'
          "
          :title="comment.userName"
        >
          {{ initials(comment.userName) }}
        </div>

        <!-- Bubble -->
        <div
          :class="[
            'min-w-0',
            comment.isAttachmentOnly ? 'max-w-[90%]' : 'max-w-[75%]',
            !comment.isAttachmentOnly && isRightSide(comment) ? 'items-end flex flex-col' : ''
          ]"
        >
          <!-- Meta (hidden for attachment-only) -->
          <div
            v-if="!comment.isAttachmentOnly"
            class="flex items-center gap-2 mb-1"
            :class="isRightSide(comment) ? 'flex-row-reverse' : ''"
          >
            <span
              class="text-[12px] font-semibold text-[#0F172A] dark:text-white"
            >
              {{ comment.userName }}
            </span>
            <span class="text-[10px] text-gray-400 capitalize">
              {{ comment.userRole }}
            </span>
            <Lock
              v-if="comment.isInternal"
              :size="10"
              class="text-yellow-500"
              title="Internal note"
            />
            <span class="text-[10px] text-gray-400">
              {{ timeAgo(comment.createdAt) }}
            </span>
          </div>

          <!-- Attachment-only system entry -->
          <div
            v-if="comment.isAttachmentOnly"
            class="px-4 py-2.5 rounded-xl border border-dashed border-gray-200 dark:border-white/10 bg-gray-50/60 dark:bg-white/[0.02] text-sm flex items-center gap-2"
          >
            <Paperclip :size="12" class="text-gray-400 flex-shrink-0" />
            <span class="text-gray-500 dark:text-gray-400 text-[12px]" v-html="comment.content" />
          </div>

          <!-- Escalation comment -->
          <div
            v-else-if="comment.isEscalationComment"
            class="px-4 py-3 rounded-xl border-l-4 border-orange-400 bg-orange-50 dark:bg-orange-900/10 text-sm text-orange-800 dark:text-orange-300"
          >
            <div class="flex items-center gap-1.5 mb-1.5">
              <AlertTriangle :size="13" class="flex-shrink-0" />
              <span class="font-semibold text-xs uppercase tracking-wide"
                >Escalation Note</span
              >
            </div>
            <div class="prose-sm" v-html="comment.content" />
          </div>

          <!-- Internal comment -->
          <div
            v-else-if="comment.isInternal"
            class="px-4 py-3 rounded-xl border border-dashed border-yellow-300 dark:border-yellow-700 bg-yellow-50/50 dark:bg-yellow-900/10 text-sm"
          >
            <div
              class="flex items-center gap-1.5 mb-1.5 text-yellow-600 dark:text-yellow-500"
            >
              <Lock :size="11" />
              <span class="text-[10px] font-semibold uppercase tracking-wide"
                >Internal note</span
              >
            </div>
            <div
              class="text-gray-700 dark:text-gray-300 prose-sm"
              v-html="comment.content"
            />
          </div>

          <!-- Regular comment -->
          <div
            v-else
            class="px-4 py-3 rounded-xl text-sm leading-relaxed"
            :class="
              isRightSide(comment)
                ? 'bg-[#14B8A6] text-white rounded-tr-sm'
                : 'bg-gray-100 dark:bg-white/[0.07] text-gray-700 dark:text-gray-300 rounded-tl-sm'
            "
          >
            <div class="prose-sm break-words" v-html="comment.content" />
          </div>

          <!-- Attachments -->
          <div
            v-if="comment.attachments && comment.attachments.length"
            class="mt-2 space-y-1"
          >
            <a
              v-for="att in comment.attachments"
              :key="att.id"
              :href="att.fileUrl"
              target="_blank"
              class="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-gray-100 dark:bg-white/5 text-xs text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-white/10 transition-colors"
            >
              <Paperclip :size="11" />
              <span class="truncate max-w-[160px]">{{ att.fileName }}</span>
              <span class="text-gray-400 flex-shrink-0">{{
                formatSize(att.fileSize)
              }}</span>
            </a>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { Lock, AlertTriangle, Paperclip } from "lucide-vue-next";

const props = defineProps({
  comments: { type: Array, default: () => [] },
  currentUserId: { type: [String, Number], default: null },
  currentUserRole: { type: String, default: "" },
  loading: { type: Boolean, default: false },
});

// Right side = your own comment (WhatsApp style). Use numeric comparison because
// authStore may return the id as a string depending on JWT decoding.
function isRightSide(comment) {
  return Number(comment.userId) === Number(props.currentUserId);
}

function initials(name) {
  return (name || "")
    .split(" ")
    .map((n) => n[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function timeAgo(dateStr) {
  if (!dateStr) return "";
  // Append Z so the browser treats the value as UTC, not local time
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr)
    ? dateStr
    : dateStr + "Z";
  const diff = Math.floor((Date.now() - new Date(normalized).getTime()) / 1000);
  if (diff < 60) return "just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
  if (diff < 86400 * 7) return `${Math.floor(diff / 86400)}d ago`;
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
}

function formatSize(bytes) {
  if (!bytes) return "";
  if (bytes < 1024) return `${bytes}B`;
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)}KB`;
  return `${(bytes / 1048576).toFixed(1)}MB`;
}
</script>
