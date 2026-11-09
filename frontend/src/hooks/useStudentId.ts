import { useCallback, useState } from "react";

const STORAGE_KEY = "school-admission.student-id";

function readStoredStudentId(): string {
  try {
    return window.localStorage.getItem(STORAGE_KEY) ?? "";
  } catch {
    return "";
  }
}

function persistStudentId(studentId: string): void {
  try {
    if (studentId.length > 0) {
      window.localStorage.setItem(STORAGE_KEY, studentId);
    } else {
      window.localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // Storage can be unavailable in private browsing; the ID still works for the session.
  }
}

export function useStudentId() {
  const [studentId, setStudentIdState] = useState<string>(readStoredStudentId);

  const setStudentId = useCallback((value: string) => {
    const digitsOnly = value.replace(/\D/g, "").slice(0, 17);
    setStudentIdState(digitsOnly);
    persistStudentId(digitsOnly);
  }, []);

  return {
    studentId,
    setStudentId,
    isValid: /^\d{17}$/.test(studentId),
  };
}
