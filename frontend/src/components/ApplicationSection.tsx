import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, type ReactNode } from "react";
import { api, errorMessage } from "../api/client";
import type { HoldDetail } from "../api/types";
import {
  effectiveStatus,
  formatCountdown,
  formatDateTime,
  secondsRemaining,
  statusLabel,
} from "../lib/format";
import { Notice } from "./Notice";
import { StatusBadge } from "./StatusBadge";

interface ApplicationSectionProps {
  holdId: string | null;
  hold: HoldDetail | undefined;
  className: string;
  studentId: string;
  studentIdValid: boolean;
  isPending: boolean;
  error: unknown;
  onRetry: () => void;
  now: number;
}

interface Feedback {
  holdId: string;
  tone: "success" | "info";
  text: string;
}

export function ApplicationSection({
  holdId,
  hold,
  className,
  studentId,
  studentIdValid,
  isPending,
  error,
  onRetry,
  now,
}: ApplicationSectionProps) {
  const queryClient = useQueryClient();
  const [feedback, setFeedback] = useState<Feedback | null>(null);

  function refresh(targetHoldId: string) {
    void queryClient.invalidateQueries({ queryKey: ["hold", targetHoldId] });
    void queryClient.invalidateQueries({ queryKey: ["my-holds"] });
    void queryClient.invalidateQueries({ queryKey: ["classes"] });
  }

  const confirm = useMutation({
    mutationFn: (targetHoldId: string) => api.confirmHold(targetHoldId, studentId),
    onSuccess: (_result, targetHoldId) => {
      setFeedback({
        holdId: targetHoldId,
        tone: "success",
        text: "Payment received. Your admission is confirmed.",
      });
      refresh(targetHoldId);
    },
  });

  const cancel = useMutation({
    mutationFn: (targetHoldId: string) => api.cancelHold(targetHoldId, studentId),
    onSuccess: (_result, targetHoldId) => {
      setFeedback({
        holdId: targetHoldId,
        tone: "info",
        text: "Application cancelled. The seat is available to other students again.",
      });
      refresh(targetHoldId);
    },
  });

  const actionPending =
    (confirm.isPending && confirm.variables === holdId) ||
    (cancel.isPending && cancel.variables === holdId);

  const actionError =
    confirm.isError && confirm.variables === holdId
      ? confirm.error
      : cancel.isError && cancel.variables === holdId
        ? cancel.error
        : null;

  const visibleFeedback = feedback?.holdId === holdId ? feedback : null;

  if (!studentIdValid) {
    return (
      <Panel>
        <EmptyState
          title="Enter your student ID to begin"
          body="Use the 17-digit birth registration number field at the top of the page."
        />
      </Panel>
    );
  }

  if (!holdId) {
    return (
      <Panel>
        <EmptyState
          title="No application selected"
          body="Apply for a class, or choose one of your applications below to see its status and pay."
        />
      </Panel>
    );
  }

  if (isPending && !hold) {
    return (
      <Panel>
        <div className="h-40 animate-pulse rounded-xl bg-slate-100" />
      </Panel>
    );
  }

  if (error && !hold) {
    return (
      <Panel>
        <Notice tone="error" title="We could not load this application">
          <p>{errorMessage(error)}</p>
          <button
            type="button"
            onClick={onRetry}
            className="mt-3 rounded-lg border border-rose-300 bg-white px-3 py-1.5 text-xs font-semibold text-rose-700 hover:bg-rose-50"
          >
            Try again
          </button>
        </Notice>
      </Panel>
    );
  }

  if (!hold) {
    return (
      <Panel>
        <EmptyState
          title="Application not found"
          body="It may have been removed. Refresh the page or choose another application."
        />
      </Panel>
    );
  }

  const status = effectiveStatus(hold.status, hold.expiresAt, now);
  const remaining = secondsRemaining(hold.expiresAt, now);
  const isHeld = status === "HELD";
  const urgent = remaining <= 60;

  return (
    <Panel>
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="text-[11px] font-semibold uppercase tracking-wide text-slate-500">
            Current application
          </p>
          <h2 className="mt-1 truncate text-lg font-bold text-slate-900">
            {className || "Selected class"}
          </h2>
        </div>
        <StatusBadge status={status} />
      </div>

      <div
        className={`mt-5 rounded-xl border p-5 ${
          isHeld
            ? urgent
              ? "border-rose-200 bg-rose-50"
              : "border-amber-200 bg-amber-50"
            : status === "CONFIRMED"
              ? "border-emerald-200 bg-emerald-50"
              : "border-slate-200 bg-slate-50"
        }`}
      >
        {isHeld ? (
          <>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">
              Time left to pay
            </p>
            <p
              aria-label={`${remaining} seconds remaining`}
              className={`mt-1 font-mono text-4xl font-bold tabular-nums ${
                urgent ? "text-rose-700" : "text-amber-800"
              }`}
            >
              {formatCountdown(remaining)}
            </p>
            <p className="mt-2 text-sm text-slate-600">
              {urgent
                ? "Hurry — the seat will be released when the timer reaches zero."
                : `Pay before ${formatDateTime(hold.expiresAt)} to keep the seat.`}
            </p>
          </>
        ) : (
          <>
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-600">
              Status
            </p>
            <p className="mt-1 text-2xl font-bold text-slate-900">
              {statusLabel(status)}
            </p>
            <p className="mt-2 text-sm text-slate-600">
              {status === "CONFIRMED"
                ? "Payment was recorded and the seat is confirmed. No further action is needed."
                : status === "EXPIRED"
                  ? `The hold expired at ${formatDateTime(hold.expiresAt)} and the seat was released. You can apply again.`
                  : "You cancelled this application. You can apply for a class again."}
            </p>
          </>
        )}
      </div>

      <dl className="mt-4 grid grid-cols-2 gap-3 text-xs">
        <div className="rounded-lg bg-slate-50 px-3 py-2">
          <dt className="font-semibold uppercase tracking-wide text-slate-500">
            Held until
          </dt>
          <dd className="mt-0.5 font-medium text-slate-800">
            {formatDateTime(hold.expiresAt)}
          </dd>
        </div>
        <div className="rounded-lg bg-slate-50 px-3 py-2">
          <dt className="font-semibold uppercase tracking-wide text-slate-500">
            Application ID
          </dt>
          <dd className="mt-0.5 truncate font-mono text-slate-800">{hold.holdId}</dd>
        </div>
      </dl>

      {actionError ? (
        <div className="mt-4">
          <Notice tone="error" title="The action could not be completed">
            {errorMessage(actionError)}
          </Notice>
        </div>
      ) : null}

      {visibleFeedback ? (
        <div className="mt-4">
          <Notice
            tone={visibleFeedback.tone === "success" ? "success" : "info"}
            onDismiss={() => setFeedback(null)}
          >
            {visibleFeedback.text}
          </Notice>
        </div>
      ) : null}

      <div className="mt-5 flex flex-col gap-2 sm:flex-row">
        <button
          type="button"
          disabled={!isHeld || actionPending}
          onClick={() => {
            setFeedback(null);
            confirm.reset();
            confirm.mutate(hold.holdId);
          }}
          className="flex-1 rounded-xl bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
        >
          {confirm.isPending && confirm.variables === hold.holdId
            ? "Confirming payment…"
            : isHeld
              ? "Pay admission fee"
              : "Payment unavailable"}
        </button>

        <button
          type="button"
          disabled={!isHeld || actionPending}
          onClick={() => {
            setFeedback(null);
            cancel.reset();
            cancel.mutate(hold.holdId);
          }}
          className="rounded-xl border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {cancel.isPending && cancel.variables === hold.holdId
            ? "Cancelling…"
            : "Cancel application"}
        </button>
      </div>

      {isHeld ? (
        <p className="mt-3 text-xs text-slate-500">
          Payment is simulated for this assignment and always succeeds while the hold is
          active.
        </p>
      ) : null}
    </Panel>
  );
}

function Panel({ children }: { children: ReactNode }) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      {children}
    </section>
  );
}

function EmptyState({ title, body }: { title: string; body: string }) {
  return (
    <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
      <p className="font-semibold text-slate-800">{title}</p>
      <p className="mt-1 text-sm text-slate-500">{body}</p>
    </div>
  );
}
