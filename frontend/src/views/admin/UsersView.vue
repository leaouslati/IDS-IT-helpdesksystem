<template>
  <AppLayout :navLinks="navLinks" pageTitle="User Management">
    <!-- Header -->
    <div class="flex flex-wrap items-start justify-between gap-4 mb-6">
      <div>
        <h1 class="text-xl font-bold text-[#0F172A] dark:text-white">
          User Management
        </h1>
        <p class="text-sm text-gray-400 mt-0.5">
          Create, manage roles, and control access for all system users
        </p>
      </div>
      <button
        @click="openCreate"
        class="flex-shrink-0 inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 transition-colors"
      >
        <UserPlus :size="15" />
        Add User
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-[60vh]">
      <div class="flex flex-col items-center gap-4">
        <div
          class="w-12 h-12 border-4 border-[#14B8A6] border-t-transparent rounded-full animate-spin"
        />
        <p class="text-sm text-gray-400">Loading users...</p>
      </div>
    </div>

    <!-- Error -->
    <div
      v-else-if="loadError"
      class="flex items-center justify-center min-h-[60vh]"
    >
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl p-10 text-center max-w-sm shadow-sm border border-gray-100 dark:border-white/[0.05]"
      >
        <AlertCircle :size="44" class="text-red-500 mx-auto mb-3" />
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">
          {{ loadError }}
        </p>
        <button
          @click="fetchAll"
          class="px-5 py-2 bg-[#14B8A6] text-white rounded-lg text-sm font-semibold hover:bg-teal-600 transition-colors"
        >
          Try Again
        </button>
      </div>
    </div>

    <!-- Content -->
    <template v-else>
      <!-- Search + filter bar -->
      <div class="flex flex-wrap items-center gap-3 mb-4">
        <div class="relative flex-1 min-w-[200px]">
          <Search
            :size="14"
            class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
          />
          <input
            v-model="search"
            type="text"
            placeholder="Search by name or email…"
            class="w-full pl-9 pr-3 py-2 text-sm bg-white dark:bg-[#1A1D2E] border border-gray-200 dark:border-white/[0.08] rounded-lg text-gray-800 dark:text-gray-200 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
          />
        </div>
        <select
          v-model="filterRole"
          class="px-3 py-2 text-sm bg-white dark:bg-[#1A1D2E] border border-gray-200 dark:border-white/[0.08] rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all cursor-pointer"
        >
          <option value="">All Roles</option>
          <option v-for="r in roles" :key="r.id" :value="r.name">
            {{ r.name }}
          </option>
        </select>
        <select
          v-model="filterStatus"
          class="px-3 py-2 text-sm bg-white dark:bg-[#1A1D2E] border border-gray-200 dark:border-white/[0.08] rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all cursor-pointer"
        >
          <option value="">All Status</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </select>
        <span class="text-xs text-gray-400 ml-auto"
          >{{ filteredUsers.length }} user{{
            filteredUsers.length !== 1 ? "s" : ""
          }}</span
        >
      </div>

      <!-- Table -->
      <div
        class="bg-white dark:bg-[#1A1D2E] rounded-xl shadow-sm border border-gray-100 dark:border-white/[0.05] overflow-hidden"
      >
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="bg-gray-50 dark:bg-white/[0.03]">
                <th
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Name
                </th>
                <th
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Email
                </th>
                <th
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Role
                </th>
                <th
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Department
                </th>
                <th
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Status
                </th>
                <th
                  class="text-left px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Created
                </th>
                <th
                  class="text-right px-5 py-3 text-[11px] font-semibold text-gray-400 uppercase tracking-wider whitespace-nowrap"
                >
                  Actions
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-white/[0.04]">
              <tr
                v-for="user in filteredUsers"
                :key="user.id"
                class="hover:bg-gray-50 dark:hover:bg-white/[0.03] transition-colors"
              >
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <div class="flex items-center gap-3">
                    <div
                      class="w-8 h-8 rounded-full bg-teal-100 dark:bg-teal-900/30 text-teal-700 dark:text-teal-400 flex items-center justify-center text-xs font-bold flex-shrink-0"
                    >
                      {{ initials(user.firstName, user.lastName) }}
                    </div>
                    <span
                      class="text-[13px] font-medium text-[#0F172A] dark:text-gray-200"
                    >
                      {{ user.firstName }} {{ user.lastName }}
                    </span>
                  </div>
                </td>
                <td
                  class="px-5 py-3.5 text-[13px] text-gray-500 dark:text-gray-400 whitespace-nowrap"
                >
                  {{ user.email }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span
                    :class="roleClass(user.role)"
                    class="px-2 py-0.5 rounded-full text-[11px] font-semibold"
                    >{{ user.role }}</span
                  >
                </td>
                <td
                  class="px-5 py-3.5 text-[13px] text-gray-500 dark:text-gray-400 whitespace-nowrap"
                >
                  {{ user.department || "—" }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <span
                    class="px-2 py-0.5 rounded-full text-[11px] font-semibold"
                    :class="
                      user.isActive
                        ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                        : 'bg-gray-100 text-gray-500 dark:bg-white/10 dark:text-gray-400'
                    "
                  >
                    {{ user.isActive ? "Active" : "Inactive" }}
                  </span>
                </td>
                <td
                  class="px-5 py-3.5 text-[13px] text-gray-500 dark:text-gray-400 whitespace-nowrap"
                >
                  {{ formatDate(user.createdAt) }}
                </td>
                <td class="px-5 py-3.5 whitespace-nowrap">
                  <div class="flex items-center justify-end gap-1">
                    <!-- Change Role -->
                    <button
                      @click="openChangeRole(user)"
                      title="Change role"
                      class="p-1.5 rounded-lg text-gray-400 hover:text-[#14B8A6] hover:bg-teal-50 dark:hover:bg-teal-900/10 transition-colors"
                    >
                      <ShieldCheck :size="15" />
                    </button>
                    <!-- Toggle Active -->
                    <button
                      @click="handleToggle(user)"
                      :title="user.isActive ? 'Deactivate' : 'Activate'"
                      class="p-1.5 rounded-lg transition-colors"
                      :class="
                        user.isActive
                          ? 'text-gray-400 hover:text-orange-500 hover:bg-orange-50 dark:hover:bg-orange-900/10'
                          : 'text-gray-400 hover:text-green-600 hover:bg-green-50 dark:hover:bg-green-900/10'
                      "
                    >
                      <ToggleLeft v-if="!user.isActive" :size="15" />
                      <ToggleRight v-else :size="15" />
                    </button>
                    <!-- Delete -->
                    <button
                      @click="confirmDelete(user)"
                      title="Delete user"
                      class="p-1.5 rounded-lg text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/10 transition-colors"
                    >
                      <Trash2 :size="15" />
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredUsers.length === 0">
                <td
                  colspan="7"
                  class="text-center py-14 text-gray-400 dark:text-gray-500 text-sm"
                >
                  No users match your filters.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- ─── Create User Modal ─── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showCreateModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showCreateModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-lg border border-gray-100 dark:border-white/[0.08]"
          >
            <div
              class="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-white/[0.08]"
            >
              <div>
                <h3 class="font-bold text-[#0F172A] dark:text-white">
                  Create New User
                </h3>
                <p class="text-xs text-gray-400 mt-0.5">
                  Admin creates all accounts — no self-registration
                </p>
              </div>
              <button
                @click="showCreateModal = false"
                class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                <X :size="16" />
              </button>
            </div>
            <form @submit.prevent="handleCreate" class="px-6 py-5 space-y-4">
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label
                    class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                    >First Name *</label
                  >
                  <input
                    v-model="createForm.firstName"
                    type="text"
                    required
                    placeholder="Jane"
                    class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
                  />
                </div>
                <div>
                  <label
                    class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                    >Last Name *</label
                  >
                  <input
                    v-model="createForm.lastName"
                    type="text"
                    required
                    placeholder="Smith"
                    class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
                  />
                </div>
              </div>
              <div>
                <label
                  class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                  >Email Address *</label
                >
                <input
                  v-model="createForm.email"
                  type="email"
                  required
                  placeholder="jane.smith@company.com"
                  class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
                />
              </div>
              <div>
                <label
                  class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                  >Temporary Password *</label
                >
                <div class="relative">
                  <input
                    v-model="createForm.password"
                    :type="showNewPw ? 'text' : 'password'"
                    required
                    placeholder="Min. 8 characters"
                    class="w-full px-3 py-2.5 pr-10 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] transition-all"
                  />
                  <button
                    type="button"
                    @click="showNewPw = !showNewPw"
                    class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
                  >
                    <EyeOff v-if="showNewPw" :size="14" />
                    <Eye v-else :size="14" />
                  </button>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label
                    class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                    >Role *</label
                  >
                  <select
                    v-model="createForm.roleId"
                    required
                    class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all cursor-pointer appearance-none"
                  >
                    <option value="" disabled>Select role…</option>
                    <option v-for="r in roles" :key="r.id" :value="r.id">
                      {{ r.name }}
                    </option>
                  </select>
                </div>
                <div>
                  <label
                    class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1.5"
                    >Department</label
                  >
                  <select
                    v-model="createForm.departmentId"
                    class="w-full px-3 py-2.5 text-sm bg-gray-50 dark:bg-[#0F172A] border border-gray-200 dark:border-gray-700 rounded-lg text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 transition-all cursor-pointer appearance-none"
                  >
                    <option :value="null">None</option>
                    <option v-for="d in departments" :key="d.id" :value="d.id">
                      {{ d.name }}
                    </option>
                  </select>
                </div>
              </div>

              <div
                v-if="createError"
                class="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-red-50 dark:bg-red-900/10 border border-red-200 dark:border-red-800 text-sm text-red-600 dark:text-red-400"
              >
                <AlertCircle :size="14" class="flex-shrink-0" />
                {{ createError }}
              </div>

              <div class="flex justify-end gap-2 pt-1">
                <button
                  type="button"
                  @click="showCreateModal = false"
                  class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  :disabled="creating"
                  class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-60 transition-colors"
                >
                  <div
                    v-if="creating"
                    class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"
                  />
                  <UserPlus v-else :size="14" />
                  Create User
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ─── Change Role Modal ─── -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="showRoleModal"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="showRoleModal = false"
          />
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-sm border border-gray-100 dark:border-white/[0.08]"
          >
            <div
              class="flex items-center justify-between px-6 py-4 border-b border-gray-100 dark:border-white/[0.08]"
            >
              <div>
                <h3 class="font-bold text-[#0F172A] dark:text-white">
                  Change Role
                </h3>
                <p class="text-xs text-gray-400 mt-0.5">
                  {{ roleTarget?.firstName }} {{ roleTarget?.lastName }}
                </p>
              </div>
              <button
                @click="showRoleModal = false"
                class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                <X :size="16" />
              </button>
            </div>
            <div class="px-6 py-5 space-y-4">
              <div>
                <label
                  class="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-2"
                  >New Role</label
                >
                <div class="space-y-2">
                  <label
                    v-for="r in roles"
                    :key="r.id"
                    class="flex items-center gap-3 p-3 rounded-xl border cursor-pointer transition-all"
                    :class="
                      selectedRoleId === r.id
                        ? 'border-[#14B8A6] bg-teal-50 dark:bg-teal-900/10'
                        : 'border-gray-200 dark:border-white/[0.08] hover:border-gray-300 dark:hover:border-white/20'
                    "
                  >
                    <input
                      type="radio"
                      :value="r.id"
                      v-model="selectedRoleId"
                      class="accent-[#14B8A6]"
                    />
                    <span
                      class="text-sm font-medium text-[#0F172A] dark:text-gray-200"
                      >{{ r.name }}</span
                    >
                    <span
                      :class="roleClass(r.name)"
                      class="ml-auto px-2 py-0.5 rounded-full text-[11px] font-semibold"
                      >{{ r.name }}</span
                    >
                  </label>
                </div>
              </div>
              <div
                v-if="roleError"
                class="flex items-center gap-2 px-3 py-2.5 rounded-lg bg-red-50 dark:bg-red-900/10 border border-red-200 dark:border-red-800 text-sm text-red-600 dark:text-red-400"
              >
                <AlertCircle :size="14" class="flex-shrink-0" />{{ roleError }}
              </div>
              <div class="flex justify-end gap-2">
                <button
                  @click="showRoleModal = false"
                  class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
                >
                  Cancel
                </button>
                <button
                  @click="handleRoleChange"
                  :disabled="
                    roleSaving || selectedRoleId === roleTarget?.roleId
                  "
                  class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-[#14B8A6] text-white text-sm font-semibold hover:bg-teal-600 disabled:opacity-60 transition-colors"
                >
                  <div
                    v-if="roleSaving"
                    class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"
                  />
                  <ShieldCheck v-else :size="14" />
                  Save
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ─── Delete Confirm Modal ─── -->
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
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-sm border border-gray-100 dark:border-white/[0.08] p-6"
          >
            <div class="flex items-center gap-3 mb-3">
              <div
                class="w-10 h-10 rounded-full bg-red-100 dark:bg-red-900/20 flex items-center justify-center flex-shrink-0"
              >
                <Trash2 :size="18" class="text-red-500" />
              </div>
              <div>
                <h3 class="font-bold text-[#0F172A] dark:text-white">
                  Delete User?
                </h3>
                <p class="text-xs text-gray-400">
                  {{ deleteTarget?.firstName }} {{ deleteTarget?.lastName }}
                </p>
              </div>
            </div>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              This action is permanent. Users with any existing activity
              (tickets, comments, logs) cannot be deleted — deactivate them
              instead.
            </p>
            <div
              v-if="deleteError"
              class="flex items-start gap-2 px-3 py-2.5 rounded-lg bg-red-50 dark:bg-red-900/10 border border-red-200 dark:border-red-800 text-sm text-red-600 dark:text-red-400 mb-3"
            >
              <AlertCircle :size="14" class="flex-shrink-0 mt-0.5" />
              {{ deleteError }}
            </div>
            <div class="flex justify-end gap-2">
              <button
                @click="showDeleteModal = false"
                class="px-4 py-2 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors"
              >
                Cancel
              </button>
              <!-- After a blocked delete, offer Deactivate instead -->
              <button
                v-if="deleteError"
                @click="deactivateFromDeleteModal"
                :disabled="deleting"
                class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-amber-500 text-white text-sm font-semibold hover:bg-amber-600 disabled:opacity-60 transition-colors"
              >
                <div
                  v-if="deleting"
                  class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"
                />
                <ToggleRight v-else :size="14" />
                Deactivate
              </button>
              <button
                v-else
                @click="handleDelete"
                :disabled="deleting"
                class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-red-500 text-white text-sm font-semibold hover:bg-red-600 disabled:opacity-60 transition-colors"
              >
                <div
                  v-if="deleting"
                  class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"
                />
                <Trash2 v-else :size="14" />
                Delete
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import {
  UserPlus,
  Search,
  ShieldCheck,
  ToggleLeft,
  ToggleRight,
  Trash2,
  AlertCircle,
  X,
  Eye,
  EyeOff,
} from "lucide-vue-next";
import AppLayout from "../../components/layout/AppLayout.vue";
import { adminApi } from "../../api/adminApi";
import { useToastStore } from "../../store/toast";
import { getNavLinks } from "../../config/navLinks";

const toastStore = useToastStore();

const navLinks = getNavLinks("Admin");

// ── Data ────────────────────────────────────────────────────────────────────
const users = ref([]);
const roles = ref([]);
const departments = ref([]);
const loading = ref(true);
const loadError = ref("");

// ── Filters ─────────────────────────────────────────────────────────────────
const search = ref("");
const filterRole = ref("");
const filterStatus = ref("");

const filteredUsers = computed(() => {
  const q = search.value.trim().toLowerCase();
  return users.value.filter((u) => {
    const matchSearch =
      !q ||
      u.firstName.toLowerCase().includes(q) ||
      u.lastName.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q);
    const matchRole = !filterRole.value || u.role === filterRole.value;
    const matchStatus =
      !filterStatus.value ||
      (filterStatus.value === "active" ? u.isActive : !u.isActive);
    return matchSearch && matchRole && matchStatus;
  });
});

async function fetchAll() {
  loading.value = true;
  loadError.value = "";
  try {
    const [usersRes, rolesRes, deptsRes] = await Promise.all([
      adminApi.getUsers(),
      adminApi.getRoles(),
      adminApi.getDepartments(),
    ]);
    users.value = usersRes.data;
    roles.value = rolesRes.data;
    departments.value = deptsRes.data;
  } catch (e) {
    loadError.value = e.response?.data?.message || "Unable to load data.";
  } finally {
    loading.value = false;
  }
}

onMounted(fetchAll);

// ── Create User ──────────────────────────────────────────────────────────────
const showCreateModal = ref(false);
const creating = ref(false);
const createError = ref("");
const showNewPw = ref(false);
const createForm = ref({
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  roleId: "",
  departmentId: null,
});

function openCreate() {
  createForm.value = {
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    roleId: "",
    departmentId: null,
  };
  createError.value = "";
  showNewPw.value = false;
  showCreateModal.value = true;
}

async function handleCreate() {
  creating.value = true;
  createError.value = "";
  try {
    const res = await adminApi.createUser({
      firstName: createForm.value.firstName,
      lastName: createForm.value.lastName,
      email: createForm.value.email,
      password: createForm.value.password,
      roleId: Number(createForm.value.roleId),
      departmentId: createForm.value.departmentId || null,
    });
    users.value.unshift(res.data);
    showCreateModal.value = false;
    toastStore.show(
      `User ${res.data.firstName} ${res.data.lastName} created.`,
      "success"
    );
  } catch (e) {
    createError.value = e.response?.data?.message || "Failed to create user.";
  } finally {
    creating.value = false;
  }
}

// ── Toggle Active ────────────────────────────────────────────────────────────
async function handleToggle(user) {
  try {
    await adminApi.toggleActive(user.id);
    user.isActive = !user.isActive;
    toastStore.show(
      `${user.firstName} ${user.lastName} has been ${
        user.isActive ? "activated" : "deactivated"
      }.`,
      "success"
    );
  } catch (e) {
    toastStore.show(
      e.response?.data?.message || "Failed to update status.",
      "error"
    );
  }
}

// ── Change Role ──────────────────────────────────────────────────────────────
const showRoleModal = ref(false);
const roleTarget = ref(null);
const selectedRoleId = ref(null);
const roleSaving = ref(false);
const roleError = ref("");

function openChangeRole(user) {
  roleTarget.value = user;
  selectedRoleId.value = user.roleId;
  roleError.value = "";
  showRoleModal.value = true;
}

async function handleRoleChange() {
  roleSaving.value = true;
  roleError.value = "";
  try {
    await adminApi.updateUserRole(roleTarget.value.id, {
      roleId: selectedRoleId.value,
    });
    const newRole = roles.value.find((r) => r.id === selectedRoleId.value);
    const idx = users.value.findIndex((u) => u.id === roleTarget.value.id);
    if (idx !== -1) {
      users.value[idx] = {
        ...users.value[idx],
        roleId: selectedRoleId.value,
        role: newRole?.name ?? users.value[idx].role,
      };
    }
    showRoleModal.value = false;
    toastStore.show("Role updated successfully.", "success");
  } catch (e) {
    roleError.value = e.response?.data?.message || "Failed to update role.";
  } finally {
    roleSaving.value = false;
  }
}

// ── Delete ───────────────────────────────────────────────────────────────────
const showDeleteModal = ref(false);
const deleteTarget = ref(null);
const deleting = ref(false);
const deleteError = ref("");

function confirmDelete(user) {
  deleteTarget.value = user;
  deleteError.value = "";
  showDeleteModal.value = true;
}

async function handleDelete() {
  deleting.value = true;
  deleteError.value = "";
  try {
    await adminApi.deleteUser(deleteTarget.value.id);
    users.value = users.value.filter((u) => u.id !== deleteTarget.value.id);
    showDeleteModal.value = false;
    toastStore.show("User deleted.", "success");
  } catch (e) {
    deleteError.value = e.response?.data?.message || "Failed to delete user.";
  } finally {
    deleting.value = false;
  }
}

async function deactivateFromDeleteModal() {
  deleting.value = true;
  try {
    await adminApi.toggleActive(deleteTarget.value.id);
    const idx = users.value.findIndex((u) => u.id === deleteTarget.value.id);
    if (idx !== -1) users.value[idx] = { ...users.value[idx], isActive: false };
    showDeleteModal.value = false;
    toastStore.show(
      `${deleteTarget.value.firstName} ${deleteTarget.value.lastName} has been deactivated.`,
      "success"
    );
  } catch {
    deleteError.value = "Failed to deactivate user. Please try again.";
  } finally {
    deleting.value = false;
  }
}

// ── Helpers ──────────────────────────────────────────────────────────────────
function initials(first, last) {
  return ((first?.[0] ?? "") + (last?.[0] ?? "")).toUpperCase();
}

function formatDate(dateStr) {
  if (!dateStr) return "—";
  const normalized = /[Zz]|[+-]\d{2}:\d{2}$/.test(dateStr)
    ? dateStr
    : dateStr + "Z";
  return new Date(normalized).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function roleClass(role) {
  const map = {
    Admin:
      "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
    Manager: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    Agent: "bg-teal-100 text-teal-700 dark:bg-teal-900/30 dark:text-teal-400",
    Employee: "bg-gray-100 text-gray-600 dark:bg-white/10 dark:text-gray-400",
  };
  return map[role] ?? map.Employee;
}
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.15s ease;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
