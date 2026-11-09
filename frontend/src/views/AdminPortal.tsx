import { CreateClassSection } from "../components/CreateClassSection";

export function AdminPortal() {
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
          Create the classes students can apply for. The student portal stays separate, so
          admission actions are not mixed with school administration.
        </p>
      </section>

      <CreateClassSection />
    </div>
  );
}
