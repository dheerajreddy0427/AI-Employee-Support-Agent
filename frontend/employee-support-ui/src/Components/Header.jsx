import { useContext, useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ThemeContext } from "../ThemeContext";
import { getRole, getUsername, getEmployeeId } from "../Utils/jwtHelper";
import { logout as clearSession } from "../Services/authApi";
import Avatar from "./Avatar";

export default function Header() {
  const { darkMode, toggleTheme } = useContext(ThemeContext);
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const ref = useRef(null);
  const username = getUsername() || "User";
  const role = getRole() || "Employee";

  useEffect(() => {
    const onClick = (e) => {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener("mousedown", onClick);
    return () => document.removeEventListener("mousedown", onClick);
  }, []);

  const handleLogout = () => {
    clearSession();
    navigate("/", { replace: true });
  };

  const greeting = (() => {
    const h = new Date().getHours();
    if (h < 12) return "Good morning";
    if (h < 18) return "Good afternoon";
    return "Good evening";
  })();

  return (
    <header className="topbar">
      <div className="greeting">
        {greeting}, {username.split(/[._]/)[0]}
        <small>Here's what's happening with your account today</small>
      </div>
      <div className="topbar-actions">
        <button className="icon-btn" onClick={toggleTheme} title="Toggle theme">
          {darkMode ? "☀️" : "🌙"}
        </button>
        <div className="dropdown" ref={ref}>
          <button className="user-chip" onClick={() => setOpen((o) => !o)}>
            <Avatar name={username} />
            <div style={{ lineHeight: 1.2, textAlign: "left" }}>
              <div style={{ fontSize: 13, fontWeight: 600 }}>{username}</div>
              <div style={{ fontSize: 11, color: "var(--text-soft)" }}>{role}</div>
            </div>
          </button>
          {open && (
            <div className="dropdown-menu" style={{ minWidth: 220 }}>
              <button className="dropdown-item" onClick={() => { setOpen(false); navigate("/profile"); }}>
                👤 My profile
              </button>
              <button className="dropdown-item" onClick={() => { setOpen(false); navigate("/profile/change-password"); }}>
                🔑 Change password
              </button>
              <button className="dropdown-item" onClick={() => { setOpen(false); navigate("/dashboard"); }}>
                🏠 Dashboard
              </button>
              <div style={{ height: 1, background: "var(--border)", margin: "4px 6px" }} />
              <button className="dropdown-item danger" onClick={handleLogout}>
                🚪 Sign out
              </button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}
