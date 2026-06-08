<template>
  <div
    class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-4"
  >
    <h4 class="text-sm font-semibold text-[#0F172A] dark:text-white mb-3">
      Add Comment
    </h4>

    <!-- Rich text editor -->
    <RichTextEditor
      ref="editorRef"
      v-model="content"
      placeholder="Write your comment..."
      min-height="120px"
    />

    <!-- File attachments -->
    <div class="mt-3">
      <input
        ref="fileInput"
        type="file"
        multiple
        class="hidden"
        @change="onFilesSelected"
      />
      <button
        type="button"
        @click="fileInput.click()"
        class="inline-flex items-center gap-1.5 text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition-colors"
      >
        <Paperclip :size="14" />
        Attach files
      </button>

      <!-- File chips -->
      <div v-if="selectedFiles.length" class="flex flex-wrap gap-2 mt-2">
        <div
          v-for="(file, i) in selectedFiles"
          :key="i"
          class="flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-gray-100 dark:bg-white/5 text-xs text-gray-600 dark:text-gray-400"
        >
          <Paperclip :size="10" />
          <span class="max-w-[140px] truncate">{{ file.name }}</span>
          <button
            @click="removeFile(i)"
            class="hover:text-red-500 transition-colors"
          >
            <X :size="11" />
          </button>
        </div>
      </div>
    </div>

    <!-- Bottom row -->
    <div
      class="flex items-center justify-between mt-3 pt-3 border-t border-gray-100 dark:border-white/[0.06]"
    >
      <!-- Internal toggle -->
      <label
        v-if="canMarkInternal"
        class="flex items-center gap-2 cursor-pointer select-none"
      >
        <div
          class="relative w-8 h-4 rounded-full transition-colors duration-200"
          :class="isInternal ? 'bg-yellow-400' : 'bg-gray-200 dark:bg-white/10'"
          @click="isInternal = !isInternal"
        >
          <span
            class="absolute top-0.5 left-0.5 w-3 h-3 bg-white rounded-full shadow transition-transform duration-200"
            :class="isInternal ? 'translate-x-4' : ''"
          />
        </div>
        <span
          class="text-xs text-gray-500 dark:text-gray-400 flex items-center gap-1"
        >
          <Lock :size="11" />
          Internal note
        </span>
      </label>
      <div v-else />

      <!-- Submit -->
      <button
        @click="submit"
        :disabled="submitting || !content.trim()"
        class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 active:bg-teal-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-150 shadow-sm"
      >
        <LoadingSpinner v-if="submitting" size="sm" />
        <Send v-else :size="14" />
        {{ submitting ? "Sending..." : "Send" }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { Paperclip, X, Lock, Send } from "lucide-vue-next";
import { ticketApi } from "../../api/ticketApi";
import { useToastStore } from "../../store/toast";
import RichTextEditor from "../ui/RichTextEditor.vue";
import LoadingSpinner from "../ui/LoadingSpinner.vue";

const props = defineProps({
  ticketId: { type: [String, Number], required: true },
  canMarkInternal: { type: Boolean, default: false },
});

const emit = defineEmits(["comment-added"]);

const toastStore = useToastStore();

const editorRef = ref(null);
const content = ref("");
const isInternal = ref(false);
const selectedFiles = ref([]);
const fileInput = ref(null);
const submitting = ref(false);

function onFilesSelected(e) {
  selectedFiles.value = [...selectedFiles.value, ...Array.from(e.target.files)];
  e.target.value = "";
}

function removeFile(i) {
  selectedFiles.value.splice(i, 1);
}

async function submit() {
  if (!content.value.trim() || submitting.value) return;
  submitting.value = true;
  try {
    const res = await ticketApi.addComment(
      props.ticketId,
      content.value,
      isInternal.value
    );
    const commentId = res.data?.id;

    for (const file of selectedFiles.value) {
      await ticketApi.uploadAttachment(props.ticketId, file, commentId);
    }

    editorRef.value?.clear();
    content.value = "";
    selectedFiles.value = [];
    isInternal.value = false;
    emit("comment-added");
  } catch {
    toastStore.show("Failed to send comment", "error");
  } finally {
    submitting.value = false;
  }
}
</script>
