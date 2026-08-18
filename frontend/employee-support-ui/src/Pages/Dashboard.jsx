import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import StatCard from "../Components/StatCard";
import StatusBadge from "../Components/StatusBadge";
import EmptyState from "../Components/EmptyState";
import { formatDate, formatMoney } from "../Utils/format";
import { getMyProfile } from "../Services/employeeApi";
import { getStoredUser } from "../Utils/roleHelper";
import { myLeaves } from "../Services/leaveApi";
import { myPayslips } from "../Services/payslipApi";
import { myTickets } from "../Services/ticketApi";
import { myReimbursements } from "../Services/reimbursementApi";

const SUGGESTIONS = [
  "Apply leave from 2026-08-10 to 2026-08-12",
  "How many leaves do I have?",
  "Show my payslip",
  "Raise ticket for laptop not working",
  "Reimburse 250 for client travel"
];

export default function Dashboard() {
  const navigate = useNavigate();
  const cachedUser = getStoredUser();
  const firstName = cachedUser?.fullName?.split(" ")[0];
  const [profile, setProfile] = useState(null);
  const [leaves, setLeaves] = useState([]);
  const [payslips, setPayslips] = useState([]);
  const [tickets, setTickets] = useState([]);
  const [reimbursements, setReimbursements] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [p, l, ps, t, r] = await Promise.allSettled([
          getMyProfile(),
          myLeaves(),
          myPayslips(),
          myTickets(),
          myReimbursements()
        ]);
        if (p.status === "fulfilled") setProfile(p.value);
        if (l.status === "fulfilled") setLeaves(l.value || []);
        if (ps.status === "fulfilled") setPayslips(ps.value || []);
        if (t.status === "fulfilled") setTickets(t.value || []);
        if (r.status === "fulfilled") setReimbursements(r.value || []);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const pendingLeaves = leaves.filter((l) => l.status === "Pending").length;
  const openTickets = tickets.filter((t) => t.status !== "Closed").length;
  const pendingReimbs = reimbursements.filter((r) => r.status === "Pending").length;
  const latestPayslip = payslips[0];

  const goChat = (msg) => {
    sessionStorage.setItem("chat:seed", msg);
    navigate("/chat");
  };

  return (
    <div className="page">
      <div style={{ marginBottom: 24 }}>
        <h1 className="page-title">Welcome back{firstName ? `, ${firstName}` : profile ? `, ${profile.fullName.split(" ")[0]}` : ""} 👋</h1>
        <p className="page-subtitle">
          Your daily HR overview. Use the AI assistant on the right, or jump into a section below.
        </p>
      </div>

      <div className="stat-grid" style={{ marginBottom: 28 }}>
        <StatCard label="Leave balance" value={profile?.leaveBalance ?? "—"} icon="🌴" />
        <StatCard label="Pending requests" value={pendingLeaves + pendingReimbs} icon="⏳" tone="warning" />
        <StatCard label="Open IT tickets" value={openTickets} icon="🎫" tone="danger" />
        <StatCard
          label="Latest payslip"
          value={latestPayslip ? latestPayslip.monthYear : "—"}
          icon="📄"
          tone="success"
        />
      </div>

      <div style={{ display: "grid", gap: 20, gridTemplateColumns: "2fr 1fr" }}>
        <div className="card">
          <div className="card-header">
            <div>
              <div className="card-title">Recent activity</div>
              <div style={{ color: "var(--text-soft)", fontSize: 13 }}>Your latest leave, ticket, and reimbursement updates</div>
            </div>
            <Link to="/my-leaves" className="btn btn-soft" style={{ textDecoration: "none" }}>View all</Link>
          </div>

          {loading ? (
            <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
              <span className="spinner lg" />
            </div>
          ) : (
            <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
              {leaves.slice(0, 3).map((l) => (
                <div key={`l-${l.id}`} style={rowStyle}>
                  <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <div style={iconStyle("var(--primary-soft)", "var(--primary)")}>📝</div>
                    <div>
                      <div style={{ fontWeight: 600 }}>Leave {formatDate(l.startDate)} → {formatDate(l.endDate)}</div>
                      <div style={{ color: "var(--text-soft)", fontSize: 13 }}>{l.reason || "—"}</div>
                    </div>
                  </div>
                  <StatusBadge status={l.status} />
                </div>
              ))}
              {tickets.slice(0, 2).map((t) => (
                <div key={`t-${t.id}`} style={rowStyle}>
                  <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <div style={iconStyle("rgba(245,158,11,.12)", "var(--warning)")}>🎫</div>
                    <div>
                      <div style={{ fontWeight: 600 }}>{t.issueTitle}</div>
                      <div style={{ color: "var(--text-soft)", fontSize: 13 }}>Filed {formatDate(t.createdDate)}</div>
                    </div>
                  </div>
                  <StatusBadge status={t.status} />
                </div>
              ))}
              {reimbursements.slice(0, 2).map((r) => (
                <div key={`r-${r.id}`} style={rowStyle}>
                  <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                    <div style={iconStyle("rgba(16,185,129,.12)", "var(--success)")}>💰</div>
                    <div>
                      <div style={{ fontWeight: 600 }}>{formatMoney(r.amount)} · {r.description}</div>
                      <div style={{ color: "var(--text-soft)", fontSize: 13 }}>Submitted {formatDate(r.submittedDate)}</div>
                    </div>
                  </div>
                  <StatusBadge status={r.status} />
                </div>
              ))}
              {leaves.length === 0 && tickets.length === 0 && reimbursements.length === 0 && (
                <EmptyState icon="🌱" title="No activity yet" subtitle="Your recent activity will appear here." />
              )}
            </div>
          )}
        </div>

        <div className="card">
          <div className="card-header">
            <div className="card-title">Quick actions</div>
          </div>
          <div style={{ display: "grid", gap: 10 }}>
            {SUGGESTIONS.map((s) => (
              <button key={s} className="btn btn-soft" onClick={() => goChat(s)} style={{ justifyContent: "flex-start" }}>
                💬 {s}
              </button>
            ))}
            <Link to="/leaves" className="btn btn-primary-solid" style={{ textDecoration: "none" }}>
              ➕ Apply for leave
            </Link>
            <Link to="/reimbursements" className="btn btn-ghost" style={{ textDecoration: "none" }}>
              💰 Submit reimbursement
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}

const rowStyle = {
  display: "flex",
  justifyContent: "space-between",
  alignItems: "center",
  padding: "12px 14px",
  background: "var(--bg)",
  border: "1px solid var(--border)",
  borderRadius: 12
};

const iconStyle = (bg, color) => ({
  width: 40, height: 40, borderRadius: 10,
  background: bg, color,
  display: "grid", placeItems: "center", fontSize: 18
});
