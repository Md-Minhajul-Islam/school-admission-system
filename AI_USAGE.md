# AI usage

## Tools used

- **DeepSeek V4.1 Flash Free** via the **OpenCode** coding agent: planning,
  scaffolding, implementation, refactoring, test authoring, Docker configuration, and
  documentation.
- An ad-hoc **Playwright** script drove the real UI against the Docker stack for browser
  smoke checks. It is not part of the committed test suite.

## What the AI was used for

- Proposing the Clean Architecture / CQRS layout and project structure.
- Generating the domain entities, commands/queries, handlers, validators, AutoMapper
  profile, repository ports and implementations, EF Core configurations, migrations,
  middleware, background worker, and API controllers.
- Writing the unit, integration, expiry, and multi-host concurrency tests.
- Building the React/TypeScript/Vite/Tailwind UI, including polling, countdown logic,
  runtime configuration, and Docker/nginx setup.
- Drafting `README.md`, `ASSUMPTIONS.md`, and this file.

## Verification performed

- `dotnet build` completed with 0 warnings and 0 errors.
- The backend suite passed: **47 tests** (28 unit + 19 integration), including the two-host
  concurrency test.
- `npm test` passed (7 Vitest tests) and `npm run build` completed the TypeScript and Vite
  production build.
- `npm audit` reported no vulnerabilities.
- `docker compose up --build` was exercised against the running API and UI, including a
  persistence check across `docker compose down` / `up`.

## Mistakes found and corrected

| # | Mistake | Correction | Found by |
|---:|---|---|---|
| 1 | Vulnerable `AutoMapper` package pin | Upgraded to `16.3.0` | `dotnet restore` (`NU1903`) |
| 2 | EF Core / Npgsql relational package version conflict | Pinned `Microsoft.EntityFrameworkCore.Relational` explicitly | `dotnet build` (`MSB3277`) |
| 3 | Controllers mapped without `AddControllers()` | Registered MVC services | integration-test startup failure |
| 4 | xUnit rejected the test factory as a class fixture (multiple public constructors) | Added a single-constructor `AdmissionApiFixture` wrapper | test run |
| 5 | Expiry test compared API ticks with PostgreSQL microsecond precision | Added a 1 ms tolerance | failing integration test |
| 6 | `Response.Clear()` removed CORS headers from error responses | Removed the clear; added a regression test | CORS check |
| 7 | Scalar SQL query returned a column named `now` instead of `Value` | Changed to `SELECT statement_timestamp() AS "Value"` | EF Core exception |
| 8 | Vulnerable Vitest dev dependencies | Upgraded Vitest to `5.1.0` | `npm audit` |
| 9 | Fixed-width applications table clipped the View button; `/favicon.ico` returned 404 | Replaced the table with a responsive list and added a favicon | Playwright smoke check |
| 10 | Class-name uniqueness was case-sensitive | Replaced the index with a functional unique index on `lower(name)` | review and duplicate-name test |
| 11 | Student IDs were coerced instead of rejected | Added the 17-digit rule and an inline error | review of the input path |
| 12 | Test files were grouped by test type instead of by the code under test | Mirrored `src` in both test projects and split combined files | review |
| 13 | UI button labels did not match the brief's quoted labels | Changed them to **Apply**, **Pay Admission Fee**, and **Cancel** | final BRD review |
| 14 | Enabled buttons showed the default cursor | Added a base-layer CSS rule | UI review |

## How issues were identified

- `dotnet build` / `dotnet restore` warnings and errors.
- Failing xUnit and Vitest tests (startup DI, fixture rules, timestamp precision, CORS).
- `npm audit`.
- A final review of the code, tests, UI, and documentation against the BRD.
- Manual `docker compose` runs against the API and frontend, including the persistence check.
