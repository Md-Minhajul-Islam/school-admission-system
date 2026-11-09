let offsetMs = 0;

/**
 * Estimates the server clock from the HTTP `Date` header. The offset is refreshed on every
 * API response so a wrong local clock does not make an active hold look expired.
 */
export function syncServerTime(dateHeader: string | null): void {
  if (!dateHeader) {
    return;
  }

  const serverTime = new Date(dateHeader).getTime();
  if (!Number.isNaN(serverTime)) {
    offsetMs = serverTime - Date.now();
  }
}

export function serverNow(): number {
  return Date.now() + offsetMs;
}
