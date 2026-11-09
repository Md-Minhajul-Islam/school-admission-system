import { describe, expect, it } from "vitest";
import {
  effectiveStatus,
  formatCountdown,
  secondsRemaining,
} from "./format";

describe("countdown helpers", () => {
  it("formats minutes and seconds", () => {
    expect(formatCountdown(0)).toBe("0:00");
    expect(formatCountdown(9)).toBe("0:09");
    expect(formatCountdown(605)).toBe("10:05");
    expect(formatCountdown(3725)).toBe("1:02:05");
  });

  it("never reports negative remaining time", () => {
    const expired = new Date(Date.now() - 5_000).toISOString();
    expect(secondsRemaining(expired)).toBe(0);
  });

  it("shows a held application as expired once the countdown reaches zero", () => {
    const expiresAt = new Date(Date.now() - 1_000).toISOString();
    expect(effectiveStatus("HELD", expiresAt)).toBe("EXPIRED");
    expect(effectiveStatus("HELD", new Date(Date.now() + 60_000).toISOString())).toBe(
      "HELD",
    );
    expect(effectiveStatus("CONFIRMED", expiresAt)).toBe("CONFIRMED");
  });
});
