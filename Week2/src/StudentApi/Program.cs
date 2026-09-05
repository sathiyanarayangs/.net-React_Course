using StudentApi.Models;
using StudentApi.Patterns;
using StudentApi.Repositories;
using StudentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Task 2.1: default middleware/services the template gives you ----
// AddControllers wires up MVC-style controllers + [ApiController] model
// binding/validation (used by Task 2.6's automatic 400 behaviour).
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student API",
        Version = "v1",
        Description = "Week 2 - unsecured on purpose. Repository + Strategy/Factory patterns applied over an in-memory store."
    });
});

// ---- Task 2.3: Repository pattern, reused from Week 1's seam ----
// AddSingleton: one shared in-memory "database" for the app's lifetime.
builder.Services.AddSingleton<IRepository<Student>, StudentRepository>();
builder.Services.AddSingleton<IRepository<Teacher>, TeacherRepository>();

// ---- Task 2.8: Strategy + Factory for grading ----
// Register every IGradeStrategy; the factory (below) picks one per request.
builder.Services.AddSingleton<IGradeStrategy, PercentageGradeStrategy>();
builder.Services.AddSingleton<IGradeStrategy, GpaGradeStrategy>();
builder.Services.AddSingleton<IGradeStrategyFactory, GradeStrategyFactory>();

// AddScoped: a fresh service instance per HTTP request. Services hold no
// state of their own (state lives in the singleton repository), so scoped
// is the natural, justified lifetime here rather than singleton or transient.
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();

var app = builder.Build();

// ---- Middleware pipeline ----
// Default order for an API-only template: Swagger (dev only) -> HTTPS
// redirection -> routing (implicit) -> authorization -> controllers.
// There's no authentication/authorization middleware doing anything yet —
// this API is unsecured on purpose per the Week 2 brief.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Student API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposes the implicit Program class to StudentApi.Tests for any future
// WebApplicationFactory<Program>-based integration tests.
public partial class Program { }
