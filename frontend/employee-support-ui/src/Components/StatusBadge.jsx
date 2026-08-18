import { statusBadge } from "../Utils/format";

export default function StatusBadge({ status }) {
  return <span className={`badge ${statusBadge(status)}`}>{status || "Unknown"}</span>;
}
