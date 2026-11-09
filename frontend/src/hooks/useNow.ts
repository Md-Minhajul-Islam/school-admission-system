import { useEffect, useState } from "react";
import { serverNow } from "../lib/serverTime";

/** Re-renders once per second so countdowns and expiry states stay current. */
export function useNow(intervalMs = 1000): number {
  const [now, setNow] = useState(() => serverNow());

  useEffect(() => {
    const timer = window.setInterval(() => setNow(serverNow()), intervalMs);
    return () => window.clearInterval(timer);
  }, [intervalMs]);

  return now;
}
