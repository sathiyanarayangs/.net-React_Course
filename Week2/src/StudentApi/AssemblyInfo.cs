using System.Runtime.CompilerServices;

// Lets the test project reference internal types if a test ever needs to
// (the WebApplicationFactory-based controller tests only need public types,
// but this keeps the door open without widening the public API surface).
[assembly: InternalsVisibleTo("StudentApi.Tests")]
