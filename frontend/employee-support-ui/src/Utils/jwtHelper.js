// Read user info from the JWT stored in localStorage
const decode = () => {
  const token = localStorage.getItem("token");
  if (!token) return null;
  try {
    return JSON.parse(atob(token.split(".")[1]));
  } catch {
    return null;
  }
};

export const getEmployeeId = () => decode()?.EmployeeId ?? null;
export const getRole = () =>
  decode()?.role ??
  decode()?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ??
  "Employee";
export const getUsername = () =>
  decode()?.name ??
  decode()?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ??
  "";
