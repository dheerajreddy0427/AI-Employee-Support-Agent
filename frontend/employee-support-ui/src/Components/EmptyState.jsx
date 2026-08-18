export default function EmptyState({ icon = "📭", title = "Nothing here yet", subtitle }) {
  return (
    <div className="empty">
      <div className="big">{icon}</div>
      <div style={{ fontWeight: 600, color: "var(--text)" }}>{title}</div>
      {subtitle && <div>{subtitle}</div>}
    </div>
  );
}
