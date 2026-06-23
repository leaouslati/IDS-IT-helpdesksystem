import { ref } from "vue";
import { defineStore } from "pinia";
import * as signalR from "@microsoft/signalr";
import { notificationApi } from "../api/notificationApi";

export const useNotificationStore = defineStore("notification", () => {
  const notifications = ref([]);
  const unreadCount = ref(0);
  const page = ref(1);
  const hasMore = ref(true);
  const loading = ref(false);

  let hub = null;
  let pollTimer = null;

  async function fetchNotifications(p = 1) {
    loading.value = true;
    try {
      const res = await notificationApi.getNotifications(p);
      const { items, totalCount } = res.data;
      if (p === 1) {
        notifications.value = items;
      } else {
        notifications.value.push(...items);
      }
      page.value = p;
      hasMore.value = notifications.value.length < totalCount;
    } catch (err) {
      console.error("[notifications] fetchNotifications failed:", err?.response?.status, err?.message);
    } finally {
      loading.value = false;
    }
  }

  async function fetchUnreadCount() {
    try {
      const res = await notificationApi.getUnreadCount();
      unreadCount.value = res.data.count;
    } catch (err) {
      console.error("[notifications] fetchUnreadCount failed:", err?.response?.status, err?.message);
    }
  }

  async function markAsRead(id) {
    const n = notifications.value.find((n) => n.id === id);
    if (n && !n.isRead) {
      n.isRead = true;
      if (unreadCount.value > 0) unreadCount.value--;
    }
    try {
      await notificationApi.markAsRead(id);
    } catch {
      // non-fatal
    }
  }

  async function markAllAsRead() {
    notifications.value.forEach((n) => (n.isRead = true));
    unreadCount.value = 0;
    try {
      await notificationApi.markAllAsRead();
    } catch {
      // non-fatal
    }
  }

  function _pushNotification(notification) {
    notifications.value.unshift(notification);
    if (!notification.isRead) unreadCount.value++;
  }

  function _startPolling() {
    clearInterval(pollTimer);
    pollTimer = setInterval(fetchUnreadCount, 30000);
  }

  async function connectToHub() {
    const token = localStorage.getItem("token");
    if (!token || hub) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(
        `https://localhost:7091/hubs/notifications?access_token=${encodeURIComponent(token)}`
      )
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on("ReceiveNotification", _pushNotification);
    connection.onreconnected(() => {
      fetchNotifications(1);
      fetchUnreadCount();
    });

    try {
      await connection.start();
      hub = connection;
    } catch {
      _startPolling();
    }

    await Promise.all([fetchNotifications(1), fetchUnreadCount()]);
  }

  function disconnect() {
    if (hub) {
      hub.stop();
      hub = null;
    }
    clearInterval(pollTimer);
    pollTimer = null;
    notifications.value = [];
    unreadCount.value = 0;
    page.value = 1;
    hasMore.value = true;
  }

  return {
    notifications,
    unreadCount,
    page,
    hasMore,
    loading,
    fetchNotifications,
    fetchUnreadCount,
    markAsRead,
    markAllAsRead,
    connectToHub,
    disconnect,
  };
});
