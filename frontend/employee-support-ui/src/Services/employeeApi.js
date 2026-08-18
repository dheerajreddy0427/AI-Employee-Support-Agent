import apiClient from "./apiClient";

export const getMyProfile = () =>
  apiClient.get("/employees/me").then((r) => r.data);

export const getAllEmployees = () =>
  apiClient.get("/employees").then((r) => r.data);

export const getEmployee = (id) =>
  apiClient.get(`/employees/${id}`).then((r) => r.data);

export const createEmployee = (payload) =>
  apiClient.post("/employees", payload).then((r) => r.data);

export const updateEmployee = (id, payload) =>
  apiClient.put(`/employees/${id}`, payload).then((r) => r.data);
