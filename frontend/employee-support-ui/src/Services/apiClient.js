import axios from "axios";

const baseURL = import.meta.env.VITE_API_URL || "http://localhost:5035/api";

const apiClient = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Retry agent/chat once on network errors. All other failures pass through.
const AGENT_RETRY_DELAY = 750;
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error.response?.status;

    if (status === 401) {
      try {
        // Remember where the user was so they land back here after re-login.
        const path = window.location.pathname + window.location.search;
        if (path && path !== "/") {
          sessionStorage.setItem("postLoginRedirect", path);
        }
        localStorage.removeItem("token");
        localStorage.removeItem("user");
      } catch {
        // ignore storage failures (private mode, etc.)
      }
      if (window.location.pathname !== "/") {
        window.location.href = "/";
      }
      return Promise.reject(error);
    }

    // One-shot retry for /agent/chat on network errors / 5xx
    const cfg = error.config || {};
    const isAgent = (cfg.url || "").includes("/agent/chat");
    const isRetryable = !error.response || error.response.status >= 500;
    if (isAgent && isRetryable && !cfg.__agentRetried) {
      cfg.__agentRetried = true;
      await new Promise((r) => setTimeout(r, AGENT_RETRY_DELAY));
      return apiClient.request(cfg);
    }

    return Promise.reject(error);
  }
);

export default apiClient;
