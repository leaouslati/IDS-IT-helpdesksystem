import axios from "axios";

export const API_ORIGIN = "https://localhost:7091";

const api = axios.create({
  baseURL: `${API_ORIGIN}/api`,
});

// It attaches the JWT token to the request automatically
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// If the token is expired or invalid, it logs the user out
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isLoginEndpoint = error.config?.url?.includes("/auth/login");
    if (error.response?.status === 401 && !isLoginEndpoint) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default api;
