using System.ComponentModel.DataAnnotations;

namespace StudentApi.Dtos;

/// <summary>
/// Task 2.7 (in) - what the client sends on POST/PUT. Deliberately has no
/// Id (server-assigned) and no InternalNotes (clients can't set it).
/// Task 2.6 - the data annotations here are what makes [ApiController]
/// auto-return 400 for invalid bodies, with no manual `if` checks.
/// </summary>
public class StudentCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(5, 100)]
    public int Age { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal Score { get; set; }
}

/// <summary>
/// Task 2.7 (out) - what the client receives. Note there is no
/// InternalNotes property here at all — that's the proof that the internal
/// field can never leak, regardless of what the entity holds.
/// </summary>
public class StudentReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Grade { get; set; } = string.Empty; // computed via IGradeStrategy
}
