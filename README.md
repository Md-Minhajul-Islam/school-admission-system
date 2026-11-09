# School Admission System

A first-come, first-served school admission system. Parents hold a seat for a class, then
pay within a configurable window to confirm it. School administrators create classes.

- **Backend:** .NET 10, ASP.NET Core, PostgreSQL, EF Core, Clean Architecture, CQRS with
  MediatR, FluentValidation, AutoMapper, repository pattern.
- **Frontend:** React 18, TypeScript, Vite, Tailwind CSS, TanStack Query.
- **Everything runs with Docker Compose.**

---

## Quick start

Requirements: Docker Desktop (or Docker Engine + Compose v2). Nothing else is required.

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | <http://localhost:3000> |
| Backend API | <http://localhost:8080> |
| Health check | <http://localhost:8080/health> |
| PostgreSQL | `localhost:5432` (optional direct access) |

The first build takes a few minutes while the .NET, Node, and PostgreSQL images are
downloaded. The backend applies EF Core migrations automatically on startup, waiting for
PostgreSQL to become ready. There is no seed data: create a class from the UI.

Stop the stack with:

```bash
docker compose down
```

The PostgreSQL data lives in the named volume `pgdata`, so classes and applications
survive `docker compose down` followed by `docker compose up` (and container restarts).
Use `docker compose down -v` only when you intentionally want to delete all data.

---

## Using the application

1. Open <http://localhost:3000>.
2. Enter a **17-digit** student ID (birth registration number) at the top. It is stored in
   the browser and reused after a refresh.
3. As a school administrator, create a class with a name and seat capacity.
4. Click **Apply for admission** on a class. A seat is held for `HOLD_DURATION_SECONDS`
   (default 600 seconds).
5. The **Current application** panel shows a live countdown. Click **Pay admission fee**
   before it reaches zero to confirm the seat, or **Cancel application** to release it.
6. **My applications** lists every application for the current student. It refreshes
   automatically, as do class seat counts. Expired holds are shown as `EXPIRED`, their seats
   return to the class, and the payment button is disabled without a page reload.

Payment is simulated: clicking the button always succeeds while the hold is active.

---

## Configuration

Copy `.env.example` to `.env` to override any default. All variables are optional.

| Variable | Default | Used by | Description |
|---|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | backend | How long a seat is held before automatic expiry. |
| `DATABASE_URL` | `Host=db;Port=5432;Database=school_admission;Username=postgres;Password=postgres` | backend | Full PostgreSQL connection string. |
| `POSTGRES_USER` | `postgres` | db | PostgreSQL user. |
| `POSTGRES_PASSWORD` | `postgres` | db | PostgreSQL password. |
| `POSTGRES_DB` | `school_admission` | db | Database name. |
| `BACKEND_PORT` | `8080` | compose | Host port mapped to the API. |
| `FRONTEND_PORT` | `3000` | compose | Host port mapped to the UI. |
| `POSTGRES_PORT` | `5432` | compose | Host port mapped to PostgreSQL. |
| `API_BASE_URL` | `http://localhost:8080` | frontend | API URL injected into the browser at container start. |
| `CORS_ORIGINS` | `http://localhost:3000,http://127.0.0.1:3000` | backend | Comma-separated allowed browser origins. |
| `TEST_DATABASE_URL` | `Host=db;Port=5432;Database=postgres;Username=postgres;Password=postgres` | tests | Maintenance database used by the test factory. |

If you change `BACKEND_PORT`, also set `API_BASE_URL` to the matching URL. If you change
`FRONTEND_PORT`, add the new origin to `CORS_ORIGINS`.

The API also reads the standard ASP.NET Core key `ConnectionStrings__DefaultConnection`
when `DATABASE_URL` is not set, so the backend can be run outside Docker.

---

## API

All endpoints use JSON. Writes related to a student require the `X-Student-Id` header with
a 17-digit value.

| # | Method | Path | Success | Purpose |
|---:|---|---|---|---|
| 1 | `POST` | `/classes` | `201` | Create a class (`name`, `totalSeats`). |
| 2 | `GET` | `/classes` | `200` | List classes with seat counts. |
| 3 | `GET` | `/classes/{classId}` | `200` | Seat status of one class. |
| 4 | `POST` | `/classes/{classId}/holds` | `201` | Apply for admission (hold a seat). |
| 5 | `GET` | `/holds/{holdId}` | `200` | Current status of an application. |
| 6 | `POST` | `/holds/{holdId}/confirm` | `200` | Pay the fee and confirm admission. |
| 7 | `DELETE` | `/holds/{holdId}` | `204` | Cancel an application. |
| 8 | `GET` | `/me/holds` | `200` | Applications belonging to the student. |
| — | `GET` | `/health` | `200` / `503` | Database connectivity health check. |

Status values are uppercase: `HELD`, `CONFIRMED`, `EXPIRED`, `CANCELLED`. `expiresAt` is an
ISO-8601 UTC timestamp ending in `Z`. Error responses use
`application/problem+json` with a user-readable `detail`.

Example:

