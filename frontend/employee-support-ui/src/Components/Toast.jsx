import { useEffect, useState, useCallback } from "react";

let listeners = new Set();
let id = 0;

export const toast = {
  success: (msg) => publish("success", msg),
  error: (msg) => publish("error", msg),
  info: (msg) => publish("info", msg),
};

function publish(type, message) {
  const next = { id: ++id, type, message };
  listeners.forEach((l) => l(next));
}

function ToastView() {
  const [items, setItems] = useState([]);

  const add = useCallback((t) => {
    setItems((prev) => [...prev, t]);
    setTimeout(() => setItems((prev) => prev.filter((x) => x.id !== t.id)), 3500);
  }, []);

  useEffect(() => {
    listeners.add(add);
    return () => { listeners.delete(add); };
  }, [add]);

  return (
    <div className="toast-wrap">
      {items.map((t) => (
        <div key={t.id} className={`toast ${t.type}`}>
          <span style={{ fontSize: 18 }}>
            {t.type === "success" ? "✅" : t.type === "error" ? "⚠️" : "ℹ️"}
          </span>
          <span>{t.message}</span>
        </div>
      ))}
    </div>
  );
}

export default ToastView;
