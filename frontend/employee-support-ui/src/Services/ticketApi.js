import apiClient from "./apiClient";

export const myTickets = () =>
  apiClient.get("/tickets").then((r) => r.data);

export const allTickets = () =>
  apiClient.get("/tickets/all").then((r) => r.data);

export const createTicket = (payload) =>
  apiClient.post("/tickets", payload).then((r) => r.data);

export const updateTicketStatus = (id, payload) =>
  apiClient.put(`/tickets/${id}/status`, payload).then((r) => r.data);
