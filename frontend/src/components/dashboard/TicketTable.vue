<template>
  <div
    class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] overflow-hidden"
  >
    <!-- Header -->
    <div
      class="flex items-center justify-between px-5 py-4 border-b border-gray-100 dark:border-white/[0.06]"
    >
      <h3 class="font-semibold text-[#0F172A] dark:text-white text-sm">
        {{ title }}
      </h3>
      <slot name="actionButton" />
    </div>

    <!-- Table -->
    <div class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="bg-gray-50 dark:bg-white/[0.03]">
            <th
              v-for="col in columns"
              :key="col.key"
              class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 dark:text-gray-500 uppercase tracking-wider whitespace-nowrap"
            >
              {{ col.label }}
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 dark:divide-white/[0.04]">
          <tr
            v-for="ticket in tickets"
            :key="ticket.id"
            class="hover:bg-gray-50 dark:hover:bg-white/[0.03] transition-colors duration-150 cursor-pointer"
            @click="emit('row-click', ticket)"
          >
            <td
              v-for="col in columns"
              :key="col.key"
              class="px-5 py-3.5 whitespace-nowrap"
            >
              <span
                v-if="col.type === 'status'"
                :class="statusClass(ticket[col.key])"
              >
                {{ ticket[col.key] }}
              </span>
              <span
                v-else-if="col.type === 'priority'"
                :class="priorityClass(ticket[col.key])"
              >
                {{ ticket[col.key] }}
              </span>
              <template v-else-if="col.type === 'action'">
                <button
                  v-if="!col.showIf || col.showIf(ticket)"
                  @click.stop="emit('action-click', ticket)"
                  class="text-xs font-semibold text-[#14B8A6] hover:text-teal-600 transition-colors duration-150"
                >
                  {{
                    col.labelFn
                      ? col.labelFn(ticket)
                      : col.actionLabel || "View"
                  }}
                </button>
              </template>
              <span
                v-else
                class="text-[#0F172A] dark:text-gray-300 text-[13px]"
              >
                {{ ticket[col.key] ?? "—" }}
              </span>
            </td>
          </tr>
          <tr v-if="!tickets || tickets.length === 0">
            <td
              :colspan="columns.length"
              class="text-center py-14 text-gray-400 dark:text-gray-500 text-sm"
            >
              No tickets to display
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
defineProps({
  tickets: { type: Array, default: () => [] },
  columns: { type: Array, default: () => [] },
  title: { type: String, default: "Tickets" },
});
const emit = defineEmits(["action-click", "row-click"]);

function statusClass(status) {
  const base = "px-2.5 py-0.5 rounded-full text-[11px] font-semibold";
  const map = {
    Open: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    "In Progress":
      "bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400",
    Pending:
      "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    Resolved:
      "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
    Closed: "bg-gray-100 text-gray-600 dark:bg-gray-700/50 dark:text-gray-400",
    Escalated:
      "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
  };
  return `${base} ${map[status] || map["Open"]}`;
}

function priorityClass(priority) {
  const base = "px-2.5 py-0.5 rounded-full text-[11px] font-semibold";
  const map = {
    Low: "bg-gray-100 text-gray-600 dark:bg-gray-700/50 dark:text-gray-400",
    Medium:
      "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    High: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
    Critical: "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
  };
  return `${base} ${map[priority] || map["Low"]}`;
}
</script>
