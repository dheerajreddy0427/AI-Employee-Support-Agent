export const formatDate = (value) => {
  if (!value) return "—";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return value;
  return d.toLocaleDateString("en-US", { year: "numeric", month: "short", day: "numeric" });
};

export const formatDateTime = (value) => {
  if (!value) return "—";
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return value;
  return d.toLocaleString("en-US", { year: "numeric", month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" });
};

export const formatMoney = (value) => {
  if (value === null || value === undefined) return "—";
  const n = Number(value);
  if (Number.isNaN(n)) return value;
  return n.toLocaleString("en-US", { style: "currency", currency: "INR", maximumFractionDigits: 2 });
};

export const statusBadge = (status) => {
  const s = (status || "").toLowerCase();
  if (s === "approved" || s === "open" || s === "active") return "success";
  if (s === "pending" || s === "in review" || s === "in-review") return "warning";
  if (s === "rejected" || s === "closed" || s === "cancelled") return "danger";
  if (s === "in progress" || s === "in-progress") return "info";
  return "primary";
};

export const initials = (name) => {
  if (!name) return "?";
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
};
