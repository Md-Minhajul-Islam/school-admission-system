import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api, errorMessage } from "../api/client";
import type { ClassSummary, HoldCreated } from "../api/types";
import { ClassSeatStats } from "./ClassSeatStats";
import { Notice } from "./Notice";

interface ClassesSectionProps {
  classes: ClassSummary[];
  isPending: boolean;
  error: unknown;
  onRetry: () => void;
  studentId: string;
  studentIdValid: boolean;
  onApplied: (hold: HoldCreated) => void;
}

export function ClassesSection({
  classes,
  isPending,
  error,
  onRetry,
  studentId,
  studentIdValid,
  onApplied,
}: ClassesSectionProps) {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (classId: string) => api.applyForClass(classId, studentId),
    onSuccess: (hold) => {
      onApplied(hold);
      void queryClient.invalidateQueries({ queryKey: ["classes"] });
      void queryClient.invalidateQueries({ queryKey: ["my-holds"] });
    },
  });

  return (
    <section className="min-w-0 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-bold text-slate-900">Open classes</h2>
          <p className="mt-1 text-sm text-slate-600">
            Choose a shift and hold a seat. Seat counts refresh on their own.
          </p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
          <span className="live-dot h-1.5 w-1.5 rounded-full bg-emerald-500" />
          Live · every 3s
        </span>
      </div>

      {error ? (
        <div className="mt-5">
          <Notice tone="error" title="We could not load the class list">
            <p>{errorMessage(error)}</p>
            <button
              type="button"
              onClick={onRetry}
              className="mt-3 rounded-lg border border-rose-300 bg-white px-3 py-1.5 text-xs font-semibold text-rose-700 hover:bg-rose-50"
            >
              Try again
            </button>
          </Notice>
        </div>
      ) : null}

      {mutation.isError ? (
        <div className="mt-5">
          <Notice tone="error" title="Could not hold a seat">
            {errorMessage(mutation.error)}
          </Notice>
        </div>
      ) : null}

      {isPending && classes.length === 0 ? (
        <div className="mt-5 grid gap-3 sm:grid-cols-2">
          {[0, 1, 2, 3].map((item) => (
            <div
              key={item}
              className="h-48 animate-pulse rounded-xl border border-slate-200 bg-slate-50"
            />
          ))}
        </div>
      ) : null}

      {!isPending && !error && classes.length === 0 ? (
        <div className="mt-5 rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
          <p className="font-semibold text-slate-800">No classes have been created yet</p>
          <p className="mt-1 text-sm text-slate-500">
            A school administrator can open the first class from the{" "}
            <a
              href="#admin"
              className="font-semibold text-emerald-700 hover:underline"
            >
              Admin console
            </a>
            .
          </p>
        </div>
      ) : null}

      <div className="mt-5 grid gap-3 xl:grid-cols-2">
        {classes.map((classRoom) => {
          const full = classRoom.availableSeats <= 0;
          const applying =
            mutation.isPending && mutation.variables === classRoom.id;

          return (
            <article
              key={classRoom.id}
              className="flex min-w-0 flex-col rounded-xl border border-slate-200 bg-slate-50/60 p-4 transition hover:border-emerald-200 hover:bg-white"
            >
              <div className="flex min-w-0 items-start justify-between gap-3">
                <h3 className="min-w-0 break-words font-semibold leading-snug text-slate-900">
                  {classRoom.name}
                </h3>
                <span
                  className={`shrink-0 rounded-full px-2 py-0.5 text-xs font-bold ${
                    full
                      ? "bg-rose-100 text-rose-700"
                      : "bg-emerald-100 text-emerald-700"
                  }`}
                >
                  {full ? "Full" : `${classRoom.availableSeats} left`}
                </span>
              </div>

              <ClassSeatStats classRoom={classRoom} />

              <button
                type="button"
                disabled={full || !studentIdValid || mutation.isPending}
                onClick={() => {
                  mutation.reset();
                  mutation.mutate(classRoom.id);
                }}
                className="mt-4 w-full rounded-xl bg-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:text-slate-600"
              >
                {full
                  ? "No seats available"
                  : !studentIdValid
                    ? "Enter your student ID first"
                    : applying
                      ? "Holding a seat…"
                      : "Apply for admission"}
              </button>
            </article>
          );
        })}
      </div>
    </section>
  );
}
