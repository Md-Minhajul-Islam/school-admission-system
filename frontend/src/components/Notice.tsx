import type { ReactNode } from "react";

type Tone = "success" | "error" | "info";

interface NoticeProps {
  tone: Tone;
  title?: string;
  children: ReactNode;
  onDismiss?: () => void;
}

const TONE_STYLES: Record<Tone, string> = {
  success: "border-emerald-200 bg-emerald-50 text-emerald-900",
  error: "border-rose-200 bg-rose-50 text-rose-900",
  info: "border-teal-200 bg-teal-50 text-teal-900",
};

const TONE_ICONS: Record<Tone, string> = {
  success: "✓",
  error: "!",
  info: "i",
};

export function Notice({ tone, title, children, onDismiss }: NoticeProps) {
  return (
    <div
      role={tone === "error" ? "alert" : "status"}
      className={`flex items-start gap-3 rounded-xl border px-4 py-3 text-sm shadow-sm ${TONE_STYLES[tone]}`}
    >
      <span
        aria-hidden="true"
        className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-white/70 text-xs font-bold"
      >
        {TONE_ICONS[tone]}
      </span>
      <div className="min-w-0 flex-1">
        {title ? <p className="font-semibold">{title}</p> : null}
        <div className={title ? "mt-1" : undefined}>{children}</div>
      </div>
      {onDismiss ? (
        <button
          type="button"
          onClick={onDismiss}
          className="rounded-md px-1.5 py-0.5 text-xs font-semibold uppercase tracking-wide opacity-70 hover:opacity-100"
          aria-label="Dismiss message"
        >
          Close
        </button>
      ) : null}
    </div>
  );
}
