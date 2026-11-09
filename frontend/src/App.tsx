import { useEffect, useState } from "react";
import { AdminPortal } from "./views/AdminPortal";
import { StudentPortal } from "./views/StudentPortal";

type PortalView = "student" | "admin";

function viewFromHash(): PortalView {
  return window.location.hash === "#admin" ? "admin" : "student";
}

export default function App() {
  const [view, setView] = useState<PortalView>(viewFromHash);

  useEffect(() => {
    const syncView = () => setView(viewFromHash());
    window.addEventListener("hashchange", syncView);
    return () => window.removeEventListener("hashchange", syncView);
  }, []);

  function navigate(next: PortalView) {
    if (viewFromHash() === next) {
      setView(next);
      return;
    }

    window.location.hash = next;
  }

  return (
    <div className="min-h-screen pb-12">
      <header className="border-b border-emerald-100 bg-white/85 backdrop-blur">
        <div className="mx-auto flex max-w-7xl flex-col gap-5 px-4 py-5 sm:px-6 md:flex-row md:items-center md:justify-between">
          <div className="flex min-w-0 items-start gap-3">
            <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-emerald-600 to-teal-500 text-sm font-black text-white shadow-md">
              SA
            </div>
            <div className="min-w-0">
              <h1 className="text-xl font-black tracking-tight text-slate-900">
                School Admission
              </h1>
              <p className="mt-0.5 max-w-md text-sm text-slate-600">
                First come, first served. Hold a seat, then pay before the timer runs out.
              </p>
            </div>
          </div>

          <nav
            aria-label="Choose portal"
            className="flex w-full rounded-xl border border-slate-200 bg-slate-100 p-1 md:w-auto"
          >
            <button
              type="button"
              onClick={() => navigate("student")}
              aria-current={view === "student" ? "page" : undefined}
              className={`flex-1 rounded-lg px-3.5 py-2 text-sm font-semibold transition md:flex-none ${
                view === "student"
                  ? "bg-white text-emerald-800 shadow-sm"
                  : "text-slate-600 hover:text-emerald-700"
              }`}
            >
              Student portal
            </button>
            <button
              type="button"
              onClick={() => navigate("admin")}
              aria-current={view === "admin" ? "page" : undefined}
              className={`flex-1 rounded-lg px-3.5 py-2 text-sm font-semibold transition md:flex-none ${
                view === "admin"
                  ? "bg-white text-emerald-800 shadow-sm"
                  : "text-slate-600 hover:text-emerald-700"
              }`}
            >
              Admin console
            </button>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-7xl space-y-6 px-4 py-7 sm:px-6">
        {view === "student" ? <StudentPortal /> : <AdminPortal />}

        <footer className="border-t border-slate-200 pt-5 text-center text-xs text-slate-500">
          Seat counts refresh every few seconds. Expired holds release their seats
          automatically — no page reload is needed.
        </footer>
      </main>
    </div>
  );
}
