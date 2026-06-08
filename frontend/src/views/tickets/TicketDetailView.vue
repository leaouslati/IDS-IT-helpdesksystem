<template>
  <AppLayout
    :navLinks="navLinks"
    :pageTitle="ticket?.referenceNumber || 'Ticket Detail'"
  >
    <!-- Loading fullpage -->
    <LoadingSpinner v-if="loading" :full-page="true" />

    <!-- Error -->
    <div
      v-else-if="loadError"
      class="flex flex-col items-center justify-center py-24 gap-4"
    >
      <AlertCircle :size="40" class="text-red-500" />
      <div class="text-center">
        <h3 class="font-semibold text-[#0F172A] dark:text-white mb-1">
          Failed to load ticket
        </h3>
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
          {{ loadError }}
        </p>
        <button
          @click="loadTicket"
          class="px-4 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold"
        >
          Try Again
        </button>
      </div>
    </div>

    <template v-else-if="ticket">
      <!-- Back + header -->
      <div class="flex flex-wrap items-start justify-between gap-4 mb-5">
        <!-- Left: back + title -->
        <div class="flex items-start gap-3 min-w-0">
          <button
            @click="router.push('/tickets')"
            class="mt-1 p-1.5 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors flex-shrink-0"
          >
            <ArrowLeft :size="17" />
          </button>
          <div class="min-w-0">
            <!-- Ref number -->
            <div class="flex items-center gap-2 mb-1.5">
              <span
                class="font-mono text-sm font-bold text-gray-500 dark:text-gray-400 bg-gray-100 dark:bg-white/5 px-2.5 py-0.5 rounded"
              >
                {{ ticket.referenceNumber }}
              </span>
              <button
                @click="copyRef"
                class="text-gray-300 dark:text-gray-600 hover:text-gray-500 dark:hover:text-gray-300 transition-colors"
                title="Copy"
              >
                <Copy :size="13" />
              </button>
            </div>
            <!-- Title -->
            <h1
              class="text-xl font-bold text-[#0F172A] dark:text-white leading-snug"
            >
              {{ ticket.title }}
            </h1>
            <!-- Badges -->
            <div class="flex flex-wrap items-center gap-1.5 mt-2">
              <StatusBadge :status="ticket.status" />
              <PriorityBadge :priority="ticket.priority" />
              <EscalatedBadge :isEscalated="ticket.isEscalated" />
              <span
                v-if="ticket.category"
                class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-medium bg-gray-100 text-gray-600 dark:bg-white/5 dark:text-gray-400"
              >
                <Tag :size="9" />
                {{ ticket.category }}
              </span>
            </div>
          </div>
        </div>

        <!-- Right: action buttons -->
        <div class="flex flex-wrap items-center gap-2 flex-shrink-0">
          <!-- Edit (not shown to agents) -->
          <button
            v-if="ticket.canEdit && !editMode"
            @click="startEdit"
            class="inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 border border-gray-200 dark:border-gray-700 transition-all"
          >
            <Pencil :size="14" />
            Edit
          </button>

          <!-- Status update -->
          <div
            v-if="ticket.canUpdateStatus"
            class="relative"
            ref="statusDropRef"
          >
            <button
              @click="showStatusDrop = !showStatusDrop"
              class="inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-white/5 border border-gray-200 dark:border-gray-700 transition-all"
            >
              <RefreshCw :size="14" />
              Update Status
              <ChevronDown :size="13" />
            </button>
            <div
              v-if="showStatusDrop"
              class="absolute right-0 top-full mt-1 w-44 bg-white dark:bg-[#1A1D2E] rounded-xl shadow-lg border border-gray-100 dark:border-white/[0.08] py-1 z-20"
            >
              <button
                v-for="s in availableStatuses"
                :key="s.id || s.name"
                @click="doUpdateStatus(s.name)"
                class="w-full text-left px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
              >
                {{ s.name }}
              </button>
            </div>
          </div>

          <!-- Escalate -->
          <button
            v-if="ticket.canEscalate && !ticket.isEscalated"
            @click="showEscalateModal = true"
            class="inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium text-orange-600 dark:text-orange-400 hover:bg-orange-50 dark:hover:bg-orange-900/10 border border-orange-200 dark:border-orange-800 transition-all"
          >
            <AlertTriangle :size="14" />
            Escalate
          </button>

          <!-- Delete -->
          <button
            v-if="ticket.canDelete"
            @click="showDeleteModal = true"
            class="inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/10 border border-red-200 dark:border-red-800 transition-all"
          >
            <Trash2 :size="14" />
            Delete
          </button>
        </div>
      </div>

      <!-- Two-column layout -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
        <!-- LEFT column (2/3) -->
        <div class="lg:col-span-2 space-y-5">
          <!-- Ticket info / edit card -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-5"
          >
            <!-- View mode -->
            <template v-if="!editMode">
              <!-- Description -->
              <div class="mb-5">
                <h3
                  class="text-xs font-semibold uppercase tracking-wider text-gray-400 mb-3"
                >
                  Description
                </h3>
                <div
                  class="prose prose-sm max-w-none text-gray-700 dark:text-gray-300 leading-relaxed"
                  v-html="
                    ticket.description ||
                    '<em class=\'text-gray-400\'>No description provided.</em>'
                  "
                />
              </div>

              <!-- Meta grid -->
              <div
                class="grid grid-cols-2 gap-4 pt-4 border-t border-gray-100 dark:border-white/[0.06]"
              >
                <div>
                  <p
                    class="text-[10px] font-semibold uppercase tracking-wider text-gray-400 mb-0.5"
                  >
                    Department
                  </p>
                  <p class="text-sm text-[#0F172A] dark:text-gray-200">
                    {{ ticket.department || "—" }}
                  </p>
                </div>
                <div>
                  <p
                    class="text-[10px] font-semibold uppercase tracking-wider text-gray-400 mb-0.5"
                  >
                    Submitted By
                  </p>
                  <p class="text-sm text-[#0F172A] dark:text-gray-200">
                    {{ ticket.createdBy || "—" }}
                  </p>
                </div>
                <div>
                  <p
                    class="text-[10px] font-semibold uppercase tracking-wider text-gray-400 mb-0.5"
                  >
                    Created
                  </p>
                  <p class="text-sm text-[#0F172A] dark:text-gray-200">
                    {{ formatDate(ticket.createdAt) }}
                  </p>
                </div>
                <div>
                  <p
                    class="text-[10px] font-semibold uppercase tracking-wider text-gray-400 mb-0.5"
                  >
                    Last Updated
                  </p>
                  <p class="text-sm text-[#0F172A] dark:text-gray-200">
                    {{ formatDate(ticket.updatedAt) }}
                  </p>
                </div>
              </div>
            </template>

            <!-- Edit mode (Employee/Manager/Admin only — canEdit from backend) -->
            <template v-else>
              <div
                v-if="ticket.status !== 'Open'"
                class="mb-4 px-4 py-3 rounded-lg bg-yellow-50 dark:bg-yellow-900/10 border border-yellow-200 dark:border-yellow-800 text-sm text-yellow-700 dark:text-yellow-400 flex items-center gap-2"
              >
                <AlertTriangle :size="14" />
                Changes on an in-progress ticket will be recorded in the
                activity log.
              </div>

              <div class="space-y-4">
                <div>
                  <label
                    class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                    >Title</label
                  >
                  <input
                    v-model="editForm.title"
                    type="text"
                    maxlength="150"
                    class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
                  />
                </div>
                <div>
                  <label
                    class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                    >Description</label
                  >
                  <RichTextEditor
                    ref="editDescRef"
                    v-model="editForm.description"
                    min-height="150px"
                  />
                </div>
                <div class="grid grid-cols-2 gap-3">
                  <div>
                    <label
                      class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                      >Category</label
                    >
                    <select
                      v-model="editForm.categoryId"
                      class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all appearance-none cursor-pointer"
                    >
                      <option
                        v-for="c in ticketStore.categories"
                        :key="c.id"
                        :value="c.id"
                      >
                        {{ c.name }}
                      </option>
                    </select>
                  </div>
                  <div>
                    <label
                      class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                      >Priority</label
                    >
                    <select
                      v-model="editForm.priorityId"
                      class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all appearance-none cursor-pointer"
                    >
                      <option
                        v-for="p in ticketStore.priorities"
                        :key="p.id"
                        :value="p.id"
                      >
                        {{ p.name }}
                      </option>
                    </select>
                  </div>
                </div>
                <div class="flex justify-end gap-2 pt-1">
                  <button
                    @click="editMode = false"
                    class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    @click="saveEdit"
                    :disabled="saving"
                    class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-60 transition-colors"
                  >
                    <LoadingSpinner v-if="saving" size="sm" />
                    <Check v-else :size="14" />
                    Save Changes
                  </button>
                </div>
              </div>
            </template>
          </div>

          <!-- Comments section -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-5"
          >
            <h3
              class="text-sm font-semibold text-[#0F172A] dark:text-white mb-4 flex items-center gap-2"
            >
              <MessageSquare :size="15" class="text-gray-400" />
              Comments
              <span class="ml-1 text-xs font-normal text-gray-400"
                >({{ comments.length }})</span
              >
            </h3>
            <CommentThread
              :comments="comments"
              :current-user-id="authStore.user?.id"
              :current-user-role="authStore.userRole"
              :loading="commentsLoading"
            />
          </div>

          <!-- Comment editor -->
          <CommentEditor
            :ticket-id="ticketId"
            :can-mark-internal="
              ['Agent', 'Manager', 'Admin'].includes(authStore.userRole)
            "
            @comment-added="onCommentAdded"
          />
        </div>

        <!-- RIGHT column (1/3) -->
        <div class="space-y-4">
          <!-- Status card -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl border shadow-sm overflow-hidden"
            :class="statusBorderClass"
          >
            <div
              class="px-4 py-3 border-b border-gray-100 dark:border-white/[0.06]"
            >
              <p
                class="text-[10px] font-semibold uppercase tracking-wider text-gray-400"
              >
                Current Status
              </p>
              <div class="flex items-center justify-between mt-1.5">
                <StatusBadge :status="ticket.status" />
                <EscalatedBadge :isEscalated="ticket.isEscalated" />
              </div>
            </div>
            <div class="px-4 py-3 space-y-2.5">
              <!-- Assigned To + Assign button -->
              <div class="flex items-center justify-between gap-2">
                <div class="min-w-0">
                  <p
                    class="text-[10px] font-semibold uppercase tracking-wider text-gray-400 mb-0.5"
                  >
                    Assigned To
                  </p>
                  <p
                    class="text-sm font-medium text-[#0F172A] dark:text-gray-200 truncate"
                    :class="!ticket.assignedTo ? 'italic text-gray-400' : ''"
                  >
                    {{ ticket.assignedTo || "Unassigned" }}
                  </p>
                </div>
                <button
                  v-if="ticket.canAssign"
                  @click="openAssignModal"
                  class="flex-shrink-0 inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-[#14B8A6] text-white hover:bg-teal-600 transition-colors shadow-sm"
                >
                  <UserCheck :size="12" />
                  {{ ticket.assignedTo ? "Reassign" : "Assign Agent" }}
                </button>
              </div>

              <div v-if="ticket.escalationReason">
                <p
                  class="text-[10px] font-semibold uppercase tracking-wider text-orange-400 mb-0.5"
                >
                  Escalation Reason
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ ticket.escalationReason }}
                </p>
              </div>
            </div>
          </div>

          <!-- Attachments -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-4"
          >
            <h3
              class="text-sm font-semibold text-[#0F172A] dark:text-white mb-3 flex items-center gap-2"
            >
              <Paperclip :size="14" class="text-gray-400" />
              Attachments
              <span class="ml-auto text-xs text-gray-400">{{
                (ticket.attachments || []).length
              }}</span>
            </h3>
            <AttachmentList :attachments="ticket.attachments || []" />
          </div>

          <!-- Activity timeline -->
          <div
            class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-4"
          >
            <h3
              class="text-sm font-semibold text-[#0F172A] dark:text-white mb-4 flex items-center gap-2"
            >
              <Activity :size="14" class="text-gray-400" />
              Activity History
              <span class="ml-auto text-xs text-gray-400">{{
                (ticket.activityLog || []).length
              }}</span>
            </h3>
            <StatusTimeline :activity-log="ticket.activityLog || []" />
          </div>
        </div>
      </div>
    </template>

    <!-- ─── Assign Agent Modal ─── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showAssignModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showAssignModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-lg border border-gray-100 dark:border-white/[0.08]"
          >
            <!-- Modal header -->
            <div
              class="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-white/[0.08]"
            >
              <div>
                <h3 class="font-bold text-[#0F172A] dark:text-white">
                  Assign Ticket to Agent
                </h3>
                <p class="text-xs text-gray-400 mt-0.5">
                  Select an available agent from your team
                </p>
              </div>
              <button
                @click="showAssignModal = false"
                class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                <X :size="16" />
              </button>
            </div>

            <!-- Agent list -->
            <div class="px-6 py-4 max-h-[380px] overflow-y-auto">
              <!-- Loading -->
              <div
                v-if="agentsLoading"
                class="flex flex-col items-center justify-center py-10 gap-3"
              >
                <div
                  class="w-8 h-8 border-2 border-[#14B8A6] border-t-transparent rounded-full animate-spin"
                />
                <p class="text-sm text-gray-400">Loading agents...</p>
              </div>

              <!-- Error -->
              <div
                v-else-if="agentsError"
                class="py-8 text-center text-sm text-red-500"
              >
                {{ agentsError }}
              </div>

              <!-- Empty -->
              <div
                v-else-if="agents.length === 0"
                class="py-10 text-center text-sm text-gray-400"
              >
                No agents available in your department
              </div>

              <!-- Agent cards -->
              <div v-else class="space-y-2">
                <div
                  v-for="agent in agents"
                  :key="agent.userId"
                  class="flex items-center gap-3 p-3 rounded-xl border transition-all"
                  :class="agentBorderClass(agent)"
                >
                  <!-- Avatar -->
                  <div
                    class="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 text-sm font-bold"
                    :class="agentAvatarClass(agent)"
                  >
                    {{ agentInitials(agent.agentName) }}
                  </div>

                  <!-- Info -->
                  <div class="flex-1 min-w-0">
                    <p
                      class="text-sm font-semibold text-[#0F172A] dark:text-white truncate"
                    >
                      {{ agent.agentName }}
                    </p>
                    <div class="flex items-center gap-2 mt-0.5">
                      <!-- Workload bar -->
                      <div
                        class="w-16 h-1.5 rounded-full bg-gray-200 dark:bg-white/10 overflow-hidden"
                      >
                        <div
                          class="h-full rounded-full transition-all"
                          :class="workloadBarColor(agent.openTickets)"
                          :style="{
                            width: `${Math.min(
                              (agent.openTickets / 3) * 100,
                              100
                            )}%`,
                          }"
                        />
                      </div>
                      <span class="text-[11px] text-gray-400">
                        {{ agent.openTickets }}/3 tickets
                      </span>
                    </div>
                  </div>

                  <!-- Availability badge -->
                  <span
                    class="text-[11px] font-bold px-2 py-0.5 rounded-full flex-shrink-0"
                    :class="agentBadgeClass(agent)"
                  >
                    {{ agentAvailabilityLabel(agent) }}
                  </span>

                  <!-- Assign button -->
                  <button
                    :disabled="
                      !agent.isAvailable || assigningId === agent.userId
                    "
                    @click="doAssign(agent.userId)"
                    class="flex-shrink-0 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all"
                    :class="
                      agent.isAvailable
                        ? 'bg-[#14B8A6] text-white hover:bg-teal-600 shadow-sm'
                        : 'bg-gray-100 dark:bg-white/5 text-gray-400 dark:text-gray-600 cursor-not-allowed'
                    "
                  >
                    <span v-if="assigningId === agent.userId">...</span>
                    <span v-else>Assign</span>
                  </button>
                </div>
              </div>
            </div>

            <!-- Modal footer -->
            <div
              class="px-6 py-3 border-t border-gray-100 dark:border-white/[0.08] flex justify-end"
            >
              <button
                @click="showAssignModal = false"
                class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ─── Escalation Modal ─── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showEscalateModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showEscalateModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-md p-6 border border-gray-100 dark:border-white/[0.08]"
          >
            <div class="flex items-center gap-3 mb-2">
              <div
                class="w-10 h-10 rounded-full bg-orange-100 dark:bg-orange-900/30 flex items-center justify-center flex-shrink-0"
              >
                <AlertTriangle :size="18" class="text-orange-600" />
              </div>
              <div>
                <h3 class="font-bold text-[#0F172A] dark:text-white">
                  Escalate Ticket
                </h3>
                <p class="text-xs text-gray-400">
                  Escalating will flag this ticket as urgent and notify the
                  manager immediately
                </p>
              </div>
            </div>

            <div
              class="mb-4 mt-4 px-3 py-2.5 bg-orange-50 dark:bg-orange-900/10 border border-orange-200 dark:border-orange-800/40 rounded-lg text-xs text-orange-700 dark:text-orange-400"
            >
              This action is permanent and will change the ticket status to
              Escalated.
            </div>

            <div class="mb-4">
              <label
                class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
              >
                Reason <span class="text-red-500">*</span>
              </label>
              <RichTextEditor
                v-model="escalateReason"
                placeholder="Explain why this ticket needs escalation..."
                min-height="100px"
              />
              <div class="flex items-center justify-between mt-1.5">
                <p v-if="escalateError" class="text-xs text-red-500">
                  {{ escalateError }}
                </p>
                <p
                  class="text-[11px] ml-auto"
                  :class="
                    escalateCharCount < 20
                      ? 'text-red-400'
                      : 'text-gray-400 dark:text-gray-500'
                  "
                >
                  {{ escalateCharCount }}/20 min characters
                </p>
              </div>
            </div>

            <div class="flex justify-end gap-2">
              <button
                @click="showEscalateModal = false"
                class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                Cancel
              </button>
              <button
                @click="doEscalate"
                :disabled="escalating || escalateCharCount < 20"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-orange-500 text-white text-sm font-semibold hover:bg-orange-600 disabled:opacity-60 transition-colors"
              >
                <LoadingSpinner v-if="escalating" size="sm" />
                <AlertTriangle v-else :size="14" />
                {{ escalating ? "Escalating..." : "Escalate Ticket" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ─── Delete Confirmation Modal ─── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showDeleteModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showDeleteModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-sm p-6 border border-gray-100 dark:border-white/[0.08]"
          >
            <div class="text-center mb-5">
              <div
                class="w-12 h-12 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center mx-auto mb-3"
              >
                <Trash2 :size="20" class="text-red-600" />
              </div>
              <h3 class="font-bold text-[#0F172A] dark:text-white mb-1">
                Delete Ticket
              </h3>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Are you sure you want to delete
                <strong>{{ ticket?.referenceNumber }}</strong
                >?
              </p>
              <p class="text-xs text-red-500 mt-1 font-medium">
                This action cannot be undone.
              </p>
            </div>
            <div class="flex gap-2">
              <button
                @click="showDeleteModal = false"
                class="flex-1 px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 border border-gray-200 dark:border-gray-700 transition-colors"
              >
                Cancel
              </button>
              <button
                @click="doDelete"
                :disabled="deleting"
                class="flex-1 inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg bg-red-500 text-white text-sm font-semibold hover:bg-red-600 disabled:opacity-60 transition-colors"
              >
                <LoadingSpinner v-if="deleting" size="sm" />
                {{ deleting ? "Deleting..." : "Delete" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  ArrowLeft,
  Copy,
  Tag,
  Pencil,
  RefreshCw,
  AlertTriangle,
  Trash2,
  ChevronDown,
  Check,
  MessageSquare,
  Paperclip,
  Activity,
  AlertCircle,
  UserCheck,
  LayoutDashboard,
  FileText,
  Bell,
  User,
  BarChart2,
  X,
} from "lucide-vue-next";

import AppLayout from "../../components/layout/AppLayout.vue";
import StatusBadge from "../../components/tickets/StatusBadge.vue";
import PriorityBadge from "../../components/tickets/PriorityBadge.vue";
import EscalatedBadge from "../../components/tickets/EscalatedBadge.vue";
import CommentThread from "../../components/tickets/CommentThread.vue";
import CommentEditor from "../../components/tickets/CommentEditor.vue";
import AttachmentList from "../../components/tickets/AttachmentList.vue";
import StatusTimeline from "../../components/tickets/StatusTimeline.vue";
import RichTextEditor from "../../components/ui/RichTextEditor.vue";
import LoadingSpinner from "../../components/ui/LoadingSpinner.vue";
import { useTicketStore } from "../../store/ticket";
import { useAuthStore } from "../../store/auth";
import { useToastStore } from "../../store/toast";
import { ticketApi } from "../../api/ticketApi";

const route = useRoute();
const router = useRouter();
const ticketStore = useTicketStore();
const authStore = useAuthStore();
const toastStore = useToastStore();

const ticketId = computed(() => route.params.id);
const ticket = computed(() => ticketStore.currentTicket);

const loading = ref(true);
const loadError = ref("");
const comments = ref([]);
const commentsLoading = ref(false);

// Agent assign modal state
const agents = ref([]);
const agentsLoading = ref(false);
const agentsError = ref("");
const showAssignModal = ref(false);
const assigningId = ref(null);

const saving = ref(false);
const deleting = ref(false);
const escalating = ref(false);

const editMode = ref(false);
const editForm = ref({
  title: "",
  description: "",
  categoryId: "",
  priorityId: "",
});
const editDescRef = ref(null);

const showStatusDrop = ref(false);
const showEscalateModal = ref(false);
const showDeleteModal = ref(false);
const statusDropRef = ref(null);

const escalateReason = ref("");
const escalateError = ref("");

const escalateCharCount = computed(
  () => escalateReason.value.replace(/<[^>]*>/g, "").trim().length
);

const role = computed(() => authStore.userRole);

const navLinks = computed(() => {
  const map = {
    Admin: [
      { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/admin" },
      { icon: FileText, label: "Tickets", to: "/tickets" },
      { icon: User, label: "Profile", to: "/profile" },
    ],
    Manager: [
      { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/manager" },
      { icon: FileText, label: "All Tickets", to: "/tickets" },
      { icon: BarChart2, label: "Reports", to: "/reports" },
      { icon: Bell, label: "Notifications", to: "/notifications" },
      { icon: User, label: "Profile", to: "/profile" },
    ],
    Agent: [
      { icon: LayoutDashboard, label: "Dashboard", to: "/dashboard/agent" },
      { icon: FileText, label: "My Tickets", to: "/tickets" },
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

const availableStatuses = computed(() => ticketStore.statuses);

const statusBorderClass = computed(() => {
  const map = {
    Open: "border-l-4 border-l-blue-400 border-gray-100 dark:border-white/[0.06]",
    "In Progress":
      "border-l-4 border-l-teal-400 border-gray-100 dark:border-white/[0.06]",
    Pending:
      "border-l-4 border-l-yellow-400 border-gray-100 dark:border-white/[0.06]",
    Resolved:
      "border-l-4 border-l-green-400 border-gray-100 dark:border-white/[0.06]",
    Closed:
      "border-l-4 border-l-gray-400 border-gray-100 dark:border-white/[0.06]",
    Escalated:
      "border-l-4 border-l-orange-400 border-gray-100 dark:border-white/[0.06]",
  };
  return (
    map[ticket.value?.status] ||
    "border border-gray-100 dark:border-white/[0.06]"
  );
});

onMounted(async () => {
  await Promise.all([loadTicket(), ticketStore.fetchLookups()]);
  document.addEventListener("click", closeStatusDrop);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", closeStatusDrop);
});

function closeStatusDrop(e) {
  if (statusDropRef.value && !statusDropRef.value.contains(e.target)) {
    showStatusDrop.value = false;
  }
}

async function loadTicket() {
  loading.value = true;
  loadError.value = "";
  try {
    await ticketStore.fetchTicket(ticketId.value);
    await loadComments();
  } catch (e) {
    loadError.value = e.response?.data?.message || "Failed to load ticket";
  } finally {
    loading.value = false;
  }
}

async function loadComments() {
  commentsLoading.value = true;
  try {
    const res = await ticketApi.getComments(ticketId.value);
    comments.value = res.data;
  } catch {
    /* silent */
  } finally {
    commentsLoading.value = false;
  }
}

async function openAssignModal() {
  showAssignModal.value = true;
  if (agents.value.length === 0) {
    agentsLoading.value = true;
    agentsError.value = "";
    try {
      const res = await ticketApi.getAgentsAvailability(ticketId.value);
      agents.value = res.data;
    } catch {
      agentsError.value = "Failed to load agents. Please try again.";
    } finally {
      agentsLoading.value = false;
    }
  }
}

function startEdit() {
  editForm.value = {
    title: ticket.value.title,
    description: ticket.value.description || "",
    categoryId: ticket.value.categoryId || "",
    priorityId: ticket.value.priorityId || "",
  };
  editMode.value = true;
}

async function saveEdit() {
  if (saving.value) return;
  saving.value = true;
  try {
    await ticketStore.updateTicket(ticketId.value, editForm.value);
    editMode.value = false;
    toastStore.show("Ticket updated successfully");
  } catch {
    toastStore.show("Failed to update ticket", "error");
  } finally {
    saving.value = false;
  }
}

async function doUpdateStatus(statusName) {
  showStatusDrop.value = false;
  try {
    await ticketApi.updateStatus(ticketId.value, statusName);
    toastStore.show(`Status updated to ${statusName}`);
    await loadTicket();
  } catch {
    toastStore.show("Failed to update status", "error");
  }
}

async function doAssign(agentUserId) {
  if (assigningId.value) return;
  assigningId.value = agentUserId;
  try {
    await ticketApi.assignTicket(ticketId.value, agentUserId);
    toastStore.show("Ticket assigned successfully");
    showAssignModal.value = false;
    agents.value = [];
    await loadTicket();
  } catch {
    toastStore.show("Failed to assign ticket", "error");
  } finally {
    assigningId.value = null;
  }
}

async function doEscalate() {
  if (escalateCharCount.value < 20) {
    escalateError.value = "Reason must be at least 20 characters";
    return;
  }
  escalateError.value = "";
  escalating.value = true;
  try {
    await ticketApi.escalateTicket(ticketId.value, escalateReason.value);
    toastStore.show("Ticket escalated successfully");
    showEscalateModal.value = false;
    escalateReason.value = "";
    await loadTicket();
  } catch {
    toastStore.show("Failed to escalate ticket", "error");
  } finally {
    escalating.value = false;
  }
}

async function doDelete() {
  if (deleting.value) return;
  deleting.value = true;
  try {
    await ticketStore.deleteTicket(ticketId.value);
    toastStore.show("Ticket deleted");
    router.push("/tickets");
  } catch {
    toastStore.show("Failed to delete ticket", "error");
    deleting.value = false;
    showDeleteModal.value = false;
  }
}

async function onCommentAdded() {
  await loadComments();
  await ticketStore.fetchTicket(ticketId.value);
}

function copyRef() {
  navigator.clipboard
    .writeText(ticket.value?.referenceNumber || "")
    .catch(() => {});
  toastStore.show("Reference copied to clipboard", "info");
}

function formatDate(dateStr) {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleDateString("en-US", {
    month: "long",
    day: "numeric",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

// Agent modal helpers
function agentInitials(name) {
  return (name || "")
    .split(" ")
    .map((n) => n[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();
}

function workloadBarColor(count) {
  if (count < 2) return "bg-green-500";
  if (count === 2) return "bg-yellow-500";
  return "bg-red-500";
}

function agentBorderClass(agent) {
  if (!agent.isAvailable)
    return "border-red-200 dark:border-red-800/40 bg-red-50/30 dark:bg-red-900/5";
  if (agent.openTickets >= 2)
    return "border-yellow-200 dark:border-yellow-800/40 bg-yellow-50/30 dark:bg-yellow-900/5";
  return "border-green-200 dark:border-green-800/40 bg-green-50/30 dark:bg-green-900/5";
}

function agentAvatarClass(agent) {
  if (!agent.isAvailable)
    return "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400";
  if (agent.openTickets >= 2)
    return "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400";
  return "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400";
}

function agentBadgeClass(agent) {
  if (!agent.isAvailable)
    return "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400";
  if (agent.openTickets >= 2)
    return "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400";
  return "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400";
}

function agentAvailabilityLabel(agent) {
  if (!agent.isAvailable) return "Full";
  if (agent.openTickets >= 2) return "Busy";
  return "Available";
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

.prose :deep(ul) {
  list-style-type: disc;
  padding-left: 1.5em;
}
.prose :deep(ol) {
  list-style-type: decimal;
  padding-left: 1.5em;
}
.prose :deep(strong) {
  font-weight: 600;
}
.prose :deep(em) {
  font-style: italic;
}
</style>
