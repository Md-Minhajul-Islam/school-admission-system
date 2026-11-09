import type { MyHold } from "../api/types";
import { errorMessage } from "../api/client";
import {
  effectiveStatus,
  formatCompactDateTime,
  formatCountdown,
  secondsRemaining,
} from "../lib/format";
import { Notice } from "./Notice";
import { StatusBadge } from "./StatusBadge";

interface MyApplicationsSectionProps {
  studentIdValid: boolean;
  holds: MyHold[];
  isPending: boolean;
  error: unknown;
  onRetry: () => void;
  selectedHoldId: string | null;
  onSelect: (holdId: string) => void;
  now: number;
}

export function MyApplicationsSection({
  studentIdValid,
  holds,
  isPending,
  error,
  onRetry,
  selectedHoldId,
  onSelect,
  now,
}: MyApplicationsSectionProps) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-bold text-slate-900">My applications</h2>
          <p className="mt-1 text-sm text-slate-600">
            Everything linked to your student ID, saved across page reloads.
          </p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
          <span className="live-dot h-1.5 w-1.5 rounded-full bg-emerald-500" />
          Live · every 2s
        </span>
      </div>

      {!studentIdValid ? (
        <div className="mt-5 rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-8 text-center">
          <p className="font-semibold text-slate-800">No student ID yet</p>
          <p className="mt-1 text-sm text-slate-500">
            Enter your 17-digit birth registration number at the top of the page to see
            your applications.
          </p>
        </div>
      ) : null}

      {studentIdValid && error ? (
        <div className="mt-5">
          <Notice tone="error" title="We could not load your applications">
            <p>{errorMessage(error)}</p>
            <button
              type="button"
              onClick={onRetry}
              className="mt-3 rounded-lg border border-rose-300 bg-white px-3 py-1.5 text-xs font-semibold text-rose-700 hover:bg-rose-50"
            >
              Try again
            </button>
          </Notice>
        </div>
      ) : null}

      {studentIdValid && isPending && holds.length === 0 ? (
        <div className="mt-5 space-y-2">
          {[0, 1, 2].map((item) => (
            <div key={item} className="h-12 animate-pulse rounded-lg bg-slate-100" />
          ))}
        </div>
      ) : null}

      {studentIdValid && !isPending && !error && holds.length === 0 ? (
        <div className="mt-5 rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-8 text-center">
          <p className="font-semibold text-slate-800">No applications yet</p>
          <p className="mt-1 text-sm text-slate-500">
            Apply for a class above and it will appear here immediately.
          </p>
        </div>
      ) : null}

      {holds.length > 0 ? (
        <ul className="mt-4 divide-y divide-slate-100">
          {holds.map((hold) => {
            const status = effectiveStatus(hold.status, hold.expiresAt, now);
            const remaining = secondsRemaining(hold.expiresAt, now);
            const selected = selectedHoldId === hold.holdId;

            return (
              <li
                key={hold.holdId}
                className={`flex flex-wrap items-start justify-between gap-3 rounded-lg px-2 py-3 transition ${
                  selected ? "bg-emerald-50/80" : ""
                }`}
              >
                <div className="min-w-0 flex-1 basis-48">
                  <p className="break-words font-medium leading-snug text-slate-900">
                    {hold.className}
                  </p>
                  <div className="mt-1.5 flex flex-wrap items-center gap-x-3 gap-y-1">
                    <StatusBadge status={status} />
                    <span className="text-xs text-slate-600">
                      {status === "HELD" ? (
                        <span className="font-mono font-semibold tabular-nums">
                          {formatCountdown(remaining)} left
                        </span>
                      ) : status === "CONFIRMED" ? (
                        <span className="font-medium text-emerald-700">Paid</span>
                      ) : (
                        <span className="text-slate-500">
                          {formatCompactDateTime(hold.expiresAt)}
                        </span>
                      )}
                    </span>
                  </div>
                  <p className="mt-1 break-all font-mono text-[11px] text-slate-400">
                    {hold.holdId}
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => onSelect(hold.holdId)}
                  aria-pressed={selected}
                  className={`shrink-0 rounded-lg border px-3 py-1.5 text-xs font-semibold transition ${
                    selected
                      ? "border-emerald-300 bg-white text-emerald-700"
                      : "border-slate-300 bg-white text-slate-700 hover:bg-emerald-50 hover:text-emerald-700"
                  }`}
                >
                  View
                </button>
              </li>
            );
          })}
        </ul>
      ) : null}
    </section>
  );
}
