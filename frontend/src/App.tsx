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
      <header className="border-b border-slate-200/80 bg-white/85 backdrop-blur">
        <div className="mx-auto flex max-w-6xl flex-col gap-5 px-4 py-5 md:flex-row md:items-center md:justify-between">
          <div className="flex items-start gap-3">
            <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-600 to-sky-500 text-sm font-black text-white shadow-md">
              SA
            </div>
            <div>
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
                  ? "bg-white text-slate-900 shadow-sm"
                  : "text-slate-600 hover:text-slate-900"
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
                  ? "bg-white text-slate-900 shadow-sm"
                  : "text-slate-600 hover:text-slate-900"
              }`}
            >
              Admin console
            </button>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-6xl space-y-6 px-4 py-7">
        {view === "student" ? <StudentPortal /> : <AdminPortal />}

        <footer className="border-t border-slate-200 pt-5 text-center text-xs text-slate-500">
          Seat counts refresh every few seconds. Expired holds release their seats
          automatically — no page reload is needed.
        </footer>
      </main>
    </div>
  );
}
