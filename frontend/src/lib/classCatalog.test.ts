import { describe, expect, it } from "vitest";
import {
  CLASS_NUMBERS,
  CLASS_SHIFTS,
  classNameFor,
} from "./classCatalog";

describe("class catalog", () => {
  it("offers classes 1 through 10", () => {
    expect(CLASS_NUMBERS).toEqual([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
  });

  it("offers the morning and day shifts", () => {
    expect(CLASS_SHIFTS).toEqual(["Morning", "Day"]);
  });

  it("builds the expected class names", () => {
    expect(classNameFor(1, "Morning")).toBe("Class 1 - Morning Shift");
    expect(classNameFor(10, "Day")).toBe("Class 10 - Day Shift");
  });

  it("produces a unique name for every class and shift combination", () => {
    const names = CLASS_NUMBERS.flatMap((classNumber) =>
      CLASS_SHIFTS.map((shift) => classNameFor(classNumber, shift)),
    );

    expect(names).toHaveLength(20);
    expect(new Set(names).size).toBe(20);
  });
});
