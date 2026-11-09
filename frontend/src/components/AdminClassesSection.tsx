import type { ClassSummary } from "../api/types";
import { ClassSeatStats } from "./ClassSeatStats";

interface AdminClassesSectionProps {
  classes: ClassSummary[];
  isPending: boolean;
  hasError: boolean;
}

export function AdminClassesSection({
  classes,
  isPending,
  hasError,
}: AdminClassesSectionProps) {
  return (
    <section className="min-w-0 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-bold text-slate-900">Created classes</h2>
          <p className="mt-1 text-sm text-slate-600">
            A live view of the classes and shifts students can currently apply for.
          </p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
          <span className="live-dot h-1.5 w-1.5 rounded-full bg-emerald-500" />
          Live · every 3s
        </span>
      </div>

      {isPending && classes.length === 0 ? (
        <div className="mt-5 grid gap-3 sm:grid-cols-2">
          {[0, 1, 2, 3].map((item) => (
            <div
              key={item}
              className="h-36 animate-pulse rounded-xl border border-slate-200 bg-slate-50"
            />
          ))}
        </div>
      ) : null}

      {!isPending && !hasError && classes.length === 0 ? (
        <div className="mt-5 rounded-xl border border-dashed border-slate-300 bg-slate-50 px-5 py-10 text-center">
          <p className="font-semibold text-slate-800">No classes have been created yet</p>
          <p className="mt-1 text-sm text-slate-500">
            Use the class and shift selector above to open the first class.
          </p>
        </div>
      ) : null}

      {classes.length > 0 ? (
        <div className="mt-5 grid gap-3 sm:grid-cols-2">
          {classes.map((classRoom) => {
            const full = classRoom.availableSeats <= 0;

            return (
              <article
                key={classRoom.id}
                className="min-w-0 rounded-xl border border-slate-200 bg-slate-50/60 p-4"
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
                    {full ? "Full" : `${classRoom.availableSeats} free`}
                  </span>
                </div>

                <ClassSeatStats classRoom={classRoom} />
              </article>
            );
          })}
        </div>
      ) : null}
    </section>
  );
}
