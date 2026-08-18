export default function StatCard({ label, value, icon, tone = "default" }) {
  return (
    <div className={`stat ${tone !== "default" ? tone : ""}`}>
      <div className="icon">{icon}</div>
      <div>
        <div className="label">{label}</div>
        <div className="value">{value}</div>
      </div>
    </div>
  );
}
