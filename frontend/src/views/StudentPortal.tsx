import { useEffect, useRef, useState } from "react";
import type { HoldCreated } from "../api/types";
import { ApplicationSection } from "../components/ApplicationSection";
import { ClassesSection } from "../components/ClassesSection";
import { MyApplicationsSection } from "../components/MyApplicationsSection";
import { Notice } from "../components/Notice";
import { StudentIdField } from "../components/StudentIdField";
import {
  useClassesQuery,
  useHoldQuery,
  useMyHoldsQuery,
} from "../hooks/useAdmissionQueries";
import { useNow } from "../hooks/useNow";
import { useStudentId } from "../hooks/useStudentId";
import { effectiveStatus } from "../lib/format";

interface Banner {
  title: string;
  text: string;
}

export function StudentPortal() {
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
    <div className="space-y-6">
      <section className="overflow-hidden rounded-2xl border border-emerald-100 bg-gradient-to-br from-emerald-50/80 via-white to-teal-50/60 p-5 shadow-sm sm:p-6">
        <div className="flex flex-col gap-5 md:flex-row md:items-start md:justify-between">
          <div className="min-w-0">
            <span className="inline-flex items-center gap-2 rounded-full bg-emerald-100/80 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-wide text-emerald-700">
              <span className="h-1.5 w-1.5 rounded-full bg-emerald-500" />
              Student portal
            </span>
            <h2 className="mt-2 text-lg font-bold text-slate-900">
              Find a class and hold your seat
            </h2>
            <p className="mt-1 max-w-xl text-sm text-slate-600">
              Enter your student ID, choose an open shift, and pay before the countdown
              ends.
            </p>
          </div>

          <StudentIdField
            value={studentId}
            isValid={isValid}
            onChange={setStudentId}
          />
        </div>

        <ol className="mt-5 grid gap-2 text-xs text-slate-600 sm:grid-cols-3">
          <li className="flex items-center gap-2 rounded-xl border border-emerald-100 bg-white/70 px-3 py-2">
            <span className="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-emerald-600 text-[10px] font-bold text-white">
              1
            </span>
            <span className="min-w-0 leading-snug">Enter your 17-digit student ID.</span>
          </li>
          <li className="flex items-center gap-2 rounded-xl border border-emerald-100 bg-white/70 px-3 py-2">
            <span className="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-emerald-600 text-[10px] font-bold text-white">
              2
            </span>
            <span className="min-w-0 leading-snug">Apply for a class to hold a seat.</span>
          </li>
          <li className="flex items-center gap-2 rounded-xl border border-emerald-100 bg-white/70 px-3 py-2">
            <span className="flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-emerald-600 text-[10px] font-bold text-white">
              3
            </span>
            <span className="min-w-0 leading-snug">
              Pay before the countdown ends to confirm.
            </span>
          </li>
        </ol>
      </section>

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
    </div>
  );
}
