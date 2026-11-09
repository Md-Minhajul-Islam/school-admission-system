import type { HoldStatus } from "../api/types";
import { statusLabel } from "../lib/format";

const STYLES: Record<HoldStatus, string> = {
  HELD: "border-amber-200 bg-amber-100 text-amber-900",
  CONFIRMED: "border-emerald-200 bg-emerald-100 text-emerald-900",
  EXPIRED: "border-rose-200 bg-rose-100 text-rose-900",
  CANCELLED: "border-slate-200 bg-slate-100 text-slate-700",
};

export function StatusBadge({ status }: { status: HoldStatus }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-semibold ${STYLES[status]}`}
    >
      <span aria-hidden="true" className="h-1.5 w-1.5 rounded-full bg-current" />
      {statusLabel(status)}
    </span>
  );
}
