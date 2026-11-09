import { useEffect, useRef, useState } from "react";
import type { HoldCreated } from "./api/types";
import { ApplicationSection } from "./components/ApplicationSection";
import { ClassesSection } from "./components/ClassesSection";
import { CreateClassSection } from "./components/CreateClassSection";
import { MyApplicationsSection } from "./components/MyApplicationsSection";
import { Notice } from "./components/Notice";
import { StudentIdField } from "./components/StudentIdField";
import {
  useClassesQuery,
  useHoldQuery,
  useMyHoldsQuery,
} from "./hooks/useAdmissionQueries";
import { useNow } from "./hooks/useNow";
import { useStudentId } from "./hooks/useStudentId";
import { effectiveStatus } from "./lib/format";

interface Banner {
  title: string;
  text: string;
}

export default function App() {
  const { studentId, setStudentId, isValid } = useStudentId();
  const now = useNow();
  const classesQuery = useClassesQuery();
  const myHoldsQuery = useMyHoldsQuery(studentId, isValid);

  const [selectedHoldId, setSelectedHoldId] = useState<string | null>(null);
  const [banner, setBanner] = useState<Banner | null>(null);
  const applicationRef = useRef<HTMLDivElement>(null);

  const holdQuery = useHoldQuery(selectedHoldId);

  useEffect(() => {
    setSelectedHoldId(null);
    setBanner(null);
  }, [studentId]);

  useEffect(() => {
    if (!isValid || selectedHoldId) {
      return;
    }

    const holds = myHoldsQuery.data;
    if (!holds || holds.length === 0) {
      return;
    }

    const active = holds.find(
      (hold) => effectiveStatus(hold.status, hold.expiresAt) === "HELD",
    );

    setSelectedHoldId((active ?? holds[0]).holdId);
  }, [isValid, selectedHoldId, myHoldsQuery.data]);

  const myHolds = myHoldsQuery.data ?? [];
  const selectedMyHold = selectedHoldId
    ? myHolds.find((hold) => hold.holdId === selectedHoldId)
    : undefined;
  const selectedClassId = holdQuery.data?.classId ?? selectedMyHold?.classId;
  const selectedClass = selectedClassId
    ? (classesQuery.data ?? []).find((classRoom) => classRoom.id === selectedClassId)
    : undefined;
  const selectedClassName =
    selectedClass?.name ?? selectedMyHold?.className ?? "";

  function handleApplied(hold: HoldCreated) {
    setSelectedHoldId(hold.holdId);
    setBanner({
      title: "Seat held",
      text: "Pay the admission fee before the countdown ends, otherwise the seat is released automatically.",
    });

    window.requestAnimationFrame(() => {
      applicationRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  }

  return (
    <div className="min-h-screen pb-12">
      <header className="border-b border-slate-200/80 bg-white/85 backdrop-blur">
        <div className="mx-auto flex max-w-6xl flex-col gap-5 px-4 py-5 md:flex-row md:items-start md:justify-between">
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

          <StudentIdField
            value={studentId}
            isValid={isValid}
            onChange={setStudentId}
          />
        </div>
      </header>

      <main className="mx-auto max-w-6xl space-y-6 px-4 py-7">
        <ol className="grid gap-2 text-xs text-slate-600 sm:grid-cols-3">
          <li className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white/70 px-3 py-2">
            <span className="flex h-5 w-5 items-center justify-center rounded-full bg-indigo-600 text-[10px] font-bold text-white">
              1
            </span>
            Enter your 17-digit student ID.
          </li>
          <li className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white/70 px-3 py-2">
            <span className="flex h-5 w-5 items-center justify-center rounded-full bg-indigo-600 text-[10px] font-bold text-white">
              2
            </span>
            Apply for a class to hold a seat.
          </li>
          <li className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white/70 px-3 py-2">
            <span className="flex h-5 w-5 items-center justify-center rounded-full bg-indigo-600 text-[10px] font-bold text-white">
              3
            </span>
            Pay before the countdown ends to confirm.
          </li>
        </ol>

        {banner ? (
          <Notice tone="success" title={banner.title} onDismiss={() => setBanner(null)}>
            {banner.text}
          </Notice>
        ) : null}

        <div className="grid gap-6 lg:grid-cols-[minmax(0,1.45fr)_minmax(0,1fr)] lg:items-start">
          <div className="space-y-6">
            <ClassesSection
              classes={classesQuery.data ?? []}
              isPending={classesQuery.isPending}
              error={classesQuery.error}
              onRetry={() => {
                void classesQuery.refetch();
              }}
              studentId={studentId}
              studentIdValid={isValid}
              onApplied={handleApplied}
            />

            <CreateClassSection />
          </div>

          <div className="space-y-6">
            <div ref={applicationRef} className="scroll-mt-24">
              <ApplicationSection
                holdId={selectedHoldId}
                hold={holdQuery.data}
                className={selectedClassName}
                studentId={studentId}
                studentIdValid={isValid}
                isPending={holdQuery.isPending}
                error={holdQuery.error}
                onRetry={() => {
                  void holdQuery.refetch();
                }}
                now={now}
              />
            </div>

            <MyApplicationsSection
              studentIdValid={isValid}
              holds={myHolds}
              isPending={myHoldsQuery.isPending}
              error={myHoldsQuery.error}
              onRetry={() => {
                void myHoldsQuery.refetch();
              }}
              selectedHoldId={selectedHoldId}
              onSelect={setSelectedHoldId}
              now={now}
            />
          </div>
        </div>

        <footer className="border-t border-slate-200 pt-5 text-center text-xs text-slate-500">
          Seat counts refresh every few seconds. Expired holds release their seats
          automatically — no page reload is needed.
        </footer>
      </main>
    </div>
  );
}