```bash
# Create a class
curl -X POST http://localhost:8080/classes \
  -H "Content-Type: application/json" \
  -d '{"name":"Class 6 - Morning Shift","totalSeats":120}'

# Apply as a student
curl -X POST http://localhost:8080/classes/<classId>/holds \
  -H "X-Student-Id: 20152691234567890"

# Pay
curl -X POST http://localhost:8080/holds/<holdId>/confirm \
  -H "X-Student-Id: 20152691234567890"
```

---

## How concurrency and expiry are guaranteed

Applications for the same class are serialized with a PostgreSQL row lock:

1. `POST /classes/{classId}/holds` opens a transaction and runs
   `SELECT * FROM classes WHERE id = <classId> FOR UPDATE`.
2. While holding that row lock it counts occupied seats directly from `holds`
   (`CONFIRMED`, or `HELD` with `expires_at > now()`), and inserts the new hold only when
   the count is below `total_seats`.
3. The transaction commits after the insert, releasing the lock.

Every API process sharing the database uses the same row lock, so **held + confirmed seats
can never exceed `totalSeats`**, even across multiple backend instances.

Two extra safeguards back this up:

- Denormalized seat counters are not stored. `heldSeats`, `confirmedSeats`, and
  `availableSeats` are computed from `holds`, so they cannot drift out of sync.
- A partial unique index on `(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')`
  prevents duplicate active applications per student per class.

All time comparisons use PostgreSQL `now()`, so clock differences between API processes do
not matter. A background worker marks overdue holds as `EXPIRED` every second, and read
queries also compute the effective status, so expiry is visible even between worker ticks.
Confirmation uses a guarded `UPDATE ... WHERE status = 'HELD' AND expires_at > now()`,
making repeated confirmation idempotent.

More detail is in [ASSUMPTIONS.md](ASSUMPTIONS.md).

---

## Running the tests

### Backend tests in Docker (recommended)

```bash
docker compose --profile test run --rm --build backend-tests
```

This starts PostgreSQL, runs the unit and integration tests in a .NET SDK container, and
removes the test container afterwards. The integration tests create and drop isolated
PostgreSQL databases, so they do not touch application data.

The suite contains **42 backend tests**, including:

- domain/validator/mapping/handler unit tests;
- API contract, ownership, idempotency, cancellation, no-seats and invalid-input tests;
- automatic expiry test;
- a **two-host concurrency test** that sends 40 simultaneous applications for 5 seats and
  asserts exactly 5 successes with `heldSeats` never exceeding `totalSeats`.

### Backend tests on a local machine

Requires a running PostgreSQL and the .NET 10 SDK:

```powershell
$env:TEST_DATABASE_URL = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"
dotnet test backend/SchoolAdmission.sln
```

`TEST_DATABASE_URL` must point at a database the test user can use to create other
databases (the `postgres` maintenance database by default).

### Frontend tests

```bash
cd frontend
npm ci
npm test
npm run build
```

Vitest covers the countdown, expiry, and formatting helpers. The production build is also
type-checked with `tsc`.

---

## Project structure

```
.
├── backend/
│   ├── src/
│   │   ├── SchoolAdmission.Domain/          # entities, enums, and behaviour
│   │   ├── SchoolAdmission.Application/     # CQRS handlers, validators, ports, mappings
│   │   ├── SchoolAdmission.Infrastructure/  # EF Core, repositories, migrations
│   │   └── SchoolAdmission.Api/             # controllers, middleware, worker, startup
│   ├── tests/
│   │   ├── SchoolAdmission.Application.UnitTests/
│   │   └── SchoolAdmission.Api.IntegrationTests/
│   ├── Dockerfile
│   └── Dockerfile.tests
├── frontend/
│   ├── src/
│   │   ├── api/                             # typed API client and error mapping
│   │   ├── components/                      # UI sections
│   │   ├── hooks/                           # polling, student ID, clock
│   │   └── lib/                             # countdown helpers + Vitest tests
│   ├── Dockerfile
│   └── nginx.conf
├── docker-compose.yml
├── .env.example
├── ASSUMPTIONS.md
└── AI_USAGE.md
```

Backend layering: `Domain` ← `Application` ← `Infrastructure` ← `Api`. Repository
interfaces are defined in `Application`; EF Core lives only in `Infrastructure`.
Input validation and normalization are handled in `Application` by FluentValidation
validators and command handlers; domain entities only hold state and behaviour.
`IRepository` is the transaction/`SaveChanges` port (the assignment's unit-of-work concept
under the requested name), while `IClassRoomRepository` and `IHoldRepository` are
aggregate-specific repositories. There is no generic repository.

---

## Troubleshooting

- **Port already in use:** set `BACKEND_PORT`, `FRONTEND_PORT`, or `POSTGRES_PORT` in `.env`.
  If you change the backend or frontend port, update `API_BASE_URL` / `CORS_ORIGINS` too.
- **Frontend shows “could not reach the admission server”:** confirm
  <http://localhost:8080/health> returns `{"status":"ok"}` and that `API_BASE_URL` matches
  the browser-visible API URL.
- **Start from a clean database:** `docker compose down -v` and run `docker compose up --build`.
- **Backend logs:** `docker compose logs -f backend` (migration retries and request logs are
  written there).
