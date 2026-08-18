import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getMyProfile, updateEmployee } from "../Services/employeeApi";
import { getStoredUser, isHrOrAdmin } from "../Utils/roleHelper";
import Avatar from "../Components/Avatar";
import { toast } from "../Components/Toast";

export default function Profile() {
  const stored = getStoredUser();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({});
  const [saving, setSaving] = useState(false);

  const isPrivileged = isHrOrAdmin(stored?.role);

  useEffect(() => {
    getMyProfile()
      .then((p) => {
        setProfile(p);
        setForm({
          fullName: p.fullName || "",
          email: p.email || "",
          department: p.department || "",
          leaveBalance: p.leaveBalance ?? 0,
          role: p.role || "Employee",
          employeeCode: p.employeeCode || ""
        });
      })
      .finally(() => setLoading(false));
  }, []);

  const save = async (e) => {
    e?.preventDefault();
    setSaving(true);
    try {
      const payload = {
        fullName: form.fullName,
        email: form.email,
        department: form.department
      };
      if (isPrivileged) {
        payload.leaveBalance = Number(form.leaveBalance);
        payload.role = form.role;
        payload.employeeCode = form.employeeCode;
      }
      const updated = await updateEmployee(profile.employeeId, payload);
      setProfile(updated);
      toast.success("Profile updated.");
      setEditing(false);
    } catch (err) {
      toast.error(
        err?.response?.data?.detail ||
        err?.response?.data?.error ||
        "Failed to update profile."
      );
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="page" style={{ display: "flex", justifyContent: "center", paddingTop: 80 }}>
        <span className="spinner lg" />
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="page">
        <div className="empty"><div className="big">⚠️</div><div>Profile unavailable</div></div>
      </div>
    );
  }

  return (
    <div className="page">
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 8 }}>
        <div>
          <h1 className="page-title">My profile</h1>
          <p className="page-subtitle">Your account details at a glance.</p>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <Link to="/profile/change-password" className="btn btn-soft">🔑 Change password</Link>
          {!editing ? (
            <button className="btn btn-primary-solid" onClick={() => setEditing(true)}>✏️ Edit</button>
          ) : (
            <>
              <button className="btn btn-soft" onClick={save} disabled={saving}>
                {saving ? "Saving..." : "💾 Save"}
              </button>
              <button
                className="btn btn-ghost"
                onClick={() => {
                  setEditing(false);
                  setForm({
                    fullName: profile.fullName || "",
                    email: profile.email || "",
                    department: profile.department || "",
                    leaveBalance: profile.leaveBalance ?? 0,
                    role: profile.role || "Employee",
                    employeeCode: profile.employeeCode || ""
                  });
                }}
              >
                Cancel
              </button>
            </>
          )}
        </div>
      </div>

      <div className="card" style={{ maxWidth: 760 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 18, marginBottom: 24 }}>
          <Avatar name={profile.fullName} size="lg" />
          <div>
            <div style={{ fontSize: 22, fontWeight: 700 }}>{profile.fullName}</div>
            <div style={{ color: "var(--text-soft)", fontSize: 14 }}>{profile.role} · {profile.department}</div>
            <div style={{ marginTop: 8 }}>
              <span className="badge primary">{profile.email}</span>
            </div>
          </div>
        </div>

        {!editing ? (
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
            <Detail label="Employee code" value={profile.employeeCode} />
            <Detail label="Email" value={profile.email} />
            <Detail label="Department" value={profile.department} />
            <Detail label="Role" value={profile.role} />
            <Detail label="Leave balance" value={`${profile.leaveBalance} days`} />
          </div>
        ) : (
          <form onSubmit={save} className="form">
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
              <Field label="Full name">
                <input className="input" required value={form.fullName}
                  onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
              </Field>
              <Field label="Email">
                <input className="input" type="email" required value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </Field>
              <Field label="Department">
                <input className="input" required value={form.department}
                  onChange={(e) => setForm({ ...form, department: e.target.value })} />
              </Field>
              {isPrivileged && (
                <>
                  <Field label="Employee code">
                    <input className="input" value={form.employeeCode}
                      onChange={(e) => setForm({ ...form, employeeCode: e.target.value })} />
                  </Field>
                  <Field label="Role">
                    <select className="input" value={form.role}
                      onChange={(e) => setForm({ ...form, role: e.target.value })}>
                      {["Employee", "Manager", "HR", "Admin"].map((r) => (
                        <option key={r}>{r}</option>
                      ))}
                    </select>
                  </Field>
                  <Field label="Leave balance">
                    <input className="input" type="number" min="0" value={form.leaveBalance}
                      onChange={(e) => setForm({ ...form, leaveBalance: e.target.value })} />
                  </Field>
                </>
              )}
            </div>
            <button type="submit" className="btn btn-primary-solid" disabled={saving}>
              {saving ? "Saving..." : "Save changes"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}

function Detail({ label, value }) {
  return (
    <div style={{ background: "var(--bg)", border: "1px solid var(--border)", borderRadius: 12, padding: 14 }}>
      <div style={{ fontSize: 12, color: "var(--text-soft)", textTransform: "uppercase", letterSpacing: ".04em", fontWeight: 600 }}>
        {label}
      </div>
      <div style={{ fontSize: 16, fontWeight: 600, marginTop: 4 }}>{value || "—"}</div>
    </div>
  );
}

function Field({ label, children }) {
  return (
    <div className="field" style={{ marginBottom: 0 }}>
      <label>{label}</label>
      {children}
    </div>
  );
}
