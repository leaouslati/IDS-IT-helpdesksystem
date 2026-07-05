import {
  LayoutDashboard,
  FileText,
  Users,
  BarChart2,
  Bell,
  BookOpen,
  Server,
  User,
} from "lucide-vue-next";

const LINKS = {
  Admin: [
    { icon: LayoutDashboard, label: "Dashboard",     to: "/dashboard/admin" },
    { icon: FileText,        label: "All Tickets",   to: "/tickets" },
    { icon: Users,           label: "Users",         to: "/admin/users" },
    { icon: BarChart2,       label: "Reports",       to: "/reports" },
    { icon: Bell,            label: "Notifications", to: "/notifications" },
    { icon: Server,          label: "System",        to: "/admin/system" },
    { icon: User,            label: "Profile",       to: "/profile" },
  ],
  Manager: [
    { icon: LayoutDashboard, label: "Dashboard",     to: "/dashboard/manager" },
    { icon: FileText,        label: "All Tickets",   to: "/tickets" },
    { icon: BookOpen,        label: "Knowledge Base",to: "/knowledge-base" },
    { icon: BarChart2,       label: "Reports",       to: "/reports" },
    { icon: Bell,            label: "Notifications", to: "/notifications" },
    { icon: User,            label: "Profile",       to: "/profile" },
  ],
  Agent: [
    { icon: LayoutDashboard, label: "Dashboard",     to: "/dashboard/agent" },
    { icon: FileText,        label: "My Tickets",    to: "/tickets" },
    { icon: Bell,            label: "Notifications", to: "/notifications" },
    { icon: User,            label: "Profile",       to: "/profile" },
  ],
  Employee: [
    { icon: LayoutDashboard, label: "Dashboard",     to: "/dashboard/employee" },
    { icon: FileText,        label: "My Tickets",    to: "/tickets" },
    { icon: Bell,            label: "Notifications", to: "/notifications" },
    { icon: User,            label: "Profile",       to: "/profile" },
  ],
};

/** Returns the nav links array for the given role. Falls back to Employee. */
export function getNavLinks(role) {
  return LINKS[role] ?? LINKS.Employee;
}
