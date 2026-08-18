import { useEffect, useState } from "react";
import { myReimbursements, createReimbursement } from "../Services/reimbursementApi";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDate, formatMoney } from "../Utils/format";
import { toast } from "../Components/Toast";

export default function MyReimbursements() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ amount: "", description: "" });
  const [submitting, setSubmitting] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const d = await myReimbursements();
      setItems(d || []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const submit = async (e) => {
    e.preventDefault();
    const amount = Number(form.amount);
    if (!amount || amount <= 0) { toast.error("Please enter a valid amount."); return; }
    if (!form.description.trim()) { toast.error("Please add a short description."); return; }
    setSubmitting(true);
    try {
      await createReimbursement({ amount, description: form.description });
      toast.success("Reimbursement submitted.");
      setForm({ amount: "", description: "" });
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.error || "Failed to submit reimbursement.");
    } finally {
      setSubmitting(false);
    }
  };

  const totalApproved = items
    .filter((r) => r.status === "Approved")
    .reduce((s, r) => s + Number(r.amount), 0);

  return (
    <div className="page">
      <h1 className="page-title">Reimbursements</h1>
      <p className="page-subtitle">Submit expense claims and track approvals.</p>

      <div className="stat-grid" style={{ marginBottom: 24, gridTemplateColumns: "repeat(3, 1fr)" }}>
        <div className="stat"><div className="icon">📦</div><div><div className="label">Total claims</div><div className="value">{items.length}</div></div></div>
        <div className="stat success"><div className="icon">✅</div><div><div className="label">Approved total</div><div className="value">{formatMoney(totalApproved)}</div></div></div>
        <div className="stat warning"><div className="icon">⏳</div><div><div className="label">Pending</div><div className="value">{items.filter((r) => r.status === "Pending").length}</div></div></div>
      </div>

      <div style={{ display: "grid", gap: 20, gridTemplateColumns: "1fr 2fr" }}>
        <div className="card">
          <div className="card-header">
            <div className="card-title">New claim</div>
          </div>
          <form onSubmit={submit} className="form">
            <div className="field">
              <label>Amount (₹)</label>
              <input
                type="number"
                step="0.01"
                min="0"
                className="input"
                value={form.amount}
                onChange={(e) => setForm({ ...form, amount: e.target.value })}
                placeholder="0.00"
              />
            </div>
            <div className="field">
              <label>Description</label>
              <textarea
                className="textarea"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="What is this claim for?"
              />
            </div>
            <button type="submit" className="btn btn-primary-solid" disabled={submitting}>
              {submitting ? "Submitting..." : "Submit claim"}
            </button>
          </form>
        </div>

        <div className="card">
          <div className="card-header">
            <div className="card-title">My claims</div>
            <span className="badge primary">{items.length}</span>
          </div>
          {loading ? (
            <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
              <span className="spinner lg" />
            </div>
          ) : items.length === 0 ? (
            <EmptyState icon="💰" title="No claims yet" subtitle="Submit your first reimbursement claim." />
          ) : (
            <div className="table-wrap" style={{ border: "none" }}>
              <table className="table">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Amount</th>
                    <th>Description</th>
                    <th>Submitted</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((r) => (
                    <tr key={r.id}>
                      <td style={{ color: "var(--text-soft)" }}>#{r.id}</td>
                      <td style={{ fontWeight: 600 }}>{formatMoney(r.amount)}</td>
                      <td style={{ color: "var(--text-muted)" }}>{r.description}</td>
                      <td style={{ color: "var(--text-soft)" }}>{formatDate(r.submittedDate)}</td>
                      <td><StatusBadge status={r.status} /></td>
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
