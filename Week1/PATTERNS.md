# PATTERNS.md — Task 1.15

Each pattern built in Week 1 mapped to where it resurfaces later in the course.

| Pattern | Built in | Where it appears later |
|---|---|---|
| **Repository + Unit of Work** | Day 3, Task 1.8 (`Day3/Repository`) | **Week 4 — Data Access.** Every DB approach (raw ADO.NET, Dapper, EF Core) plugs into the exact same `IRepository<T>` / `IUnitOfWork` seam built here, so swapping the persistence technology later never touches calling code. |
| **Strategy** | Day 3, Task 1.7 (`Day3/Strategy`) | **Runtime choices across the web phases** — e.g. selecting an authentication scheme, a pricing/discount rule, or a serialization format per request. Any "pick one algorithm out of several at runtime" decision reuses this shape. |
| **Observer** | Day 2, Task 1.6 (`Day2/Observer`) | **Eventing across the app** — ASP.NET Core's own event/notification hooks, SignalR broadcasts, and in-process domain events (e.g. "OrderPlaced" triggering email + inventory update) are all Observer under the hood; the C#-events version here is the same delegate machinery used there. |
| **Factory / Factory Method** | Day 2, Task 1.5 (`Day2/Vehicles`) | **Service creation / DI container internals.** ASP.NET Core's `IServiceProvider` is effectively a generalized factory; custom factories for things like DB connections or HTTP clients per environment follow this same "caller never calls `new` directly" shape. |
| **Singleton** | Day 2, Task 1.4 (`Day2/Logger`) | **App-wide shared state** — configuration objects, caches, and (later) DI-registered `Singleton` lifetime services in ASP.NET Core all rely on the same "exactly one instance, thread-safe construction" guarantee proven here. |
| **Adapter / Facade** | Day 3, Task 1.9 (`Day3/AdapterFacade`) | **Integrating third-party/legacy systems and simplifying subsystem coordination** — wrapping external payment gateways or legacy SOAP/XML services (Adapter), and exposing a single simple API over multiple internal services (Facade), both come back once the app talks to real external systems. |
| **Async/await + Task.WhenAll** | Day 1, Task 1.3 (`Day1/AsyncDemo.cs`) | **All I/O-bound web code** — controller actions, DB calls, and outbound HTTP calls in every later phase are async all the way down; `Task.WhenAll` is the standard way to parallelize independent I/O (e.g. fetching several API responses to compose one page). |
| **Custom exceptions + Dispose pattern** | Day 1, Tasks 1.1–1.2 | **Cross-cutting error handling & resource management** — custom exceptions become the backbone of API error responses (mapped to HTTP status codes) later; the Dispose pattern reappears anywhere a later phase manages unmanaged/scarce resources (DB connections, file handles, HTTP clients). |
| **Reflection + Attributes** | Day 4, Tasks 1.11–1.12 | **Frameworks rely on this** — ASP.NET Core model binding/validation (`[Required]`, `[MaxLength]`), dependency injection, and serialization all use reflection over attributes exactly like the custom `[MaxLength(n)]` validator built here. |
| **Builder / Prototype / Decorator / Command / Template Method / Mediator / Chain of Responsibility / State** | Day 3, Task 1.9 short notes | Recognized as they appear incidentally later: Decorator in `Stream` wrapping and ASP.NET Core middleware, Chain of Responsibility *is* the middleware pipeline itself, Builder in fluent configuration APIs (e.g. `WebApplicationBuilder`), State in order/workflow status modeling. |

See `src/PatternsLab/Day3/PATTERN_NOTES.md` for the full 3–5 line note + use case on each of the eight "short notes" patterns.
