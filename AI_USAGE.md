# AI usage

## Tools used

- **DeepSeek V4.1 Flash Free** (via the OpenCode coding agent) was used for planning,
  scaffolding, implementation, refactoring, test authoring, Docker configuration, and
  documentation.

## What the AI was used for

- Proposing the Clean Architecture / CQRS layout and project structure.
- Generating the initial domain entities, commands/queries, handlers, validators,
  AutoMapper profile, repository ports/implementations, EF Core configurations, migration,
  middleware, background worker, and API controllers.
- Writing the unit, integration, expiry, and multi-host concurrency tests.
- Building the React/TypeScript/Vite/Tailwind UI, countdown/polling logic, runtime
  configuration, and Docker/nginx setup.
- Drafting `README.md`, `ASSUMPTIONS.md`, and this file.

All generated code was built, exercised, and reviewed as part of the work; the test suite
and Docker smoke tests were run after the changes.

## Mistakes found and corrected

1. **Vulnerable AutoMapper version** — the first package pin was AutoMapper `14.0.0`.
   `dotnet restore` reported `NU1903` (high-severity advisory). It was upgraded to
   `16.3.0`, and the build/restore became clean.
2. **EF Core assembly version conflict** — `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3`
   pulled EF Core Relational `10.0.4` while the project pinned `10.0.14`, producing
   `MSB3277` warnings. Adding an explicit `Microsoft.EntityFrameworkCore.Relational
   10.0.14` reference resolved the conflict.
3. **Missing MVC registration** — the first API build succeeded, but the integration tests
   failed at startup with “Unable to find the required services… AddControllers”. The AI
   had mapped controllers without calling `builder.Services.AddControllers()`. Fixed in
   `Program.cs`; the integration tests now catch this class of startup regression.
4. **xUnit fixture rule** — xUnit rejected `AdmissionApiFactory` as a class fixture because
   it had multiple public constructors. A single-constructor `AdmissionApiFixture` wrapper
   was added.
5. **Database time precision in a test** — the expiry test compared the creation response
   timestamp (100 ns ticks) with the value read back from PostgreSQL (microsecond
   precision). The test now uses a 1 ms tolerance.
6. **CORS headers lost on errors** — the global exception handler called
   `Response.Clear()`, which also removed the CORS headers added by `UseCors`. The clear was
   removed and a regression test now asserts that a `404` response still contains
   `Access-Control-Allow-Origin`.
7. **Scalar SQL alias** — PostgreSQL `now()` returned a column named `now`, but EF Core's
   scalar `SqlQuery<DateTimeOffset>` expects the column to be named `Value`. The query was
   changed to `SELECT now() AS "Value"`.
8. **Dev-only npm advisories** — Vitest 3 pulled vulnerable `@vitest/mocker`/`tinypool`
   versions. Vitest was upgraded to 5.1.0; `npm audit` now reports zero vulnerabilities.
9. **Clipped applications table and missing favicon** — a browser smoke test (Playwright
   driving the real UI against the Docker stack) showed the fixed-width “My applications”
   table cutting off the **View** button and logging a `404` for `/favicon.ico`. The table
   was replaced with a responsive list, a favicon was added, and the smoke test now reports
   zero console errors and zero failed requests.

## How issues were identified

- Build warnings/errors from `dotnet build`, `dotnet restore`, `tsc`, and `vite build`.
- Failing xUnit tests (startup DI, fixture rules, timestamp precision, CORS regression).
- `npm audit`.
- Manual `docker compose up --build` checks against the running API and frontend, plus a
  persistence check across `docker compose down` / `up`.
