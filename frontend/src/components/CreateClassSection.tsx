import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { api, errorMessage } from "../api/client";
import type { ClassSummary } from "../api/types";
import {
  CLASS_NUMBERS,
  CLASS_SHIFTS,
  classNameFor,
  type ClassNumber,
  type ClassShift,
} from "../lib/classCatalog";
import { Notice } from "./Notice";

interface CreateClassSectionProps {
  classes: ClassSummary[];
  classesReady: boolean;
  classesError: boolean;
}

export function CreateClassSection({
  classes,
  classesReady,
  classesError,
}: CreateClassSectionProps) {
  const queryClient = useQueryClient();
  const [classNumber, setClassNumber] = useState<ClassNumber | "">("");
  const [shift, setShift] = useState<ClassShift | "">("");
  const [seats, setSeats] = useState("120");
  const [formError, setFormError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: api.createClass,
    onSuccess: (created) => {
      setClassNumber("");
      setShift("");
      setSeats("120");
      setFormError(null);
      setSuccess(
        `“${created.name}” is now open with ${created.totalSeats} ${
          created.totalSeats === 1 ? "seat" : "seats"
        }.`,
      );
      void queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });

  const classByName = new Map(classes.map((classRoom) => [classRoom.name, classRoom]));
  const selectedName =
    classNumber !== "" && shift !== "" ? classNameFor(classNumber, shift) : null;
  const selectedExists = selectedName ? classByName.has(selectedName) : false;

  const seatsNumber = Number(seats);
  const seatsValid =
    Number.isInteger(seatsNumber) && seatsNumber >= 1 && seatsNumber <= 10000;
  const canSubmit =
    Boolean(selectedName) &&
    seatsValid &&
    classesReady &&
    !selectedExists &&
    !mutation.isPending;

  function handleClassChange(value: string) {
    const nextClassNumber = value === "" ? "" : (Number(value) as ClassNumber);

    setClassNumber(nextClassNumber);
    setFormError(null);
    setSuccess(null);
    mutation.reset();

    if (nextClassNumber === "") {
      setShift("");
      return;
    }

    const firstAvailableShift = CLASS_SHIFTS.find(
      (candidate) => !classByName.has(classNameFor(nextClassNumber, candidate)),
    );
    setShift(firstAvailableShift ?? "");
  }

  function handleShiftChange(value: string) {
    setShift(value === "" ? "" : (value as ClassShift));
    setFormError(null);
    setSuccess(null);
    mutation.reset();
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSuccess(null);

    if (classNumber === "" || shift === "" || !selectedName) {
      setFormError("Choose a class name and a class shift.");
      return;
    }

    if (!classesReady) {
      setFormError("Wait for the existing class list to load before creating a class.");
      return;
    }

    if (selectedExists) {
      setFormError(`${selectedName} already exists.`);
      return;
    }

    if (!seatsValid) {
      setFormError("Seat capacity must be a whole number between 1 and 10000.");
      return;
    }

    setFormError(null);
    mutation.mutate({ name: selectedName, totalSeats: seatsNumber });
  }

  return (
    <section className="min-w-0 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <h2 className="text-lg font-bold text-slate-900">Create a class</h2>
          <p className="mt-1 max-w-2xl text-sm text-slate-600">
            Choose a class name and a class shift from the dropdowns, then set the seat
            capacity. Each shift has its own capacity.
          </p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
          <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
          Admin
        </span>
      </div>

      <form className="mt-5 space-y-5" onSubmit={handleSubmit} noValidate>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <div className="min-w-0">
            <label htmlFor="class-name" className="text-sm font-semibold text-slate-700">
              Class name
            </label>
            <div className="relative mt-1.5">
              <select
                id="class-name"
                value={classNumber}
                onChange={(event) => handleClassChange(event.target.value)}
                disabled={!classesReady || mutation.isPending}
                className="w-full appearance-none rounded-xl border border-slate-300 bg-white py-2.5 pl-3.5 pr-10 text-sm shadow-sm outline-none transition focus:border-emerald-400 focus:ring-4 focus:ring-emerald-100 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500"
              >
                <option value="">Select a class</option>
                {CLASS_NUMBERS.map((candidate) => {
                  const allShiftsCreated = CLASS_SHIFTS.every((candidateShift) =>
                    classByName.has(classNameFor(candidate, candidateShift)),
                  );

                  return (
                    <option
                      key={candidate}
                      value={candidate}
                      disabled={allShiftsCreated}
                    >
                      Class {candidate}
                      {allShiftsCreated ? " (all shifts created)" : ""}
                    </option>
                  );
                })}
              </select>
              <SelectChevron />
            </div>
          </div>

          <div className="min-w-0">
            <label htmlFor="class-shift" className="text-sm font-semibold text-slate-700">
              Class shift
            </label>
            <div className="relative mt-1.5">
              <select
                id="class-shift"
                value={shift}
                onChange={(event) => handleShiftChange(event.target.value)}
                disabled={!classesReady || mutation.isPending || classNumber === ""}
                className="w-full appearance-none rounded-xl border border-slate-300 bg-white py-2.5 pl-3.5 pr-10 text-sm shadow-sm outline-none transition focus:border-emerald-400 focus:ring-4 focus:ring-emerald-100 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500"
              >
                <option value="">Select a shift</option>
                {CLASS_SHIFTS.map((candidate) => {
                  const existing =
                    classNumber === ""
                      ? undefined
                      : classByName.get(classNameFor(classNumber, candidate));

                  return (
                    <option
                      key={candidate}
                      value={candidate}
                      disabled={Boolean(existing)}
                    >
                      {candidate} Shift
                      {existing ? " (created)" : ""}
                    </option>
                  );
                })}
              </select>
              <SelectChevron />
            </div>
          </div>

          <div className="min-w-0">
            <label htmlFor="class-seats" className="text-sm font-semibold text-slate-700">
              Seat capacity
            </label>
            <input
              id="class-seats"
              type="number"
              min={1}
              max={10000}
              step={1}
              value={seats}
              onChange={(event) => setSeats(event.target.value)}
              disabled={mutation.isPending}
              className="mt-1.5 w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm shadow-sm outline-none transition focus:border-emerald-400 focus:ring-4 focus:ring-emerald-100 disabled:bg-slate-100"
            />
          </div>
        </div>

        {!classesReady ? (
          <p className="text-xs font-medium text-amber-700">
            {classesError
              ? "The existing class list could not be loaded. Retry it above before creating a class."
              : "Loading the existing class list before enabling creation…"}
          </p>
        ) : null}

        <div className="rounded-xl border border-dashed border-emerald-200 bg-emerald-50/50 px-4 py-3">
          <p className="text-[10px] font-semibold uppercase tracking-wide text-emerald-700">
            Selected class
          </p>
          <p className="mt-1 break-words text-sm font-semibold text-slate-900">
            {selectedName ?? "No class and shift selected"}
          </p>
          {selectedExists ? (
            <p className="mt-1 text-xs font-medium text-rose-600">
              This class and shift already exist.
            </p>
          ) : null}
        </div>

        {formError ? (
          <Notice tone="error" title="Check the form">
            {formError}
          </Notice>
        ) : null}

        {mutation.isError ? (
          <Notice tone="error" title="The class could not be created">
            {errorMessage(mutation.error)}
          </Notice>
        ) : null}

        {success ? (
          <Notice tone="success" onDismiss={() => setSuccess(null)}>
            {success}
          </Notice>
        ) : null}

        <button
          type="submit"
          disabled={!canSubmit}
          className="w-full rounded-xl bg-emerald-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
        >
          {mutation.isPending
            ? "Creating class…"
            : !classesReady
              ? classesError
                ? "Class list unavailable"
                : "Waiting for class list…"
              : classNumber === ""
                ? "Select a class"
                : shift === ""
                  ? "Select a shift"
                  : selectedExists
                    ? "Class already created"
                    : !seatsValid
                      ? "Enter a valid seat capacity"
                      : `Create ${selectedName}`}
        </button>
      </form>
    </section>
  );
}

function SelectChevron() {
  return (
    <span
      aria-hidden="true"
      className="pointer-events-none absolute inset-y-0 right-3 flex items-center text-slate-400"
    >
      <svg
        viewBox="0 0 20 20"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.8"
        className="h-4 w-4"
      >
        <path d="M6 8l4 4 4-4" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    </span>
  );
}
