# Week 4 — Database Access (ADO.NET, EF Code First, EF DB First)

The same `IRepository<Student>` seam implemented three ways, swappable by
one config value, plus the Week 3 login user moved from an in-memory
Dictionary into a real table.

## Structure

```
Week4.sln
src/
  DataAccess.Core/          Student/Teacher/User models, IRepository<T>/IUserStore, StudentService, PasswordHasher
  DataAccess.AdoNet/         AdoNetStudentRepository (4.2-4.4), AdoNetUserRepository (4.5), Sql/*.sql (4.1, 4.4)
  DataAccess.EfCodeFirst/    AppDbContext (4.6), Migrations/ (4.7, 4.8), EfStudentRepository (4.7)
  DataAccess.EfDbFirst/      Scaffolded/StudentsRow + ScaffoldedDbContext (4.9), EfDbFirstStudentRepository (4.10)
  DataAccess.Api/            Program.cs picks the data layer from config (4.11); StudentsController, AuthController
tests/DataAccess.Tests/      xUnit — see Testing Focus below
ER-DIAGRAM.md                 Task 4.1 (Mermaid ER diagram + normalization notes)
DATA-COMPARISON.md            Task 4.12
```

## Which parts are runnable out of the box, and which need a real SQL Server

This project is honest about a real constraint: ADO.NET and EF DB First
scaffolding are both meant to point at an already-existing database, and EF
Code First's whole point is generating one via migrations — none of that
can be faked with an in-memory substitute without contradicting the
assignment. So:

- **`DataLayer: "AdoNet"`** (the default in `appsettings.json`) runs
  immediately, no setup required — it points at a local SQLite file
  (`adonet-demo.db`) that gets created and seeded automatically on first
  run. This is what you can `dotnet run` right now.
- **`DataLayer: "EfCodeFirst"`** and **`"EfDbFirst"`** both point at
  `ConnectionStrings:SqlServer` in `appsettings.json`, which needs a real
  SQL Server instance (LocalDB, SQL Server Express, or a full instance) to
  actually work end-to-end. See "Running against real SQL Server" below.
- The stored-procedure methods (`GetByIdViaStoredProcedureAsync`,
  `AddViaStoredProcedureAsync` in `AdoNetStudentRepository`) also need real
  SQL Server — SQLite has no stored procedures — so they're demonstrated in
  code and covered by `Sql/StoredProcedures.sql`, but aren't hit by the
  automated test suite (which runs against SQLite).

## Run the API (AdoNet layer — works immediately)

```bash
dotnet run --project src/DataAccess.Api/DataAccess.Api.csproj
```

Swagger opens at `/swagger`. Two demo accounts are seeded automatically
(Task 4.5's login, now hitting a real — if local — database):

| Username | Password | Role |
|---|---|---|
| `alice.teacher` | `TeacherPass123!` | Teacher |
| `bob.student` | `StudentPass123!` | Student |

Try `POST /api/auth/login`, then the full CRUD set under `/api/students`.

## Running against real SQL Server (EfCodeFirst / EfDbFirst)

1. Install SQL Server (Express/Developer edition) or LocalDB, and SSMS.
2. Update `ConnectionStrings:SqlServer` in `src/DataAccess.Api/appsettings.json` to match your instance.
3. Run `src/DataAccess.AdoNet/Sql/CreateTables.sql` then `Sql/StoredProcedures.sql` in SSMS (Task 4.1, 4.4) — this is the database EF DB First scaffolds from.
4. For EF Code First: install the EF tools if you haven't (`dotnet tool install --global dotnet-ef`), then from `src/DataAccess.EfCodeFirst`:
   ```bash
   dotnet ef database update --startup-project ../DataAccess.Api
   ```
   This applies both migrations in this repo (`Initial`, `AddEnrolledOn`) for real against your SQL Server — regenerate them yourself with `dotnet ef migrations add Initial` first if you want fresh Designer/Snapshot files rather than the hand-authored ones included here (see the comment in `Migrations/20240110120000_Initial.cs`).
5. For EF DB First: after step 3, you can regenerate the scaffolded classes for real with:
   ```bash
   dotnet ef dbcontext scaffold "<your connection string>" Microsoft.EntityFrameworkCore.SqlServer --context ScaffoldedDbContext --output-dir Scaffolded --force
   ```
6. Change `DataLayer` in `appsettings.json` to `"EfCodeFirst"` or `"EfDbFirst"` and `dotnet run` again.

## Task 4.11 — proving the swap

With a real SQL Server set up per above: run the exact same Postman
requests against `/api/students` with `DataLayer` set to `AdoNet`, then
`EfCodeFirst`, then `EfDbFirst` (restarting the app between each change) —
responses should be indistinguishable. That's the whole point of the seam.

## Run the tests with coverage

```bash
dotnet test tests/DataAccess.Tests/DataAccess.Tests.csproj --collect:"XPlat Code Coverage"
```

No SQL Server needed for any of this — tests use SQLite in-memory (ADO.NET
layer) and EF Core's InMemory provider (both EF layers), per the brief's
own testing-focus guidance.

## Testing focus (per the brief)

- Service tested against a **mocked** `IRepository<Student>` — `StudentServiceTests` (proves the service's logic doesn't depend on which concrete data layer is behind it)
- EF Code First CRUD round-trips via **EF Core InMemory** — `EfCodeFirstRepositoryTests`
- EF DB First CRUD round-trips, same InMemory approach — `EfDbFirstRepositoryTests`
- ADO.NET parameterized-query injection proof — `AdoNetStudentRepositoryTests.AddAsync_WithSqlInjectionPayloadAsName_StoresItAsLiteralText`, plus general ADO.NET CRUD via SQLite in-memory
- Login/password verify — `PasswordHasherTests`, `AdoNetUserRepositoryTests`

`Program.cs` is excluded from the coverage denominator (`<ExcludeByFile>` in the test csproj) — same reasoning as Weeks 1-3.

## Notes

- Targets **.NET 8**.
- `AdoNetStudentRepository`/`AdoNetUserRepository` are written against
  `System.Data.Common` (`DbConnection`/`DbCommand`), not `SqlConnection`
  directly, so the exact same repository code runs against either
  `Microsoft.Data.SqlClient` (production, SQL Server) or
  `Microsoft.Data.Sqlite` (the demo/test provider) — only the injected
  connection factory changes.
- Every ADO.NET query uses parameters, never string-concatenated SQL —
  see the injection test above for the proof.
