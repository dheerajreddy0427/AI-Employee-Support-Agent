import { useEffect, useState } from "react";
import { applyLeave, myLeaves } from "../Services/leaveApi";
import { getEmployeeId } from "../Utils/jwtHelper";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDate } from "../Utils/format";
import { toast } from "../Components/Toast";

export default function ApplyLeave() {
  const [form, setForm] = useState({ startDate: "", endDate: "", reason: "" });
  const [loading, setLoading] = useState(false);
  const [leaves, setLeaves] = useState([]);
  const [loadingList, setLoadingList] = useState(true);

  const load = async () => {
    setLoadingList(true);
    try {
      const data = await myLeaves();
      setLeaves(data || []);
    } finally {
      setLoadingList(false);
    }
  };

  useEffect(() => { load(); }, []);

  const submit = async (e) => {
    e.preventDefault();
    if (!form.startDate || !form.endDate || !form.reason.trim()) {
      toast.error("Please fill in all fields.");
      return;
    }
    if (form.endDate < form.startDate) {
      toast.error("End date must be on or after start date.");
      return;
    }
    setLoading(true);
    try {
      await applyLeave({
        employeeId: getEmployeeId(),
        startDate: form.startDate,
        endDate: form.endDate,
        reason: form.reason
      });
      toast.success("Leave request submitted.");
      setForm({ startDate: "", endDate: "", reason: "" });
      await load();
    } catch (err) {
      const msg = err?.response?.data?.error || "Failed to submit leave request.";
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <h1 className="page-title">Apply for leave</h1>
      <p className="page-subtitle">Submit a request and your manager will be notified for approval.</p>

      <div style={{ display: "grid", gap: 20, gridTemplateColumns: "1fr 1fr" }}>
        <div className="card">
          <div className="card-header">
            <div className="card-title">New leave request</div>
          </div>
          <form onSubmit={submit} className="form">
            <div className="form-row">
              <div className="field">
                <label>Start date</label>
                <input
                  type="date"
                  className="input"
                  value={form.startDate}
                  onChange={(e) => setForm({ ...form, startDate: e.target.value })}
                />
              </div>
              <div className="field">
                <label>End date</label>
                <input
                  type="date"
                  className="input"
                  value={form.endDate}
                  onChange={(e) => setForm({ ...form, endDate: e.target.value })}
                />
              </div>
            </div>
            <div className="field">
              <label>Reason</label>
              <textarea
                className="textarea"
                placeholder="Briefly describe the reason for your leave..."
                value={form.reason}
                onChange={(e) => setForm({ ...form, reason: e.target.value })}
              />
            </div>
            <button type="submit" className="btn btn-primary-solid" disabled={loading}>
              {loading ? "Submitting..." : "Submit request"}
            </button>
          </form>
        </div>

        <div className="card">
          <div className="card-header">
            <div className="card-title">My recent leave</div>
          </div>
          {loadingList ? (
            <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
              <span className="spinner lg" />
            </div>
          ) : leaves.length === 0 ? (
            <EmptyState icon="🌴" title="No leave requests yet" subtitle="Your history will show up here." />
          ) : (
            <div className="table-wrap" style={{ border: "none" }}>
              <table className="table">
                <thead>
                  <tr>
                    <th>Period</th>
                    <th>Reason</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {leaves.slice(0, 6).map((l) => (
                    <tr key={l.id}>
                      <td>{formatDate(l.startDate)} → {formatDate(l.endDate)}</td>
                      <td style={{ color: "var(--text-muted)" }}>{l.reason || "—"}</td>
                      <td><StatusBadge status={l.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
