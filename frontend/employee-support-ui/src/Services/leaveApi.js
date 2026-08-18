import apiClient from "./apiClient";

export const applyLeave = (payload) =>
  apiClient.post("/leave/apply", payload).then((r) => r.data);

export const myLeaves = () =>
  apiClient.get("/leave/history").then((r) => r.data);

export const leavesByEmployee = (id) =>
  apiClient.get(`/leave/history/${id}`).then((r) => r.data);

export const pendingLeaves = () =>
  apiClient.get("/leave/pending").then((r) => r.data);

export const approveLeave = (payload) =>
  apiClient.put("/leave/approve", payload).then((r) => r.data);

export const rejectLeave = (payload) =>
  apiClient.put("/leave/reject", payload).then((r) => r.data);
