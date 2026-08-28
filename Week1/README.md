# Week 1 Patterns Lab

Exceptions, async/await, GoF design patterns, TPL, Reflection/Attributes,
interface-vs-abstract & static-vs-instance, and IComparable/IComparer sorting.

## Structure

```
PatternsLab.sln
src/PatternsLab/            <- console app, one folder per day, run via Program.cs
  Day1/                     Task 1.1-1.3: exceptions, IDisposable, async/await
  Day2/                     Task 1.4-1.6: Singleton, Factory/Factory Method, Observer
  Day3/                     Task 1.7-1.9: Strategy, Repository+UoW, Adapter/Facade
                             + PATTERN_NOTES.md (8 extra pattern notes)
  Day4/                     Task 1.10-1.12: TPL, Reflection, custom Attribute
  Day5/                     Task 1.13-1.14: interface vs abstract, static vs
                             instance, IComparable/IComparer sorting
tests/PatternsLab.Tests/    xUnit tests: Strategy, Factory, Singleton,
                             Repository/UoW, MathHelper, IComparable/IComparer
PATTERNS.md                 pattern -> future-week map (Task 1.15)
```

## Run the demo console app

```bash
dotnet run --project src/PatternsLab/PatternsLab.csproj
```

This runs every Day 1-5 demo in sequence and prints output for each task.

## Run the tests with coverage

```bash
dotnet test tests/PatternsLab.Tests/PatternsLab.Tests.csproj \
  --collect:"XPlat Code Coverage"
```

This produces a Cobertura XML report under
`tests/PatternsLab.Tests/TestResults/<run-id>/coverage.cobertura.xml`.
To turn that into a human-readable HTML summary (optional):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:tests/PatternsLab.Tests/TestResults/**/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html
```

Then open `coverage-report/index.html`. Coverage is focused on the pattern
implementations (Strategy, Factory, Singleton, Repository/UoW, MathHelper,
IComparable/IComparer) per the Week 1 testing focus — that's where ≥80% is
both easy and meaningful, per the assignment brief.

## Notes

- Targets **.NET 8**. Requires the .NET 8 SDK.
- `dotnet restore` needs access to nuget.org for the test project's packages
  (xunit, Microsoft.NET.Test.Sdk, coverlet.collector). The main console
  project has no external dependencies and builds offline.
- Each `Day*` demo class has a `Run()` (or `RunAsync()`) method that's also
  called directly from the xUnit tests where relevant, so the same code
  backs both the console output and the automated tests.
