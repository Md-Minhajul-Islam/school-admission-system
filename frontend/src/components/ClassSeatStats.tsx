import type { ClassSummary } from "../api/types";

type SeatCounts = Pick<
  ClassSummary,
  "totalSeats" | "heldSeats" | "confirmedSeats" | "availableSeats"
>;

export function ClassSeatStats({ classRoom }: { classRoom: SeatCounts }) {
  const stats = [
    { label: "Total", value: classRoom.totalSeats, tone: "text-slate-900" },
    { label: "Held", value: classRoom.heldSeats, tone: "text-amber-600" },
    { label: "Confirmed", value: classRoom.confirmedSeats, tone: "text-emerald-600" },
    { label: "Free", value: classRoom.availableSeats, tone: "text-teal-600" },
  ];

  return (
    <dl className="mt-4 grid grid-cols-2 gap-2 text-center">
      {stats.map((stat) => (
        <div
          key={stat.label}
          className="min-w-0 rounded-xl bg-white px-2 py-2.5 ring-1 ring-slate-200"
        >
          <dt className="break-words text-[10px] font-semibold uppercase leading-tight tracking-wide text-slate-500">
            {stat.label}
          </dt>
          <dd className={`mt-0.5 text-base font-bold ${stat.tone}`}>{stat.value}</dd>
        </div>
      ))}
    </dl>
  );
}
