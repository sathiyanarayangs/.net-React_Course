namespace StudentApi.Models;

/// <summary>Task 2.10 (stretch) - a second, thinner entity to prove the same
/// controller/service/repository shape generalizes.</summary>
public class Teacher
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
