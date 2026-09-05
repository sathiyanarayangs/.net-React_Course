namespace StudentApi.Models;

/// <summary>
/// Task 2.3 - the entity. Note the internal-only field (InternalNotes):
/// Task 2.7 proves this never leaks into API responses because only DTOs
/// cross the wire, never this class directly.
/// </summary>
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal Score { get; set; } // 0-100, used by the grading strategy

    // Internal-only field: e.g. admin notes on a student record. Should
    // never appear in any StudentReadDto response (Task 2.7's proof point).
    public string InternalNotes { get; set; } = string.Empty;
}
