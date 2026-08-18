import apiClient from "./apiClient";

export const myReimbursements = () =>
  apiClient.get("/reimbursements").then((r) => r.data);

export const pendingReimbursements = () =>
  apiClient.get("/reimbursements/pending").then((r) => r.data);

export const createReimbursement = (payload) =>
  apiClient.post("/reimbursements", payload).then((r) => r.data);

export const approveReimbursement = (id) =>
  apiClient.put(`/reimbursements/${id}/approve`).then((r) => r.data);

export const rejectReimbursement = (id) =>
  apiClient.put(`/reimbursements/${id}/reject`).then((r) => r.data);
