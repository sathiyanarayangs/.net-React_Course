namespace SecureFileVault.Core.Auth;

/// <summary>
/// Task 3.15 - "in-memory users" per the brief. Deliberately tiny: this
/// week's point is the login/role-check mechanism, not user management.
/// </summary>
public record User(string Username, string PasswordHash, string Role);

public class UserStore
{
    private readonly Dictionary<string, User> _usersByUsername = new(StringComparer.OrdinalIgnoreCase);

    public UserStore()
    {
        // Seed one account per role so 3.15's "Student gets 403, Teacher
        // gets 200" proof has both to log in as.
        Seed("alice.teacher", "TeacherPass123!", "Teacher");
        Seed("bob.student", "StudentPass123!", "Student");
    }

    private void Seed(string username, string plainPassword, string role)
    {
        _usersByUsername[username] = new User(username, PasswordHasher.Hash(plainPassword), role);
    }

    public User? FindByUsername(string username) =>
        _usersByUsername.TryGetValue(username, out var user) ? user : null;
}
