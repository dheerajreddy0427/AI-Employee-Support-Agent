import { Navigate, Outlet } from "react-router-dom";
import { getRole } from "../Utils/jwtHelper";
import { getStoredUser } from "../Utils/roleHelper";

/**
 * Wraps a route element. If the current user's role is not in `allowedRoles`,
 * redirect to /dashboard. Otherwise renders <Outlet /> (for nested routes) or
 * the provided children.
 */
export default function RoleGuard({ allowedRoles, children }) {
  const role = (getRole() || getStoredUser()?.role || "Employee").toLowerCase();
  const allowed = allowedRoles.map((r) => r.toLowerCase());
  if (!allowed.includes(role)) {
    return <Navigate to="/dashboard" replace />;
  }
  return children ?? <Outlet />;
}
