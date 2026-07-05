<template>
  <AppLayout :navLinks="navLinks" pageTitle="Knowledge Base">
    <div class="space-y-6">
      <!-- Header -->
      <div>
        <h1 class="text-2xl font-bold text-[#0F172A] dark:text-white">
          IT Knowledge Base
        </h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
          Find answers to common IT issues before creating a ticket
        </p>
      </div>

      <!-- Search -->
      <div class="relative max-w-lg">
        <Search
          :size="16"
          class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 pointer-events-none"
        />
        <input
          v-model="search"
          type="text"
          placeholder="Search articles..."
          class="w-full pl-10 pr-4 py-2.5 text-sm bg-white dark:bg-[#1A1D2E] border border-gray-200 dark:border-white/[0.06] rounded-lg text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#14B8A6]/40 focus:border-[#14B8A6] shadow-sm transition-all"
        />
      </div>

      <!-- No results -->
      <div
        v-if="filteredArticles.length === 0"
        class="text-center py-12 text-sm text-gray-400 dark:text-gray-500"
      >
        No articles match
        <span class="font-medium text-gray-500 dark:text-gray-400"
          >"{{ search }}"</span
        >. Try a different keyword or
        <button @click="search = ''" class="text-[#14B8A6] hover:underline">
          clear the search</button
        >.
      </div>

      <!-- Articles grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <button
          v-for="article in filteredArticles"
          :key="article.id"
          class="bg-white dark:bg-[#1A1D2E] rounded-xl border border-gray-100 dark:border-white/[0.06] shadow-sm p-4 text-left flex items-start gap-3 hover:border-[#14B8A6]/40 hover:shadow-md transition-all duration-150 group cursor-pointer"
          @click="openModal(article)"
        >
          <!-- Icon -->
          <div
            class="w-9 h-9 rounded-lg flex items-center justify-center flex-shrink-0 mt-0.5"
            :style="{ backgroundColor: `${article.color}1a` }"
          >
            <component
              :is="article.icon"
              :size="16"
              :style="{ color: article.color }"
            />
          </div>

          <!-- Text -->
          <div class="flex-1 min-w-0">
            <p
              class="text-[13px] font-semibold text-[#0F172A] dark:text-white leading-snug group-hover:text-[#14B8A6] transition-colors"
            >
              {{ article.title }}
            </p>
            <p
              class="text-[12px] text-gray-400 dark:text-gray-500 mt-0.5 leading-relaxed"
            >
              {{ article.summary }}
            </p>
            <span
              class="inline-block mt-1.5 text-[10px] font-semibold uppercase tracking-wide px-1.5 py-0.5 rounded"
              :style="{
                backgroundColor: `${article.color}1a`,
                color: article.color,
              }"
            >
              {{ article.category }}
            </span>
          </div>

          <!-- Arrow hint -->
          <ArrowRight
            :size="14"
            class="text-gray-300 dark:text-gray-600 flex-shrink-0 mt-1 group-hover:text-[#14B8A6] transition-colors"
          />
        </button>
      </div>
    </div>

    <!-- Article modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div
          v-if="modalArticle"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <!-- Backdrop -->
          <div
            class="absolute inset-0 bg-black/50 backdrop-blur-sm"
            @click="closeModal"
          />

          <!-- Panel -->
          <div
            class="relative bg-white dark:bg-[#1A1D2E] rounded-2xl shadow-2xl w-full max-w-lg max-h-[85vh] flex flex-col border border-gray-100 dark:border-white/[0.08]"
          >
            <!-- Modal header -->
            <div
              class="flex items-start gap-3 p-5 border-b border-gray-100 dark:border-white/[0.06] flex-shrink-0"
            >
              <div
                class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                :style="{ backgroundColor: `${modalArticle.color}1a` }"
              >
                <component
                  :is="modalArticle.icon"
                  :size="18"
                  :style="{ color: modalArticle.color }"
                />
              </div>
              <div class="flex-1 min-w-0">
                <h3
                  class="text-[15px] font-bold text-[#0F172A] dark:text-white leading-snug"
                >
                  {{ modalArticle.title }}
                </h3>
                <span
                  class="inline-block mt-1 text-[10px] font-semibold uppercase tracking-wide px-1.5 py-0.5 rounded"
                  :style="{
                    backgroundColor: `${modalArticle.color}1a`,
                    color: modalArticle.color,
                  }"
                >
                  {{ modalArticle.category }}
                </span>
              </div>
              <button
                @click="closeModal"
                class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-white/5 transition-colors flex-shrink-0"
              >
                <X :size="16" />
              </button>
            </div>

            <!-- Modal body (scrollable) -->
            <div class="p-5 overflow-y-auto flex-1">
              <div
                class="text-[13px] text-gray-600 dark:text-gray-300 leading-relaxed space-y-2 article-body"
                v-html="modalArticle.body"
              />
            </div>

            <!-- Modal footer -->
            <div
              class="px-5 py-3 border-t border-gray-100 dark:border-white/[0.06] flex items-center justify-between gap-3 flex-shrink-0"
            >
              <p class="text-[11px] text-gray-400 dark:text-gray-500">
                Still need help?
                <router-link
                  to="/tickets/create"
                  class="text-[#14B8A6] hover:underline font-medium"
                  @click="closeModal"
                >
                  Open a support ticket
                </router-link>
              </p>
              <button
                @click="closeModal"
                class="px-3 py-1.5 rounded-lg text-sm font-medium text-gray-500 hover:bg-gray-100 dark:hover:bg-white/5 border border-gray-200 dark:border-gray-700 transition-colors"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </AppLayout>
