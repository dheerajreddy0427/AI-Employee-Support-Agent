import { useEffect, useState } from "react";
import { myLeaves } from "../Services/leaveApi";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDate } from "../Utils/format";

export default function MyLeaves() {
  const [leaves, setLeaves] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    myLeaves()
      .then((d) => setLeaves(d || []))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <h1 className="page-title">My leaves</h1>
      <p className="page-subtitle">All your leave requests, current status, and approvals.</p>

      <div className="card">
        {loading ? (
          <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
            <span className="spinner lg" />
          </div>
        ) : leaves.length === 0 ? (
          <EmptyState icon="🌴" title="No leave requests" subtitle="Apply for leave from the Apply Leave page." />
        ) : (
          <div className="table-wrap" style={{ border: "none" }}>
            <table className="table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>From</th>
                  <th>To</th>
                  <th>Reason</th>
                  <th>Applied on</th>
                  <th>Status</th>
                  <th>Remarks</th>
                </tr>
              </thead>
              <tbody>
                {leaves.map((l) => (
                  <tr key={l.id}>
                    <td style={{ color: "var(--text-soft)" }}>#{l.id}</td>
                    <td>{formatDate(l.startDate)}</td>
                    <td>{formatDate(l.endDate)}</td>
                    <td style={{ color: "var(--text-muted)" }}>{l.reason || "—"}</td>
                    <td style={{ color: "var(--text-soft)" }}>{formatDate(l.createdAt)}</td>
                    <td><StatusBadge status={l.status} /></td>
                    <td style={{ color: "var(--text-soft)" }}>{l.remarks || "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
