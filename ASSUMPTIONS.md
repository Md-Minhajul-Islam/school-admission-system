# Assumptions and design decisions

The specification leaves some details open. These are the choices made, and why.

## Identity and data model

- **Student identity** is exactly 17 ASCII digits, sent in `X-Student-Id`. No checksum is
  enforced because the brief only defines the format. Empty, shorter, longer, or
  non-numeric values return `400`.
- **IDs** are opaque strings (`c_` + GUID for classes, `h_` + GUID for holds). The examples
  in the brief explicitly allow any ID format.
- **Statuses** are uppercase (`HELD`, `CONFIRMED`, `EXPIRED`, `CANCELLED`) because the
  examples in the brief use uppercase.
- **A class is a shift.** "Class 6 - Morning Shift" and "Class 6 - Day Shift" are separate
  classes, each with its own capacity, exactly as required.
- **The admin console uses a fixed catalog.** The **Class name** and **Class shift**
  dropdowns offer classes 1 through 10 with Morning and Day shifts. The UI generates names
  such as `Class 6 - Morning Shift` and disables combinations that already exist, so the
  same class and shift is not created twice from the interface. A unique index on
  `lower(name)` enforces the same rule for direct API callers: a duplicate name returns
  `409`, and the comparison is case-insensitive.
- **Seat counts are derived, not stored.** `heldSeats` counts `HELD` holds whose
  `expires_at` is still in the future; `confirmedSeats` counts `CONFIRMED` holds;
  `availableSeats = totalSeats - heldSeats - confirmedSeats`. There is no denormalized
  counter that could drift from the holds table.

## Hold lifecycle

- A hold lasts exactly `HOLD_DURATION_SECONDS` (default 600) from creation. Reading the
  application does **not** extend it.
- A new hold starts as `HELD`. Paying moves it to `CONFIRMED`; cancelling moves it to
  `CANCELLED`; an unpaid hold past its deadline becomes `EXPIRED`.
- **Expiry is automatic.** A background worker runs every second and persists overdue
  `HELD` holds as `EXPIRED`. In addition, all read paths compute the effective status from
  `expires_at`, and all seat counts exclude expired holds. Therefore the API is correct
  even if the worker is temporarily between ticks or not running, and no client call is
  needed to release a seat.
- **One active application per student per class.** A student may have a `HELD` or
  `CONFIRMED` application in a class, but not more than one. Once an application is
  `EXPIRED` or `CANCELLED`, the student may apply again.
- **Owner-only confirmation and cancellation.** The header student must own the hold.
  Attempts by another student return `403`.
- **Confirmation is idempotent.** The update is guarded by
  `status = 'HELD' AND expires_at > statement_timestamp()`. If it already succeeded, a second call returns
  `200` with `CONFIRMED` and does not increase `confirmedSeats` again. Confirming an
  expired/cancelled application returns `409`.
- **Cancellation** is allowed only for a live `HELD` application. Repeated cancellation of
  an already cancelled application returns `204` (idempotent). A confirmed admission
  cannot be cancelled (`409`), because the brief does not define a refund/withdrawal
  process.
- **Payment is simulated.** Pressing “Pay” always succeeds while the hold is active.

## Concurrency guarantee (and multiple API processes)

The invariant is: `heldSeats + confirmedSeats <= totalSeats`, at all times.

`POST /classes/{classId}/holds` runs in one database transaction:

1. `SELECT * FROM classes WHERE id = :classId FOR UPDATE` locks the class row. All API
   processes sharing the database contend on this same row, so applications for a class
   are serialized.
2. While the lock is held, occupied seats are counted directly from `holds`
   (`CONFIRMED` or unexpired `HELD`).
3. If the count is below `total_seats`, the new hold is inserted; otherwise the request
   returns `409 Conflict`.
4. Commit releases the lock. A waiting process then sees the newly inserted hold in its
   own count.

Additional safeguards:

- A **partial unique index** on `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')`
  enforces the one-active-application rule even if the locking path were bypassed.
- **Confirmation never adds a seat**: it changes an already-counted `HELD` row to
  `CONFIRMED`, so it cannot break the capacity invariant.
- All comparisons use **PostgreSQL statement time (`statement_timestamp()`)**, not the
  application server clock. This keeps multiple backend processes consistent and avoids a
  stale transaction timestamp after waiting on the class row lock. The hold's `created_at`
  and `expires_at` are also read from PostgreSQL, so its duration does not depend on the
  API host clock. The application `TimeProvider` is used only for the class creation
  timestamp.
- There are no cached counters in memory or in the database, so separate processes cannot
  disagree about the number of occupied seats.
- The integration test `Concurrent_applications_from_two_hosts_never_exceed_total_seats`
  runs two API hosts against one database, sends 40 simultaneous applications for 5 seats,
  samples the class while the requests run, and asserts that occupied seats never exceed
  the capacity, with exactly 5 successes, 35 conflicts, and `heldSeats == 5` at the end.

## API behaviour

- `GET /holds/{holdId}` is intentionally view-only and does not require the student header;
  the brief only requires an owner check for confirmation. Writes (`confirm`, `cancel`,
  `apply`, `me/holds`) require a valid `X-Student-Id`.
- Errors use RFC 7807 Problem Details (`application/problem+json`) with a human-readable
  `detail`. Validation errors also include an `errors` dictionary. HTTP codes:
  `400` invalid input, `403` wrong owner, `404` missing record, `409` unavailable seat /
  duplicate application / expired or non-confirmable hold, `500` unexpected server error.
- `POST /classes` returns `201` with `Location: /classes/{id}`.
- `DELETE /holds/{id}` returns `204`.
- `expiresAt` is serialized as UTC (`Z`).
- CORS is enabled for the configured frontend origin(s), including on error responses.

## Validation limits

- Class name: required, trimmed by the Application layer, at most 200 characters.
- Total seats: whole number from 1 to 10,000. The upper bound is a sanity limit, not from
  the brief.
- Input validation and normalization are enforced by FluentValidation validators and command
  handlers in the Application layer. Domain entities only store already-validated values.
- Unknown/extra JSON properties are ignored by the standard ASP.NET Core binder.

## Running and persistence

- No authentication is implemented. The brief says login is not required and describes
  school-admin actions without a login flow, so class creation is open in this exercise.
- PostgreSQL data is stored in the named Docker volume `pgdata`. `docker compose down`
  keeps it; `docker compose down -v` deletes it.
- Migrations run automatically at backend startup under a PostgreSQL advisory lock, with
  retries while the database starts. This avoids races when multiple backend containers
  start together.
- There is no seed data. Classes are created through the UI/API.

## Frontend

- The 17-digit student ID is persisted in `localStorage`, so it survives refreshes and
  browser restarts. Changing the ID immediately switches the visible applications.
- Class seat counts poll every 3 seconds, the student's applications every 2 seconds, and
  the selected application every second. Countdowns tick locally once per second.
- The UI computes `EXPIRED` locally when the countdown reaches zero and disables
  “Pay admission fee” immediately; the server independently rejects late payment.
- After a reload, the UI loads `GET /me/holds` and automatically selects the newest active
  application (or the most recent application if none is active).
- The API base URL is read at runtime from `window.__APP_CONFIG__.API_BASE_URL`. The Docker
  image writes that file from the `API_BASE_URL` environment variable at container start,
  so the same static build works in different environments. `VITE_API_BASE_URL` is a
  build-time fallback for local development; the final fallback is `http://localhost:8080`.
- All failures are shown in the UI as plain-language notices. An error boundary prevents a
  blank page if rendering fails.
