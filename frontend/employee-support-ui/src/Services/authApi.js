import apiClient from "./apiClient";

export const login = (username, password) =>
  apiClient.post("/auth/login", { username, password }).then((r) => r.data);

/**
 * Persist login response into localStorage so the UI can render without
 * re-hitting /api/employees/me on every reload. The token is required for
 * the axios interceptor; the user blob is used by Header/Sidebar/Dashboard.
 */
export const persistLogin = (data) => {
  try {
    if (data?.token) localStorage.setItem("token", data.token);
    if (data?.user) localStorage.setItem("user", JSON.stringify(data.user));
  } catch {
    // ignore storage failures
  }
};

export const logout = () => {
  try {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
  } catch {
    // ignore
  }
};

export const changePassword = (payload) =>
  apiClient.post("/auth/change-password", payload).then((r) => r.data);

export const me = () => apiClient.get("/employees/me").then((r) => r.data);
