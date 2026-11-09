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

interface Selection {
  classNumber: ClassNumber;
  shift: ClassShift;
}

export function CreateClassSection({
  classes,
  classesReady,
  classesError,
}: CreateClassSectionProps) {
  const queryClient = useQueryClient();
  const [selection, setSelection] = useState<Selection | null>(null);
  const [seats, setSeats] = useState("120");
  const [formError, setFormError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: api.createClass,
    onSuccess: (created) => {
      setSelection(null);
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
  const selectedName = selection
    ? classNameFor(selection.classNumber, selection.shift)
    : null;
  const selectedExists = selectedName ? classByName.has(selectedName) : false;

  const seatsNumber = Number(seats);
  const seatsValid =
    Number.isInteger(seatsNumber) && seatsNumber >= 1 && seatsNumber <= 10000;
  const canSubmit =
    Boolean(selection) &&
    seatsValid &&
    classesReady &&
    !selectedExists &&
    !mutation.isPending;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSuccess(null);

    if (!selection || !selectedName) {
      setFormError("Choose a class and a shift.");
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
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-bold text-slate-900">Create a class</h2>
          <p className="mt-1 max-w-2xl text-sm text-slate-600">
            Pick one of the fixed classes from 1 to 10 and choose either the Morning or
            Day shift. Each shift has its own seat capacity.
          </p>
        </div>
        <span className="rounded-full bg-slate-100 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-slate-600">
          Admin
        </span>
      </div>

      <form className="mt-5 space-y-5" onSubmit={handleSubmit} noValidate>
        <fieldset>
          <legend className="text-sm font-semibold text-slate-700">
            Class and shift
          </legend>

          <div className="mt-3 grid gap-2 sm:grid-cols-2">
            {CLASS_NUMBERS.map((classNumber) => {
              const selectedInClass =
                selection?.classNumber === classNumber;

              return (
                <div
                  key={classNumber}
                  className={`rounded-xl border p-3 transition ${
                    selectedInClass
                      ? "border-indigo-200 bg-indigo-50/50"
                      : "border-slate-200 bg-slate-50/60"
                  }`}
                >
                  <p className="text-sm font-bold text-slate-900">
                    Class {classNumber}
                  </p>

                  <div className="mt-2 grid grid-cols-2 gap-2">
                    {CLASS_SHIFTS.map((shift) => {
                      const name = classNameFor(classNumber, shift);
                      const existing = classByName.get(name);
                      const selected =
                        selection?.classNumber === classNumber &&
                        selection.shift === shift;
                      const disabled =
                        Boolean(existing) || !classesReady || mutation.isPending;

                      return (
                        <button
                          key={shift}
                          type="button"
                          aria-pressed={selected}
                          disabled={disabled}
                          onClick={() => {
                            setSelection({ classNumber, shift });
                            setFormError(null);
                            setSuccess(null);
                            mutation.reset();
                          }}
                          className={`rounded-lg border px-2.5 py-2 text-left transition ${
                            existing
                              ? "cursor-not-allowed border-slate-200 bg-slate-100 text-slate-400"
                              : selected
                                ? "border-indigo-400 bg-indigo-600 text-white shadow-sm"
                                : "border-slate-200 bg-white text-slate-700 hover:border-indigo-300 hover:bg-indigo-50 disabled:cursor-not-allowed disabled:opacity-50"
                          }`}
                        >
                          <span className="block text-xs font-bold">{shift}</span>
                          <span
                            className={`mt-0.5 block text-[10px] ${
                              selected ? "text-indigo-100" : "text-slate-500"
                            }`}
                          >
                            {existing
                              ? `Created · ${existing.totalSeats} seats`
                              : selected
                                ? "Selected"
                                : "Select"}
                          </span>
                        </button>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>

          {!classesReady ? (
            <p className="mt-3 text-xs font-medium text-amber-700">
              {classesError
                ? "The existing class list could not be loaded. Retry it above before creating a class."
                : "Loading the existing class list before enabling creation…"}
            </p>
          ) : null}
        </fieldset>

        <div className="grid gap-4 sm:grid-cols-[minmax(0,1fr)_minmax(0,1.4fr)] sm:items-end">
          <div>
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
              className="mt-1.5 w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm shadow-sm outline-none transition focus:border-indigo-400 focus:ring-4 focus:ring-indigo-100 disabled:bg-slate-100"
            />
          </div>

          <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 px-4 py-3">
            <p className="text-[10px] font-semibold uppercase tracking-wide text-slate-500">
              Selected class
            </p>
            <p className="mt-1 text-sm font-semibold text-slate-900">
              {selectedName ?? "No class and shift selected"}
            </p>
            {selectedExists ? (
              <p className="mt-1 text-xs font-medium text-rose-600">
                This class and shift already exist.
              </p>
            ) : null}
          </div>
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
          className="w-full rounded-xl bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {mutation.isPending
            ? "Creating class…"
            : !classesReady
              ? classesError
                ? "Class list unavailable"
                : "Waiting for class list…"
              : !selection
                ? "Select a class and shift"
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