</template>

<script setup>
import { ref, computed } from "vue";
import { useNavLinks } from "../composables/useNavLinks";
import {
  Search,
  ArrowRight,
  X,
  Wifi,
  KeyRound,
  Mail,
  Printer,
  Package,
  Globe,
  MonitorSmartphone,
} from "lucide-vue-next";
import AppLayout from "../components/layout/AppLayout.vue";

const { navLinks } = useNavLinks();

const search = ref("");
const modalArticle = ref(null);

function openModal(article) {
  modalArticle.value = article;
}

function closeModal() {
  modalArticle.value = null;
}

const articles = [
  {
    id: 1,
    category: "Network",
    icon: Wifi,
    color: "#3B82F6",
    title: "How to connect to the company VPN",
    summary:
      "Step-by-step guide to setting up and connecting to the company VPN from home or any remote location.",
    body: `
      <ol>
        <li>Download the VPN client from <strong>IT Portal → Downloads → GlobalProtect</strong>.</li>
        <li>Install and open the application.</li>
        <li>Enter the VPN gateway address: <code>vpn.company.com</code></li>
        <li>Sign in with your company email and Windows password.</li>
        <li>If prompted for MFA, approve the push notification on your registered phone.</li>
        <li>The status icon turns <strong>green</strong> when you are connected successfully.</li>
      </ol>
      <p class="note">If the connection times out repeatedly, check that your local firewall is not blocking the VPN port (UDP 4500), or open a ticket for IT to investigate.</p>
    `,
  },
  {
    id: 2,
    category: "Access",
    icon: KeyRound,
    color: "#8B5CF6",
    title: "How to reset your Windows or email password",
    summary:
      "Forgot your password? Use the self-service portal to reset it without waiting for IT support.",
    body: `
      <ol>
        <li>On the login screen, click <strong>Forgot Password</strong> (or use the link in this app).</li>
        <li>Enter your company email address and click <strong>Send Reset Link</strong>.</li>
        <li>Check your personal recovery inbox (the email registered with HR) for the reset message.</li>
        <li>Click the link — it expires in <strong>30 minutes</strong>.</li>
        <li>Enter and confirm your new password. It must be at least 8 characters and include a number and a special character.</li>
        <li>Log in with your new password.</li>
      </ol>
      <p class="note">If you are fully locked out with no recovery email on file, open an <em>Access Request</em> ticket and IT will verify your identity manually.</p>
    `,
  },
  {
    id: 3,
    category: "Email",
    icon: Mail,
    color: "#14B8A6",
    title: "Setting up Outlook on a new device",
    summary:
      "Configure your company email in Microsoft Outlook on Windows, Mac, or mobile in a few steps.",
    body: `
      <p><strong>Windows / Mac:</strong></p>
      <ol>
        <li>Open Outlook and click <strong>File → Add Account</strong>.</li>
        <li>Enter your company email address and click <strong>Connect</strong>.</li>
        <li>Select <strong>Microsoft 365</strong> as the account type.</li>
        <li>Sign in with your credentials and complete MFA if prompted.</li>
        <li>Click <strong>Done</strong> — your inbox will sync within a few minutes.</li>
      </ol>
      <p style="margin-top:0.75rem"><strong>Mobile (iOS / Android):</strong></p>
      <ol>
        <li>Install <strong>Microsoft Outlook</strong> from the App Store or Play Store.</li>
        <li>Tap <strong>Add Account</strong> and enter your company email.</li>
        <li>Authenticate with your password and complete MFA.</li>
      </ol>
      <p class="note">If your account shows authentication errors, your password may have recently expired. Try resetting it first.</p>
    `,
  },
  {
    id: 4,
    category: "Hardware",
    icon: Printer,
    color: "#F59E0B",
    title: "Printer not responding or showing offline",
    summary:
      "Common fixes for when a shared or local printer shows as offline, jams, or simply won't print.",
    body: `
      <p>Try these steps in order:</p>
      <ol>
        <li><strong>Check physical connections</strong> — ensure the printer is powered on and cables are secure.</li>
        <li><strong>Clear the print queue</strong> — go to <em>Settings → Printers → Open queue → Cancel all documents</em>.</li>
        <li><strong>Set as default</strong> — right-click the printer and select <em>Set as default printer</em>.</li>
        <li><strong>Restart the Print Spooler</strong> — open <em>Services</em> (Win + R → <code>services.msc</code>), find <em>Print Spooler</em>, right-click → <em>Restart</em>.</li>
        <li><strong>Remove and re-add the printer</strong> — go to <em>Settings → Printers &amp; Scanners → Remove device</em>, then add it again.</li>
        <li>If it is a network printer, confirm you are connected to the office Wi-Fi or VPN.</li>
      </ol>
      <p class="note">If the printer affects an entire floor and none of the above works, it may need a firmware update — open a Hardware ticket.</p>
    `,
  },
  {
    id: 5,
    category: "Software",
    icon: Package,
    color: "#10B981",
    title: "How to request software installation",
    summary:
      "Need a new application installed? Submit a request through the ticketing system for IT approval and scheduling.",
    body: `
      <ol>
        <li>Click <strong>Create New Ticket</strong> from your dashboard.</li>
        <li>Set <strong>Category</strong> to <em>Software</em>.</li>
        <li>Set <strong>Priority</strong> based on urgency (Low or Medium for non-critical tools).</li>
        <li>In the description, include:
          <ul>
            <li>The exact software name and version required</li>
            <li>A business justification for why you need it</li>
            <li>Whether a license already exists or needs to be purchased</li>
          </ul>
        </li>
        <li>Submit. An agent will review and schedule installation within the SLA window.</li>
      </ol>
      <p class="note">Unlicensed or unapproved software cannot be installed without manager authorization. Requests without a justification may be rejected.</p>
    `,
  },
  {
    id: 6,
    category: "Network",
    icon: Globe,
    color: "#EF4444",
    title: "Troubleshooting slow or no internet connection",
    summary:
      "Diagnose and fix common network issues including Wi-Fi drops, slow browsing, and complete loss of connectivity.",
    body: `
      <ol>
        <li><strong>Check your connection type</strong> — are you on Wi-Fi or wired? Try switching to the other.</li>
        <li><strong>Restart your network adapter</strong> — go to <em>Settings → Network → Disable → Enable</em>.</li>
        <li><strong>Forget and reconnect to Wi-Fi</strong> — especially useful after a network password change.</li>
        <li><strong>Flush DNS cache</strong> — open Command Prompt as administrator and run: <code>ipconfig /flushdns</code></li>
        <li><strong>Check if others are affected</strong> — if multiple users on the same floor have issues, it may be a switch or access point fault. Report to IT immediately.</li>
        <li><strong>Test a wired connection</strong> — plug directly into a wall Ethernet port to rule out Wi-Fi hardware.</li>
      </ol>
      <p class="note">If the issue is building-wide or affects more than 3 users simultaneously, it is classified as a Critical priority incident and should be escalated.</p>
    `,
  },
  {
    id: 7,
    category: "Software",
    icon: MonitorSmartphone,
    color: "#6366F1",
    title: "Connecting to your work PC remotely (RDP)",
    summary:
      "Access your office workstation from home using Remote Desktop Protocol — requires VPN to be active first.",
    body: `
      <p class="note"><strong>Before you start:</strong> Your office PC must be powered on and you must be connected to the company VPN (see the VPN guide above).</p>
      <ol>
        <li>Connect to the VPN first.</li>
        <li>Press <strong>Win + R</strong>, type <code>mstsc</code>, and press Enter to open Remote Desktop.</li>
        <li>Enter your office PC's <strong>hostname</strong> (e.g. <code>DESK-JD001</code>) — contact IT if you do not know it.</li>
        <li>Click <strong>Connect</strong> and sign in with your Windows username and password.</li>
        <li>Accept the certificate warning on first connection.</li>
        <li>Your office desktop will appear in the Remote Desktop window.</li>
      </ol>
      <p class="note">Remote Desktop must be pre-enabled on your PC by IT. If you receive a connection refused error, open an Access Request ticket to have it enabled.</p>
    `,
  },
];

const filteredArticles = computed(() => {
  if (!search.value.trim()) return articles;
  const q = search.value.toLowerCase();
  return articles.filter(
    (a) =>
      a.title.toLowerCase().includes(q) ||
      a.summary.toLowerCase().includes(q) ||
      a.category.toLowerCase().includes(q)
  );
});
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

.article-body :deep(ol) {
  list-style-type: decimal;
  padding-left: 1.25rem;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
}
.article-body :deep(ul) {
  list-style-type: disc;
  padding-left: 1.25rem;
  margin-top: 0.25rem;
}
.article-body :deep(li) {
  margin-bottom: 0.375rem;
}
.article-body :deep(code) {
  font-family: ui-monospace, monospace;
  font-size: 0.72rem;
  background-color: rgba(0, 0, 0, 0.07);
  padding: 0.1rem 0.35rem;
  border-radius: 0.25rem;
}
.dark .article-body :deep(code) {
  background-color: rgba(255, 255, 255, 0.12);
}
.article-body :deep(.note) {
  margin-top: 0.625rem;
  font-size: 0.72rem;
  color: #9ca3af;
}
.dark .article-body :deep(.note) {
  color: #6b7280;
}
</style>
