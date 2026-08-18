import { useEffect, useMemo, useState } from "react";
import { myTickets, createTicket } from "../Services/ticketApi";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDateTime, statusBadge } from "../Utils/format";
import { toast } from "../Components/Toast";

const FILTERS = ["All", "Open", "InProgress", "Resolved", "Closed"];

export default function MyTickets() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ issueTitle: "", description: "" });
  const [submitting, setSubmitting] = useState(false);
  const [filter, setFilter] = useState("All");

  const load = async () => {
    setLoading(true);
    try {
      const d = await myTickets();
      setTickets(d || []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const counts = useMemo(() => {
    const out = { All: tickets.length };
    for (const f of FILTERS) {
      if (f === "All") continue;
      out[f] = tickets.filter((t) => t.status === f).length;
    }
    return out;
  }, [tickets]);

  const visible = useMemo(
    () => filter === "All" ? tickets : tickets.filter((t) => t.status === filter),
    [tickets, filter]
  );

  const submit = async (e) => {
    e.preventDefault();
    if (!form.issueTitle.trim() || !form.description.trim()) {
      toast.error("Please add a title and description.");
      return;
    }
    setSubmitting(true);
    try {
      await createTicket({ issueTitle: form.issueTitle, description: form.description });
      toast.success("IT ticket created.");
      setForm({ issueTitle: "", description: "" });
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.error || "Failed to create ticket.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="page">
      <h1 className="page-title">IT tickets</h1>
      <p className="page-subtitle">Report a tech issue and track its status here.</p>

      <div style={{ display: "grid", gap: 20, gridTemplateColumns: "1fr 2fr" }}>
        <div className="card">
          <div className="card-header">
            <div className="card-title">New ticket</div>
          </div>
          <form onSubmit={submit} className="form">
            <div className="field">
              <label>Title</label>
              <input
                className="input"
                value={form.issueTitle}
                onChange={(e) => setForm({ ...form, issueTitle: e.target.value })}
                placeholder="e.g. VPN keeps dropping"
              />
            </div>
            <div className="field">
              <label>Description</label>
              <textarea
                className="textarea"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="Describe the problem in detail..."
              />
            </div>
            <button type="submit" className="btn btn-primary-solid" disabled={submitting}>
              {submitting ? "Creating..." : "Create ticket"}
            </button>
          </form>
        </div>

        <div className="card">
          <div className="card-header">
            <div className="card-title">My tickets</div>
            <span className="badge primary">{visible.length}</span>
          </div>

          <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 12 }}>
            {FILTERS.map((f) => {
              const active = f === filter;
              const tone = f === "All" ? "primary" : statusBadge(f);
              const cls = tone === "warning" ? "btn-warning"
                : tone === "danger"  ? "btn-danger"
                : tone === "success" ? "btn-success"
                : tone === "info"    ? "btn-info"
                : "btn-soft";
              return (
                <button
                  key={f}
                  className={`btn btn-${cls}`}
                  onClick={() => setFilter(f)}
                  style={{
                    outline: active ? "2px solid var(--primary)" : "none",
                    opacity: active ? 1 : 0.85
                  }}
                >
                  {f} <span style={{ opacity: 0.7, marginLeft: 4 }}>{counts[f] ?? 0}</span>
                </button>
              );
            })}
          </div>

          {loading ? (
            <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
              <span className="spinner lg" />
            </div>
          ) : visible.length === 0 ? (
            <EmptyState
              icon="🎫"
              title={filter === "All" ? "No tickets" : `No ${filter} tickets`}
              subtitle={filter === "All" ? "Create a ticket to get IT support." : "Try a different filter."}
            />
          ) : (
            <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
              {visible.map((t) => (
                <div key={t.id} style={{
                  border: "1px solid var(--border)",
                  background: "var(--bg)",
                  borderRadius: 12,
                  padding: 16
                }}>
                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 12 }}>
                    <div>
                      <div style={{ fontWeight: 600 }}>#{t.id} · {t.issueTitle}</div>
                      <div style={{ color: "var(--text-muted)", fontSize: 14, marginTop: 4 }}>{t.description}</div>
                      <div style={{ color: "var(--text-soft)", fontSize: 12, marginTop: 8 }}>Created {formatDateTime(t.createdDate)}</div>
                    </div>
                    <StatusBadge status={t.status} />
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
