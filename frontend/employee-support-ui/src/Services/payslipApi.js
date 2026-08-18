import apiClient from "./apiClient";

export const myPayslips = () =>
  apiClient.get("/payslips/me").then((r) => r.data);
