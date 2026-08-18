import apiClient from "./apiClient";

export const sendMessage = (message) =>
  apiClient.post("/agent/chat", { message }).then((r) => r.data);

export const getHistory = () =>
  apiClient.get("/chat/history").then((r) => r.data);

export const clearHistory = () =>
  apiClient.delete("/chat/history").then((r) => r.data);
