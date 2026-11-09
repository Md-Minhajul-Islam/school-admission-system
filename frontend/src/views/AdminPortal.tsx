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
      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm sm:p-6">
        <p className="text-[11px] font-semibold uppercase tracking-wide text-indigo-600">
          Admin console
        </p>
        <h2 className="mt-1 text-lg font-bold text-slate-900">
          Open classes and manage capacity
        </h2>
        <p className="mt-1 max-w-2xl text-sm text-slate-600">
          Pick one of the fixed classes from 1 to 10 and either the Morning or Day shift.
          Existing combinations are locked, and the student portal stays separate from
          administration.
        </p>
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
