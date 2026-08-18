import { useEffect, useRef, useState } from "react";
import { sendMessage, getHistory, clearHistory } from "../Services/agentApi";
import MessageBubble from "../Components/MessageBubble";
import { toast } from "../Components/Toast";

const SUGGESTIONS = [
  "How many leaves do I have?",
  "Apply leave from 2026-08-10 to 2026-08-12",
  "Show my payslip",
  "Raise ticket for laptop not working",
  "Reimburse 250 for client travel",
  "My profile"
];

export default function ChatWindow() {
  const [messages, setMessages] = useState([
    {
      sender: "Agent",
      text: "Hi! I'm your HR assistant. Ask me about leaves, payslips, IT tickets, or reimbursements. Type \"help\" to see everything I can do.",
      time: now(),
      meta: null
    }
  ]);
  const [input, setInput] = useState("");
  const [typing, setTyping] = useState(false);
  const bodyRef = useRef(null);

  useEffect(() => {
    const seed = sessionStorage.getItem("chat:seed");
    if (seed) { setInput(seed); sessionStorage.removeItem("chat:seed"); }
  }, []);

  useEffect(() => {
    (async () => {
      try {
        const h = await getHistory();
        if (Array.isArray(h) && h.length > 0) {
          const formatted = h.map((m) => ({
            sender: m.sender === "User" ? "You" : "Agent",
            text: m.messageText,
            time: new Date(m.createdAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }),
            meta: null
          }));
          setMessages(formatted);
        }
      } catch (e) { /* ignore */ }
    })();
  }, []);

  useEffect(() => {
    bodyRef.current?.scrollTo({ top: bodyRef.current.scrollHeight, behavior: "smooth" });
  }, [messages, typing]);

  const send = async (textOverride) => {
    const text = (textOverride ?? input).trim();
    if (!text || typing) return;

    setInput("");
    const userMsg = { sender: "You", text, time: now(), meta: null };
    setMessages((m) => [...m, userMsg]);
    setTyping(true);
    try {
      const res = await sendMessage(text);
      setMessages((m) => [
        ...m,
        {
          sender: "Agent",
          text: res.reply,
          time: now(),
          meta: res.meta || null
        }
      ]);
    } catch (e) {
      toast.error("Couldn't reach the agent. Try again.");
      setMessages((m) => [
        ...m,
        { sender: "Agent", text: "I'm having trouble reaching the server right now. Please try again.", time: now(), meta: null }
      ]);
    } finally {
      setTyping(false);
    }
  };

  const onKey = (e) => {
    if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); send(); }
  };

  const onClear = async () => {
    if (!confirm("Clear chat history?")) return;
    try { await clearHistory(); } catch {}
    setMessages([{ sender: "Agent", text: "Chat history cleared. What can I help you with?", time: now(), meta: null }]);
  };

  const hasUserMessages = messages.some((m) => m.sender === "You");

  return (
    <div className="chat-shell">
      <div className="chat-header">
        <div>
          <h2>HR AI Assistant</h2>
          <div className="sub">Ask in plain English — I'll route it to the right place.</div>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="btn btn-ghost" onClick={onClear}>Clear chat</button>
        </div>
      </div>

      <div className="chat-body" ref={bodyRef}>
        {!hasUserMessages && (
          <div className="chat-empty">
            <div style={{ fontSize: 56 }}>💬</div>
            <h3 style={{ color: "var(--text)" }}>How can I help you today?</h3>
            <p>Try one of these:</p>
            <div className="chips">
              {SUGGESTIONS.map((s) => (
                <button key={s} className="chip" onClick={() => send(s)}>{s}</button>
              ))}
            </div>
          </div>
        )}

        {hasUserMessages && messages.map((m, i) => (
          <MessageBubble key={i} sender={m.sender} text={m.text} time={m.time} meta={m.meta} />
        ))}

        {typing && (
          <div className="bubble-row agent">
            <div className="avatar" style={{ width: 32, height: 32, fontSize: 11 }}>AI</div>
            <div className="bubble"><div className="typing"><span></span><span></span><span></span></div></div>
          </div>
        )}
      </div>

      <div className="composer">
        <div className="composer-inner">
          <textarea
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={onKey}
            placeholder="Ask me anything — e.g. apply leave from 2026-08-10 to 2026-08-12"
            rows={1}
          />
          <button
            className="send-btn"
            disabled={!input.trim() || typing}
            onClick={() => send()}
            title="Send"
          >
            ➤
          </button>
        </div>
        <div style={{ marginTop: 8, fontSize: 11, color: "var(--text-soft)" }}>
          Press Enter to send · Shift+Enter for new line
        </div>
      </div>
    </div>
  );
}

function now() {
  return new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
}
