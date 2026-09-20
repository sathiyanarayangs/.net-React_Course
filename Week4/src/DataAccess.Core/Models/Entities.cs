namespace DataAccess.Core.Models;

/// <summary>
/// Task 4.1 - the domain entity persisted three different ways this week
/// (ADO.NET, EF Code First, EF DB First) behind the exact same
/// IRepository&lt;Student&gt; interface.
/// </summary>
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Task 4.8 - added in the second EF Core migration, after the table
    // already existed, to prove a schema can evolve without recreating it.
    public DateTime? EnrolledOn { get; set; }
}

/// <summary>Task 4.1 - a thin Teacher table, included for the ER diagram's ≥3NF shape.</summary>
public class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

/// <summary>
/// Task 4.5 - the Week 3 in-memory login user, now a real row instead of a
/// Dictionary entry. PasswordHash is the same "iterations.salt.hash" PBKDF2
/// format Week 3 used — moving storage engines doesn't change the format.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Teacher" or "Student"
}
