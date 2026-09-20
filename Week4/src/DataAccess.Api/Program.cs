using DataAccess.AdoNet;
using DataAccess.Core.Models;
using DataAccess.Core.Repositories;
using DataAccess.Core.Services;
using DataAccess.EfCodeFirst;
using DataAccess.EfDbFirst;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ScaffoldedDbContext = DataAccess.EfDbFirst.Scaffolded.ScaffoldedDbContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DataAccess API",
        Version = "v1",
        Description = "Week 4 - the same IRepository<Student> served by ADO.NET, EF Code First, or EF DB First, chosen entirely by the 'DataLayer' setting below."
    });
});

// Task 4.1 - the connection string comes from configuration (appsettings.json,
// environment variables, user secrets, etc via IConfiguration), never
// hardcoded in source.
string sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServer in configuration.");
string sqlServerCodeFirstConnectionString = builder.Configuration.GetConnectionString("SqlServerCodeFirst")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:SqlServerCodeFirst in configuration.");
Console.WriteLine($"[DEBUG] SqlServerCodeFirst = {sqlServerCodeFirstConnectionString}");
string sqliteDemoConnectionString = builder.Configuration.GetConnectionString("SqliteDemo")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:SqliteDemo in configuration.");

// Task 4.5 - the login user store is always ADO.NET-backed, independent of
// which layer is currently serving Student CRUD below. Uses the same
// SQLite-file connection factory as the AdoNet Student layer for a
// from-the-box runnable demo; point this at Microsoft.Data.SqlClient with
// the SqlServer connection string for a real deployment.
builder.Services.AddSingleton<Func<System.Data.Common.DbConnection>>(_ =>
    () => new SqliteConnection(sqliteDemoConnectionString));
builder.Services.AddScoped<IUserStore, AdoNetUserRepository>();

// Task 4.11 - the ENTIRE data layer is chosen by this one setting. No
// controller, service, or interface changes when you flip it — only this
// block of Program.cs (the composition root) knows or cares which
// concrete repository is behind IRepository<Student> right now.
string dataLayer = builder.Configuration["DataLayer"] ?? "AdoNet";

switch (dataLayer)
{
    case "AdoNet":
        builder.Services.AddScoped<IRepository<Student>, AdoNetStudentRepository>();
        break;

    case "EfCodeFirst":
        // Points at its own database (DataAccessWeek4_CodeFirst), NOT the
        // one CreateTables.sql populated — Code First generates its own
        // schema from scratch via migrations, so it can't share a database
        // that already has same-named tables created a different way.
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(sqlServerCodeFirstConnectionString));
        builder.Services.AddScoped<IRepository<Student>, EfStudentRepository>();
        break;

    case "EfDbFirst":
        // Deliberately points at the SAME database ADO.NET's CreateTables.sql
        // created — Task 4.9's "treat the Day 1/2 database as owned by
        // another team" scenario means DB First scaffolds off tables that
        // already exist, not a fresh schema of its own.
        builder.Services.AddDbContext<ScaffoldedDbContext>(options => options.UseSqlServer(sqlServerConnectionString));
        builder.Services.AddScoped<IRepository<Student>, EfDbFirstStudentRepository>();
        break;

    default:
        throw new InvalidOperationException($"Unknown DataLayer setting '{dataLayer}'. Expected AdoNet, EfCodeFirst, or EfDbFirst.");
}

builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

// The SQLite demo schema backs Task 4.5's login store (Users table)
// REGARDLESS of which DataLayer is chosen for Student CRUD below — login
// is always ADO.NET/SQLite-backed in this demo, per the comment above.
// Students/Teachers tables are also created here for the "AdoNet" case;
// they're simply unused (but harmless) when EfCodeFirst/EfDbFirst is
// selected instead, since those point Student CRUD at SQL Server via
// sqlServerConnectionString instead.
using (var connection = new SqliteConnection(sqliteDemoConnectionString))
{
    connection.Open();
    SqliteSchemaBootstrapper.EnsureCreated(connection);
}

// Task 4.5 - seed the same two demo accounts Week 3's in-memory UserStore
// had, but now as real rows, so `POST /api/auth/login` is testable
// immediately without a manual INSERT first.
await SeedDemoUsersAsync(app.Services, sqliteDemoConnectionString);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "DataAccess API v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Logger.LogInformation("DataAccess API started with DataLayer = {DataLayer}", dataLayer);

app.Run();

static async Task SeedDemoUsersAsync(IServiceProvider services, string sqliteDemoConnectionString)
{
    using var connection = new SqliteConnection(sqliteDemoConnectionString);
    connection.Open();
    using var checkCommand = connection.CreateCommand();
    checkCommand.CommandText = "SELECT COUNT(*) FROM Users";
    long existingCount = (long)(await checkCommand.ExecuteScalarAsync() ?? 0L);
    if (existingCount > 0) return; // already seeded on a previous run

    using var scope = services.CreateScope();
    var userStore = scope.ServiceProvider.GetRequiredService<IUserStore>();

    await userStore.AddAsync(new User
    {
        Username = "alice.teacher",
        PasswordHash = DataAccess.Core.Auth.PasswordHasher.Hash("TeacherPass123!"),
        Role = "Teacher"
    });
    await userStore.AddAsync(new User
    {
        Username = "bob.student",
        PasswordHash = DataAccess.Core.Auth.PasswordHasher.Hash("StudentPass123!"),
        Role = "Student"
    });
}

public partial class Program { }