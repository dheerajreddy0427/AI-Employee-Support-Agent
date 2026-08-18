import { useEffect, useState } from "react";
import { myPayslips } from "../Services/payslipApi";
import EmptyState from "../Components/EmptyState";
import { formatDate } from "../Utils/format";
import { toast } from "../Components/Toast";

export default function MyPayslips() {
  const [payslips, setPayslips] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    myPayslips()
      .then((d) => setPayslips(d || []))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <h1 className="page-title">My payslips</h1>
      <p className="page-subtitle">Download your monthly payslips and view your compensation history.</p>

      <div className="card">
        {loading ? (
          <div style={{ display: "flex", justifyContent: "center", padding: 24 }}>
            <span className="spinner lg" />
          </div>
        ) : payslips.length === 0 ? (
          <EmptyState icon="📄" title="No payslips yet" subtitle="Your payslips will appear here once HR uploads them." />
        ) : (
          <div className="table-wrap" style={{ border: "none" }}>
            <table className="table">
              <thead>
                <tr>
                  <th>Month</th>
                  <th>File</th>
                  <th>Uploaded</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {payslips.map((p) => (
                  <tr key={p.id}>
                    <td style={{ fontWeight: 600 }}>{p.monthYear}</td>
                    <td style={{ color: "var(--text-muted)" }}>{p.fileName}</td>
                    <td style={{ color: "var(--text-soft)" }}>{formatDate(p.uploadedDate)}</td>
                    <td>
                      <a
                        href={p.fileUrl}
                        target="_blank"
                        rel="noreferrer"
                        className="btn btn-soft"
                        onClick={(e) => {
                          if (!p.fileUrl || p.fileUrl.startsWith("https://example.com")) {
                            e.preventDefault();
                            toast.info("Demo payslip — link is a placeholder.");
                          }
                        }}
                      >
                        ⬇ Download
                      </a>
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
