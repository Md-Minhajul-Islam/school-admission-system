export const CLASS_NUMBERS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] as const;

export const CLASS_SHIFTS = ["Morning", "Day"] as const;

export type ClassNumber = (typeof CLASS_NUMBERS)[number];

export type ClassShift = (typeof CLASS_SHIFTS)[number];

export function classNameFor(classNumber: ClassNumber, shift: ClassShift): string {
  return `Class ${classNumber} - ${shift} Shift`;
}
