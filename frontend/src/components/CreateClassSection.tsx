import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { api, errorMessage } from "../api/client";
import { Notice } from "./Notice";

export function CreateClassSection() {
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [seats, setSeats] = useState("120");
  const [formError, setFormError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: api.createClass,
    onSuccess: (created) => {
      setName("");
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

  const seatsNumber = Number(seats);
  const seatsValid = Number.isInteger(seatsNumber) && seatsNumber >= 1 && seatsNumber <= 10000;

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSuccess(null);

    if (!name.trim()) {
      setFormError("Enter a class name, for example “Class 6 - Morning Shift”.");
      return;
    }

    if (!seatsValid) {
      setFormError("Seat capacity must be a whole number between 1 and 10000.");
      return;
    }

    setFormError(null);
    mutation.mutate({ name: name.trim(), totalSeats: seatsNumber });
  }

  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-lg font-bold text-slate-900">Create a class</h2>
          <p className="mt-1 text-sm text-slate-600">
            School admin: each shift is a separate class with its own seat capacity.
          </p>
        </div>
        <span className="rounded-full bg-slate-100 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-slate-600">
          Admin
        </span>
      </div>

      <form className="mt-5 space-y-4" onSubmit={handleSubmit} noValidate>
        <div>
          <label htmlFor="class-name" className="text-sm font-semibold text-slate-700">
            Class name
          </label>
          <input
            id="class-name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            placeholder="Class 6 - Morning Shift"
            maxLength={200}
            className="mt-1.5 w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm shadow-sm outline-none transition focus:border-indigo-400 focus:ring-4 focus:ring-indigo-100"
          />
        </div>

        <div>
          <label htmlFor="class-seats" className="text-sm font-semibold text-slate-700">
            Total seats
          </label>
          <input
            id="class-seats"
            type="number"
            min={1}
            max={10000}
            step={1}
            value={seats}
            onChange={(event) => setSeats(event.target.value)}
            className="mt-1.5 w-full rounded-xl border border-slate-300 bg-white px-3.5 py-2.5 text-sm shadow-sm outline-none transition focus:border-indigo-400 focus:ring-4 focus:ring-indigo-100"
          />
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
          disabled={mutation.isPending}
          className="w-full rounded-xl bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
        >
          {mutation.isPending ? "Creating class…" : "Create class"}
        </button>
      </form>
    </section>
  );
}
