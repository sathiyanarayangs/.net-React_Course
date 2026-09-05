# Week 2 — Student API (Unsecured on Purpose)

ASP.NET Core Web API applying the Week 1 patterns (Repository, Strategy/Factory)
over a Student/Teacher domain. No auth, no database — just clean API thinking.

## Structure

```
StudentApi.sln
src/StudentApi/
  Controllers/         StudentsController, TeachersController — thin, no business logic
  Models/               Student (with an internal-only field), Teacher
  Dtos/                 StudentCreateDto/StudentReadDto, TeacherCreateDto/TeacherReadDto
  Repositories/         IRepository<T> + InMemoryRepository<T> (the Week 1 seam, reused)
  Services/             IStudentService/StudentService, ITeacherService/TeacherService
  Patterns/             IGradeStrategy (Percentage/GPA) + GradeStrategyFactory — Task 2.8
  Program.cs             composition root: DI registrations, Swagger, middleware pipeline
tests/StudentApi.Tests/
  GradeStrategyTests.cs               strategy output + factory selection logic
  StudentServiceTests.cs              service layer, mocked IRepository<Student>
  StudentsControllerTests.cs          status codes (200/201/204/404) via real service + in-memory repo
  StudentValidationIntegrationTests.cs the automatic 400 behaviour (needs the real HTTP pipeline)
  DtoMappingTests.cs                  InternalNotes / Id never leak through the DTOs
  TeacherServiceTests.cs              stretch task coverage
```

## Run the API

```bash
dotnet run --project src/StudentApi/StudentApi.csproj
```

Swagger UI opens automatically at `https://localhost:7236/swagger` (or
`http://localhost:5236/swagger`) — every endpoint is documented and
exercisable straight from there. Update the ports here once you know your
actual `launchSettings.json` values, and paste the real Swagger URL into
this section before submitting.

### Endpoints

| Verb | Route | Success | Failure |
|---|---|---|---|
| GET | `/api/students` | 200 | — |
| GET | `/api/students/{id}` | 200 | 404 |
| GET | `/api/students/search?name=...` | 200 (matches or empty list) | — |
| POST | `/api/students` | 201 + `Location` header | 400 (invalid body) |
| PUT | `/api/students/{id}` | 204 | 404 / 400 |
| DELETE | `/api/students/{id}` | 204 | 404 |
| GET/POST/PUT/DELETE | `/api/teachers...` | same shape as above (stretch task) | |

`gradeStrategy` is an optional query string on the student GET/POST
endpoints (`?gradeStrategy=percentage` or `?gradeStrategy=gpa`) — this is
Task 2.8's swappable-per-request Strategy/Factory in action.

### Postman

Import the endpoints above into a collection (or export one from Swagger's
`swagger.json`) and paste the shared collection link here before submitting.

## Run the tests with coverage

```bash
dotnet test tests/StudentApi.Tests/StudentApi.Tests.csproj --collect:"XPlat Code Coverage"
```

`Program.cs` (composition-root wiring) is excluded from the coverage
denominator via `<ExcludeByFile>` in the test csproj, same reasoning as
Week 1 — DI registration and middleware order aren't unit-testable logic,
so including them would dilute the percentage without saying anything
about the code that matters (services, patterns, DTO mapping, controllers).

## Notes

- Targets **.NET 8**, Swashbuckle.AspNetCore for Swagger/OpenAPI.
- `dotnet restore` needs nuget.org access for both projects this week (the
  Web API needs Swashbuckle; the tests need xUnit, Moq, and
  Microsoft.AspNetCore.Mvc.Testing for the integration test).
- The API is intentionally unsecured — no authentication/authorization
  middleware does anything meaningful yet. That's explicit in the Week 2
  brief and will presumably change in a later week.
