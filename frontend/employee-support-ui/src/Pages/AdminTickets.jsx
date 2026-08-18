import { useEffect, useState } from "react";
import { allTickets, updateTicketStatus } from "../Services/ticketApi";
import { getAllEmployees } from "../Services/employeeApi";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDateTime } from "../Utils/format";
import { toast } from "../Components/Toast";

const STATUSES = ["Open", "InProgress", "Resolved", "Closed"];

export default function AdminTickets() {
  const [tickets, setTickets] = useState([]);
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      const [t, e] = await Promise.all([allTickets(), getAllEmployees().catch(() => [])]);
      setTickets(t || []);
      setEmployees(e || []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleStatus = async (ticket, status) => {
    try {
      await updateTicketStatus(ticket.id, {
        status,
        assignedToId: ticket.assignedToId || null
      });
      toast.success(`Ticket #${ticket.id} → ${status}`);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to update.");
    }
  };

  const empName = (id) => employees.find((e) => e.employeeId === id)?.fullName || `Employee #${id}`;

  return (
    <div className="page">
      <h1 className="page-title">All IT tickets</h1>
      <p className="page-subtitle">Track and progress every ticket across the team.</p>

      <div className="card">
        {loading ? (
          <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
            <span className="spinner lg" />
          </div>
        ) : tickets.length === 0 ? (
          <EmptyState icon="🎫" title="No tickets" />
        ) : (
          <div className="table-wrap" style={{ border: "none" }}>
            <table className="table">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Employee</th>
                  <th>Title</th>
                  <th>Filed</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {tickets.map((t) => (
                  <tr key={t.id}>
                    <td style={{ color: "var(--text-soft)" }}>#{t.id}</td>
                    <td>{empName(t.employeeId)}</td>
                    <td style={{ fontWeight: 600 }}>{t.issueTitle}</td>
                    <td style={{ color: "var(--text-soft)" }}>{formatDateTime(t.createdDate)}</td>
                    <td><StatusBadge status={t.status} /></td>
                    <td>
                      <select
                        className="input"
                        style={{ maxWidth: 160 }}
                        value={t.status}
                        onChange={(e) => handleStatus(t, e.target.value)}
                      >
                        {STATUSES.map((s) => (
                          <option key={s} value={s}>{s}</option>
                        ))}
                      </select>
                    </td>
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
