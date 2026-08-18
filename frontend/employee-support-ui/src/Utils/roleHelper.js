// Role helpers — read from cached `user` first, then fall back to JWT.
// `getRole()` (in jwtHelper.js) is preferred when working off the token alone.
const stored = () => {
  try {
    return JSON.parse(localStorage.getItem("user") || "null");
  } catch {
    return null;
  }
};

export const getStoredUser = () => stored();

export const isHrOrAdmin = (role) => {
  const r = (role ?? stored()?.role ?? "").toLowerCase();
  return r === "hr" || r === "admin";
};

export const isApprover = (role) => {
  const r = (role ?? stored()?.role ?? "").toLowerCase();
  return r === "manager" || r === "hr" || r === "admin";
};

export const isAdmin = (role) => (role ?? stored()?.role ?? "").toLowerCase() === "admin";
