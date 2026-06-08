<template>
  <div class="space-y-2">
    <div
      v-for="agent in agents"
      :key="agent.userId"
      class="flex items-center gap-3 p-3 rounded-xl border border-gray-100 dark:border-white/[0.06] bg-gray-50/50 dark:bg-white/[0.02] hover:bg-gray-50 dark:hover:bg-white/[0.04] transition-all"
    >
      <!-- Avatar -->
      <div
        class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0 text-xs font-bold"
        :class="
          agent.isAvailable
            ? 'bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400'
            : 'bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400'
        "
      >
        {{ initials(agent.agentName) }}
      </div>

      <!-- Info -->
      <div class="flex-1 min-w-0">
        <div class="flex items-center justify-between gap-2 mb-1">
          <p
            class="text-[13px] font-semibold text-[#0F172A] dark:text-white truncate"
          >
            {{ agent.agentName }}
          </p>
          <span
            class="text-[10px] font-bold px-1.5 py-0.5 rounded-full flex-shrink-0"
            :class="
              agent.isAvailable
                ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                : 'bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400'
            "
          >
            {{ agent.isAvailable ? "Available" : "Full" }}
          </span>
        </div>
        <!-- Workload bar -->
        <div class="flex items-center gap-2">
          <div
            class="flex-1 h-1.5 rounded-full bg-gray-200 dark:bg-white/10 overflow-hidden"
          >
            <div
              class="h-full rounded-full transition-all duration-300"
              :class="barColor(agent.openTickets)"
              :style="{
                width: `${Math.min((agent.openTickets / 3) * 100, 100)}%`,
              }"
            />
          </div>
          <span
            class="text-[11px] text-gray-400 dark:text-gray-500 flex-shrink-0"
          >
            {{ agent.openTickets }}/3
          </span>
        </div>
      </div>

      <!-- Assign button -->
      <button
        v-if="showAssign"
        :disabled="!agent.isAvailable"
        @click="$emit('assign', agent.userId)"
        class="flex-shrink-0 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-150"
        :class="
          agent.isAvailable
            ? 'bg-[#14B8A6] text-white hover:bg-teal-600 active:bg-teal-700 shadow-sm'
            : 'bg-gray-100 text-gray-400 dark:bg-white/5 dark:text-gray-600 cursor-not-allowed'
        "
      >
        Assign
      </button>
    </div>

    <div
      v-if="!agents.length"
      class="py-8 text-center text-gray-400 dark:text-gray-500 text-sm"
    >
      No agents available
    </div>
  </div>
</template>

<script setup>
defineProps({
  agents: { type: Array, default: () => [] },
  ticketId: { type: [String, Number], default: null },
  showAssign: { type: Boolean, default: true },
});

defineEmits(["assign"]);

function initials(name) {
  return (name || "")
    .split(" ")
    .map((n) => n[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function barColor(count) {
  if (count < 2) return "bg-green-500";
  if (count === 2) return "bg-yellow-500";
  return "bg-red-500";
}
</script>
