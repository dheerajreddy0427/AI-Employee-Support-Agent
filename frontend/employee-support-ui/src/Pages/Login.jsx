import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login, persistLogin } from "../Services/authApi";

const DEMO_USERS = [
  { user: "EMP001", role: "Admin" },
  { user: "EMP002", role: "Manager" },
  { user: "EMP003", role: "HR" },
  { user: "EMP004", role: "Employee" }
];

export default function Login() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleLogin = async (e) => {
    e?.preventDefault?.();
    setError("");
    setLoading(true);
    try {
      const data = await login(username, password);
      persistLogin(data);

      // Send the user back to where they were before the 401 redirect, if any.
      const redirect = sessionStorage.getItem("postLoginRedirect");
      if (redirect) {
        sessionStorage.removeItem("postLoginRedirect");
        navigate(redirect, { replace: true });
      } else {
        navigate("/dashboard", { replace: true });
      }
    } catch (err) {
      const msg =
        err?.response?.data?.error ||
        err?.response?.data?.detail ||
        err?.response?.data?.message ||
        "Invalid username or password";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <div className="login-card">
        <div style={{ display: "flex", justifyContent: "center", marginBottom: 18 }}>
          <div
            style={{
              width: 72, height: 72, borderRadius: 20,
              background: "var(--gradient)",
              display: "grid", placeItems: "center",
              fontSize: 30, boxShadow: "0 12px 30px rgba(79, 70, 229, .4)"
            }}
          >
            🤖
          </div>
        </div>
        <h1 className="login-title" style={{ fontSize: 26, textAlign: "center" }}>Employee Support Agent</h1>
        <p className="login-subtitle" style={{ textAlign: "center" }}>AI-powered HR assistant — sign in to continue</p>

        <form onSubmit={handleLogin} className="form" style={{ marginTop: 20 }}>
          <div className="field">
            <label style={{ color: "rgba(255,255,255,.85)" }}>Username</label>
            <input
              className="login-input"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Enter your username"
              autoComplete="username"
              required
            />
          </div>
          <div className="field">
            <label style={{ color: "rgba(255,255,255,.85)" }}>Password</label>
            <input
              className="login-input"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              autoComplete="current-password"
              required
            />
          </div>
          {error && <div style={{ color: "#fecaca", fontSize: 13, textAlign: "center" }}>{error}</div>}
          <button className="btn-primary" type="submit" disabled={loading}>
            {loading ? <span className="spinner" style={{ borderTopColor: "var(--primary)" }} /> : null}
            {loading ? "Signing in..." : "Sign in"}
          </button>
        </form>
      </div>
    </div>
  );
}