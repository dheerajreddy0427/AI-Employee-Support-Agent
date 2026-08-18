import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { changePassword } from "../Services/authApi";
import { toast } from "../Components/Toast";

export default function ChangePassword() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ currentPassword: "", newPassword: "", confirmNewPassword: "" });
  const [loading, setLoading] = useState(false);

  const submit = async (e) => {
    e.preventDefault();
    if (!form.currentPassword || !form.newPassword || !form.confirmNewPassword) {
      toast.error("Please fill in all fields.");
      return;
    }
    if (form.newPassword.length < 8) {
      toast.error("New password must be at least 8 characters.");
      return;
    }
    if (form.newPassword !== form.confirmNewPassword) {
      toast.error("New password and confirmation do not match.");
      return;
    }
    setLoading(true);
    try {
      await changePassword(form);
      toast.success("Password changed successfully.");
      navigate("/profile", { replace: true });
    } catch (err) {
      toast.error(
        err?.response?.data?.detail ||
        err?.response?.data?.error ||
        "Failed to change password."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page" style={{ maxWidth: 520 }}>
      <h1 className="page-title">Change password</h1>
      <p className="page-subtitle">Use a strong password you don't reuse elsewhere.</p>

      <div className="card">
        <form onSubmit={submit} className="form">
          <div className="field">
            <label>Current password</label>
            <input
              type="password"
              className="input"
              value={form.currentPassword}
              onChange={(e) => setForm({ ...form, currentPassword: e.target.value })}
              autoComplete="current-password"
              required
            />
          </div>
          <div className="field">
            <label>New password</label>
            <input
              type="password"
              className="input"
              value={form.newPassword}
              onChange={(e) => setForm({ ...form, newPassword: e.target.value })}
              autoComplete="new-password"
              minLength={8}
              required
            />
          </div>
          <div className="field">
            <label>Confirm new password</label>
            <input
              type="password"
              className="input"
              value={form.confirmNewPassword}
              onChange={(e) => setForm({ ...form, confirmNewPassword: e.target.value })}
              autoComplete="new-password"
              minLength={8}
              required
            />
          </div>
          <button type="submit" className="btn btn-primary-solid" disabled={loading}>
            {loading ? "Updating..." : "Update password"}
          </button>
        </form>
      </div>
    </div>
  );
}
