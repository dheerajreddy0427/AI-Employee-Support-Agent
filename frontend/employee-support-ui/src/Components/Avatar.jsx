import { initials } from "../Utils/format";

export default function Avatar({ name, size = "md" }) {
  const cls = size === "lg" ? "avatar lg" : "avatar";
  return <div className={cls}>{initials(name)}</div>;
}
