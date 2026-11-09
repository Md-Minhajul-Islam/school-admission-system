interface StudentIdFieldProps {
  value: string;
  isValid: boolean;
  onChange: (value: string) => void;
}

export function StudentIdField({ value, isValid, onChange }: StudentIdFieldProps) {
  const showError = value.length > 0 && !isValid;
  const hasNonDigits = /[^0-9]/.test(value);
  const errorMessage = hasNonDigits
    ? "Use digits only — the birth registration number is exactly 17 digits."
    : value.length > 17
      ? "Too long — the birth registration number is exactly 17 digits."
      : `Enter all 17 digits — ${17 - value.length} more to go.`;

  return (
    <div className="w-full min-w-0 md:max-w-sm">
      <label
        htmlFor="student-id"
        className="flex flex-wrap items-center justify-between gap-x-3 gap-y-1 text-xs font-semibold uppercase tracking-wide text-slate-600"
      >
        <span className="min-w-0 break-words">Student ID · birth registration number</span>
        <span className={isValid ? "text-emerald-600" : "text-slate-400"}>
          {value.length}/17
        </span>
      </label>

      <div className="relative mt-1.5">
        <input
          id="student-id"
          name="student-id"
          type="text"
          inputMode="numeric"
          autoComplete="off"
          spellCheck={false}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder="e.g. 20152691234567890"
          aria-invalid={showError}
          aria-describedby="student-id-help"
          className={`w-full rounded-xl border bg-white py-2.5 pl-3.5 pr-10 text-sm font-medium tracking-wide shadow-sm outline-none transition focus:ring-4 ${
            showError
              ? "border-rose-300 focus:border-rose-400 focus:ring-rose-100"
              : "border-slate-300 focus:border-emerald-400 focus:ring-emerald-100"
          }`}
        />
        <span
          aria-hidden="true"
          className={`absolute right-3 top-2.5 text-base ${
            isValid ? "text-emerald-500" : "text-slate-300"
          }`}
        >
          {isValid ? "✓" : "•"}
        </span>
      </div>

      <p
        id="student-id-help"
        className={`mt-1.5 text-xs ${showError ? "font-medium text-rose-600" : "text-slate-500"}`}
      >
        {showError
          ? errorMessage
          : "Your number is saved in this browser and identifies your applications."}
      </p>
    </div>
  );
}
