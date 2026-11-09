import { errorMessage } from "../api/client";
import { AdminClassesSection } from "../components/AdminClassesSection";
import { CreateClassSection } from "../components/CreateClassSection";
import { Notice } from "../components/Notice";
import { useClassesQuery } from "../hooks/useAdmissionQueries";

export function AdminPortal() {
  const classesQuery = useClassesQuery();
  const classes = classesQuery.data ?? [];

  return (
    <div className="space-y-6">
      <section className="relative overflow-hidden rounded-2xl border border-emerald-100 bg-gradient-to-br from-emerald-50/80 via-white to-teal-50/60 p-5 shadow-sm sm:p-6">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-10 -top-12 h-36 w-36 rounded-full bg-emerald-200/40 blur-2xl"
        />
        <div className="relative flex flex-wrap items-start justify-between gap-4">
          <div className="min-w-0">
            <span className="inline-flex items-center gap-2 rounded-full bg-emerald-100/80 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              Admin console
            </span>
            <h2 className="mt-2 text-lg font-bold text-slate-900">
              Open classes and manage capacity
            </h2>
            <p className="mt-1 max-w-2xl text-sm text-slate-600">
              Create a class for each shift and set the seats available to students.
            </p>
          </div>
          <span className="inline-flex shrink-0 items-center gap-2 rounded-full border border-emerald-200 bg-white/80 px-3 py-1.5 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
            <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
            Administrator
          </span>
        </div>
      </section>

      {classesQuery.error ? (
        <Notice tone="error" title="We could not load the class list">
          <p>{errorMessage(classesQuery.error)}</p>
          <button
            type="button"
            onClick={() => {
              void classesQuery.refetch();
            }}
            className="mt-3 rounded-lg border border-rose-300 bg-white px-3 py-1.5 text-xs font-semibold text-rose-700 hover:bg-rose-50"
          >
            Try again
          </button>
        </Notice>
      ) : null}

      <CreateClassSection
        classes={classes}
        classesReady={!classesQuery.isPending && !classesQuery.error}
        classesError={Boolean(classesQuery.error)}
      />

      <AdminClassesSection
        classes={classes}
        isPending={classesQuery.isPending}
        hasError={Boolean(classesQuery.error)}
      />
    </div>
  );
}
