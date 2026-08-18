import { NavLink } from "react-router-dom";
import { getRole, getUsername } from "../Utils/jwtHelper";
import Avatar from "./Avatar";

const COMMON = [
  { to: "/dashboard", label: "Dashboard", icon: "🏠" },
  { to: "/chat", label: "AI Assistant", icon: "💬" },
  { to: "/leaves", label: "Apply Leave", icon: "📝" },
  { to: "/my-leaves", label: "My Leaves", icon: "📅" },
  { to: "/payslips", label: "Payslips", icon: "📄" },
  { to: "/tickets", label: "IT Tickets", icon: "🎫" },
  { to: "/reimbursements", label: "Reimbursements", icon: "💰" },
  { to: "/profile", label: "Profile", icon: "👤" }
];

const APPROVALS = { to: "/admin/approvals", label: "Approvals", icon: "✅" };
const ALL_TICKETS = { to: "/admin/tickets", label: "All Tickets", icon: "🧰" };
const EMPLOYEES = { to: "/admin/employees", label: "Employees", icon: "👥" };

const ROLE_LINKS = {
  Employee: COMMON,
  Manager: [...COMMON.slice(0, 7), APPROVALS, COMMON[7]],
  HR: [...COMMON.slice(0, 7), APPROVALS, ALL_TICKETS, EMPLOYEES, COMMON[7]],
  Admin: [...COMMON.slice(0, 7), APPROVALS, ALL_TICKETS, EMPLOYEES, COMMON[7]]
};

export default function Sidebar() {
  const role = getRole() || "Employee";
  const username = getUsername() || "User";
  const links = ROLE_LINKS[role] || ROLE_LINKS.Employee;

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <div className="logo-dot">AI</div>
        <div>
          <div>HR Assistant</div>
          <div style={{ fontSize: 11, fontWeight: 500, color: "rgba(255,255,255,.5)" }}>
            Employee Support
          </div>
        </div>
      </div>

      <div className="sidebar-section">Navigation</div>
      <nav style={{ display: "flex", flexDirection: "column", gap: 4 }}>
        {links.map((l) => (
          <NavLink
            key={l.to}
            to={l.to}
            className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
          >
            <span className="icon">{l.icon}</span>
            <span>{l.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-footer">
        <div style={{ display: "flex", alignItems: "center", gap: 12, padding: "12px 10px" }}>
          <Avatar name={username} />
          <div style={{ minWidth: 0 }}>
            <div style={{ color: "#fff", fontWeight: 600, fontSize: 14, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
              {username}
            </div>
            <div style={{ color: "rgba(255,255,255,.55)", fontSize: 12 }}>{role}</div>
          </div>
        </div>
      </div>
    </aside>
  );
}
