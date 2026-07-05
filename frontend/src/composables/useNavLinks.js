import { computed } from "vue";
import { useAuthStore } from "../store/auth";
import { getNavLinks } from "../config/navLinks";

/** Reactive composable — use in views that serve multiple roles. */
export function useNavLinks() {
  const authStore = useAuthStore();
  const navLinks = computed(() => getNavLinks(authStore.userRole));
  return { navLinks };
}
