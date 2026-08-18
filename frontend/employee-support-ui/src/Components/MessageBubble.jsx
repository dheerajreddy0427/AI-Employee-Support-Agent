export default function MessageBubble({ sender, text, time, meta }) {
  const isUser = sender === "You" || sender === "user";
  return (
    <div className={`bubble-row ${isUser ? "user" : "agent"}`}>
      {!isUser && <div className="avatar" style={{ width: 32, height: 32, fontSize: 11 }}>AI</div>}
      <div>
        <div className="bubble">{text}</div>
        {meta?.payslipUrl && (
          <div className="meta-card">
            📄 <a href={meta.payslipUrl} target="_blank" rel="noreferrer">Open latest payslip</a>
          </div>
        )}
        {meta?.leave && (
          <div className="meta-card">
            ✅ Leave {meta.leave.startDate?.slice(0, 10)} → {meta.leave.endDate?.slice(0, 10)} ({meta.leave.days} day(s))
          </div>
        )}
        {time && <div className="time">{time}</div>}
      </div>
    </div>
  );
}
