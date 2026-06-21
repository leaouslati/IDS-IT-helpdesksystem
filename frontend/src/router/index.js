import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../store/auth";

export function getRoleDashboard(role) {
  const map = {
    Admin: "/dashboard/admin",
    Manager: "/dashboard/manager",
    Agent: "/dashboard/agent",
    Employee: "/dashboard/employee",
  };
  return map[role] || "/dashboard/admin";
}

const routes = [
  {
    path: "/",
    redirect: "/login",
  },
  {
    path: "/login",
    name: "Login",
    component: () => import("../views/LoginView.vue"),
    meta: { requiresGuest: true },
  },
  {
    path: "/dashboard",
    redirect: () => {
      const authStore = useAuthStore();
      return getRoleDashboard(authStore.userRole);
    },
  },
  {
    path: "/dashboard/admin",
    name: "AdminDashboard",
    component: () => import("../views/dashboards/AdminDashboard.vue"),
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/admin/users",
    name: "AdminUsers",
    component: () => import("../views/admin/UsersView.vue"),
    meta: { requiresAuth: true, roles: ["Admin"] },
  },
  {
    path: "/dashboard/manager",
    name: "ManagerDashboard",
    component: () => import("../views/dashboards/ManagerDashboard.vue"),
    meta: { requiresAuth: true, roles: ["Manager"] },
  },
  {
    path: "/dashboard/agent",
    name: "AgentDashboard",
    component: () => import("../views/dashboards/AgentDashboard.vue"),
    meta: { requiresAuth: true, roles: ["Agent"] },
  },
  {
    path: "/dashboard/employee",
    name: "EmployeeDashboard",
    component: () => import("../views/dashboards/EmployeeDashboard.vue"),
    meta: { requiresAuth: true, roles: ["Employee"] },
  },
  {
    path: "/forgot-password",
    name: "ForgotPassword",
    component: () => import("../views/ForgotPasswordView.vue"),
    meta: { requiresGuest: true },
  },
  {
    path: "/reset-password",
    name: "ResetPassword",
    component: () => import("../views/ResetPasswordView.vue"),
    meta: { requiresGuest: true },
  },
  {
    path: "/tickets",
    name: "Tickets",
    component: () => import("../views/tickets/TicketsView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/tickets/create",
    name: "CreateTicket",
    component: () => import("../views/tickets/CreateTicketView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/tickets/:id",
    name: "TicketDetail",
    component: () => import("../views/tickets/TicketDetailView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/knowledge-base",
    name: "KnowledgeBase",
    component: () => import("../views/KnowledgeBaseView.vue"),
    meta: { requiresAuth: true },
  },
  {
    path: "/unauthorized",
    name: "Unauthorized",
    component: () => import("../views/UnauthorizedView.vue"),
  },
  {
    path: "/:pathMatch(.*)*",
    name: "NotFound",
    component: () => import("../views/NotFoundView.vue"),
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore();

  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    next("/login");
    return;
  }

  if (to.meta.requiresGuest && authStore.isLoggedIn) {
    next(getRoleDashboard(authStore.userRole));
    return;
  }

  // Role-based access check
  if (
    to.meta.roles &&
    authStore.userRole &&
    !to.meta.roles.includes(authStore.userRole)
  ) {
    next(getRoleDashboard(authStore.userRole));
    return;
  }

  next();
});

export default router;
