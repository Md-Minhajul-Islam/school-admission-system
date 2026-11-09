import type { HoldStatus } from "../api/types";

export function secondsRemaining(expiresAt: string, now = Date.now()): number {
  const target = new Date(expiresAt).getTime();

  if (Number.isNaN(target)) {
    return 0;
  }

  return Math.max(0, Math.ceil((target - now) / 1000));
}

export function formatCountdown(totalSeconds: number): string {
  const safeSeconds = Math.max(0, Math.floor(totalSeconds));
  const hours = Math.floor(safeSeconds / 3600);
  const minutes = Math.floor((safeSeconds % 3600) / 60);
  const seconds = safeSeconds % 60;

  const paddedMinutes = String(minutes).padStart(2, "0");
  const paddedSeconds = String(seconds).padStart(2, "0");

  return hours > 0
    ? `${hours}:${paddedMinutes}:${paddedSeconds}`
    : `${minutes}:${paddedSeconds}`;
}

export function effectiveStatus(
  status: HoldStatus,
  expiresAt: string,
  now = Date.now(),
): HoldStatus {
  return status === "HELD" && secondsRemaining(expiresAt, now) <= 0
    ? "EXPIRED"
    : status;
}

export function formatDateTime(value: string): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "Unknown time";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "medium",
  }).format(date);
}

/** Compact timestamp for table cells where horizontal space is limited. */
export function formatCompactDateTime(value: string): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(date);
}

export function statusLabel(status: HoldStatus): string {
  switch (status) {
    case "HELD":
      return "Seat held";
    case "CONFIRMED":
      return "Confirmed";
    case "EXPIRED":
      return "Expired";
    case "CANCELLED":
      return "Cancelled";
  }
}
