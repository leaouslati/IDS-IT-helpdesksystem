<template>
  <div class="rich-text-editor">
    <div ref="toolbarEl" class="ql-toolbar-custom">
      <button
        type="button"
        @mousedown.prevent="format('bold')"
        :class="{ active: activeFormats.bold }"
        title="Bold"
      >
        <strong class="text-xs">B</strong>
      </button>
      <button
        type="button"
        @mousedown.prevent="format('italic')"
        :class="{ active: activeFormats.italic }"
        title="Italic"
      >
        <em class="text-xs">I</em>
      </button>
      <button
        type="button"
        @mousedown.prevent="format('underline')"
        :class="{ active: activeFormats.underline }"
        title="Underline"
      >
        <span class="text-xs underline">U</span>
      </button>
      <span class="separator" />
      <button
        type="button"
        @mousedown.prevent="format('list', 'bullet')"
        title="Bullet list"
      >
        <List :size="13" />
      </button>
      <button
        type="button"
        @mousedown.prevent="format('list', 'ordered')"
        title="Ordered list"
      >
        <ListOrdered :size="13" />
      </button>
      <span class="separator" />
      <button
        type="button"
        @mousedown.prevent="clearFormat"
        title="Clear formatting"
      >
        <RemoveFormatting :size="13" />
      </button>
    </div>
    <div
      ref="editorEl"
      class="ql-editor-custom"
      :style="{ minHeight: minHeight }"
      contenteditable="true"
      :data-placeholder="placeholder"
      @input="onInput"
      @keyup="updateFormats"
      @mouseup="updateFormats"
    />
    <div
      v-if="charLimit"
      class="flex justify-end px-3 py-1.5 border-t border-gray-200 dark:border-gray-700"
    >
      <span
        class="text-[11px]"
        :class="charCount > charLimit ? 'text-red-500' : 'text-gray-400'"
      >
        {{ charCount }} / {{ charLimit }}
      </span>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { List, ListOrdered, RemoveFormatting } from "lucide-vue-next";

const props = defineProps({
  modelValue: { type: String, default: "" },
  placeholder: { type: String, default: "Write something..." },
  minHeight: { type: String, default: "140px" },
  charLimit: { type: Number, default: 0 },
});

const emit = defineEmits(["update:modelValue"]);

const editorEl = ref(null);
const charCount = ref(0);
const activeFormats = ref({ bold: false, italic: false, underline: false });

onMounted(() => {
  if (props.modelValue && editorEl.value) {
    editorEl.value.innerHTML = props.modelValue;
    charCount.value = editorEl.value.innerText.length;
  }
  editorEl.value?.addEventListener("selectionchange", updateFormats);
});

watch(
  () => props.modelValue,
  (val) => {
    if (editorEl.value && editorEl.value.innerHTML !== val) {
      editorEl.value.innerHTML = val || "";
      charCount.value = editorEl.value.innerText.length;
    }
  }
);

function onInput() {
  const html = editorEl.value.innerHTML;
  charCount.value = editorEl.value.innerText.length;
  emit("update:modelValue", html === "<br>" ? "" : html);
  updateFormats();
}

function format(cmd, value) {
  editorEl.value?.focus();
  document.execCommand(cmd, false, value || null);
  updateFormats();
  onInput();
}

function clearFormat() {
  editorEl.value?.focus();
  document.execCommand("removeFormat", false, null);
  onInput();
}

function updateFormats() {
  activeFormats.value = {
    bold: document.queryCommandState("bold"),
    italic: document.queryCommandState("italic"),
    underline: document.queryCommandState("underline"),
  };
}

function getHTML() {
  return editorEl.value?.innerHTML || "";
}

function clear() {
  if (editorEl.value) {
    editorEl.value.innerHTML = "";
    charCount.value = 0;
    emit("update:modelValue", "");
  }
}

defineExpose({ getHTML, clear });
</script>

<style scoped>
.rich-text-editor {
  border: 1px solid #e5e7eb;
  border-radius: 0.5rem;
  overflow: hidden;
  background: #f9fafb;
  transition: border-color 0.15s, box-shadow 0.15s;
}

:global(.dark) .rich-text-editor {
  border-color: #374151;
  background: #0f172a;
}

.rich-text-editor:focus-within {
  border-color: #14b8a6;
  box-shadow: 0 0 0 2px rgba(20, 184, 166, 0.2);
}

.ql-toolbar-custom {
  display: flex;
  align-items: center;
  gap: 2px;
  padding: 6px 8px;
  border-bottom: 1px solid #e5e7eb;
  background: white;
}

:global(.dark) .ql-toolbar-custom {
  background: #1a1d2e;
  border-bottom-color: #374151;
}

.ql-toolbar-custom button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  border-radius: 4px;
  border: none;
  background: transparent;
  cursor: pointer;
  color: #6b7280;
  transition: all 0.15s;
}

.ql-toolbar-custom button:hover {
  background: #f3f4f6;
  color: #111827;
}

:global(.dark) .ql-toolbar-custom button:hover {
  background: rgba(255, 255, 255, 0.08);
  color: #f9fafb;
}

.ql-toolbar-custom button.active {
  background: rgba(20, 184, 166, 0.12);
  color: #14b8a6;
}

.separator {
  display: inline-block;
  width: 1px;
  height: 18px;
  background: #e5e7eb;
  margin: 0 4px;
}

:global(.dark) .separator {
  background: #374151;
}

.ql-editor-custom {
  padding: 12px 14px;
  outline: none;
  font-size: 14px;
  line-height: 1.6;
  color: #0f172a;
  word-break: break-word;
}

:global(.dark) .ql-editor-custom {
  color: #e5e7eb;
}

.ql-editor-custom:empty::before {
  content: attr(data-placeholder);
  color: #9ca3af;
  pointer-events: none;
}

.ql-editor-custom ul {
  list-style-type: disc;
  padding-left: 1.5em;
}

.ql-editor-custom ol {
  list-style-type: decimal;
  padding-left: 1.5em;
}
</style>
