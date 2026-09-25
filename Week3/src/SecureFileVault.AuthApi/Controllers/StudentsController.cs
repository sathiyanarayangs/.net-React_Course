using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureFileVault.AuthApi.Controllers;

public record StudentDto(int Id, string Name, string Dob, string Designation, string Email);

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private static readonly List<StudentDto> _students = new()
    {
        new(1, "Sample Student", "2000-01-01", "Student", "student@example.com"),
        new(2, "Jane Doe", "1999-05-15", "Student", "jane@example.com")
    };

    [HttpGet]
    [Authorize]
    public ActionResult<IEnumerable<StudentDto>> GetStudents()
    {
        return Ok(_students);
    }
}
