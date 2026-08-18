import { useEffect, useState } from "react";
import { pendingLeaves, approveLeave, rejectLeave } from "../Services/leaveApi";
import {
  pendingReimbursements,
  approveReimbursement,
  rejectReimbursement
} from "../Services/reimbursementApi";
import { getMyProfile } from "../Services/employeeApi";
import { getEmployeeId } from "../Utils/jwtHelper";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDate, formatMoney } from "../Utils/format";
import { toast } from "../Components/Toast";

const TABS = [
  { key: "leave", label: "Leave" },
  { key: "reimbursement", label: "Reimbursements" }
];

export default function AdminApprovals() {
  const [tab, setTab] = useState("leave");
  const [leaves, setLeaves] = useState([]);
  const [reimbs, setReimbs] = useState([]);
  const [employees, setEmployees] = useState({});
  const [loading, setLoading] = useState(true);

  const load = async () => {
    setLoading(true);
    try {
      // Try the relevant endpoint based on the active tab; HR (and Admin) can
      // pull both, Manager only the leave one.
      const [p1, p2, p3] = await Promise.allSettled([
        pendingLeaves(),
        pendingReimbursements(),
        getMyProfile()
      ]);
      setLeaves(p1.status === "fulfilled" ? p1.value || [] : []);
      setReimbs(p2.status === "fulfilled" ? p2.value || [] : []);
      if (p3.status === "fulfilled" && p3.value) {
        const map = { [p3.value.employeeId]: p3.value };
        setEmployees(map);
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleApproveLeave = async (leave) => {
    try {
      await approveLeave({
        leaveId: leave.id,
        managerId: getEmployeeId(),
        remarks: "Approved"
      });
      toast.success(`Leave #${leave.id} approved.`);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to approve leave.");
    }
  };

  const handleRejectLeave = async (leave) => {
    try {
      await rejectLeave({
        leaveId: leave.id,
        managerId: getEmployeeId(),
        remarks: "Rejected"
      });
      toast.success(`Leave #${leave.id} rejected.`);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to reject leave.");
    }
  };

  const handleApproveReimb = async (r) => {
    try {
      await approveReimbursement(r.id);
      toast.success(`Reimbursement #${r.id} approved.`);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to approve reimbursement.");
    }
  };

  const handleRejectReimb = async (r) => {
    try {
      await rejectReimbursement(r.id);
      toast.success(`Reimbursement #${r.id} rejected.`);
      await load();
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.error || "Failed to reject reimbursement.");
    }
  };

  const empName = (id) => employees[id]?.fullName || `Employee #${id}`;

  return (
    <div className="page">
      <h1 className="page-title">Approvals</h1>
      <p className="page-subtitle">Review and act on pending requests from your team.</p>

      <div style={{ display: "flex", gap: 8, marginBottom: 16 }}>
        {TABS.map((t) => (
          <button
            key={t.key}
            className={`btn ${tab === t.key ? "btn-primary-solid" : "btn-ghost"}`}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {loading ? (
        <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
          <span className="spinner lg" />
        </div>
      ) : tab === "leave" ? (
        <div className="card">
          <div className="card-header">
            <div className="card-title">Pending leave requests</div>
            <span className="badge primary">{leaves.length}</span>
          </div>
          {leaves.length === 0 ? (
            <EmptyState icon="✅" title="No pending leaves" subtitle="All leave requests have been actioned." />
          ) : (
            <div className="table-wrap" style={{ border: "none" }}>
              <table className="table">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Employee</th>
                    <th>Period</th>
                    <th>Reason</th>
                    <th>Status</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {leaves.map((l) => (
                    <tr key={l.id}>
                      <td style={{ color: "var(--text-soft)" }}>#{l.id}</td>
                      <td>{empName(l.employeeId)}</td>
                      <td>{formatDate(l.startDate)} → {formatDate(l.endDate)}</td>
                      <td style={{ color: "var(--text-muted)" }}>{l.reason || "—"}</td>
                      <td><StatusBadge status={l.status} /></td>
                      <td>
                        <div style={{ display: "flex", gap: 6 }}>
                          <button className="btn btn-soft" onClick={() => handleApproveLeave(l)}>✅</button>
                          <button className="btn btn-ghost" onClick={() => handleRejectLeave(l)}>✋</button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      ) : (
        <div className="card">
          <div className="card-header">
            <div className="card-title">Pending reimbursements</div>
            <span className="badge primary">{reimbs.length}</span>
          </div>
          {reimbs.length === 0 ? (
            <EmptyState icon="💰" title="No pending reimbursements" subtitle="All claims have been actioned." />
          ) : (
            <div className="table-wrap" style={{ border: "none" }}>
              <table className="table">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Employee</th>
                    <th>Amount</th>
                    <th>Description</th>
                    <th>Status</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {reimbs.map((r) => (
                    <tr key={r.id}>
                      <td style={{ color: "var(--text-soft)" }}>#{r.id}</td>
                      <td>{empName(r.employeeId)}</td>
                      <td style={{ fontWeight: 600 }}>{formatMoney(r.amount)}</td>
                      <td style={{ color: "var(--text-muted)" }}>{r.description}</td>
                      <td><StatusBadge status={r.status} /></td>
                      <td>
                        <div style={{ display: "flex", gap: 6 }}>
                          <button className="btn btn-soft" onClick={() => handleApproveReimb(r)}>✅</button>
                          <button className="btn btn-ghost" onClick={() => handleRejectReimb(r)}>✋</button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
