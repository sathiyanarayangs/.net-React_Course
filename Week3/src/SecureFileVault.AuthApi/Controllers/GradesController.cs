using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecureFileVault.AuthApi.Controllers;

public record PostGradeRequest(string StudentName, string Grade);

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    /// <summary>
    /// Task 3.15's proof point: this write endpoint is restricted to the
    /// Teacher role via the standard [Authorize(Roles = "Teacher")]
    /// attribute — the same attribute a full JWT-bearer setup would use.
    /// A valid Teacher token gets 200; a valid Student token (authenticated,
    /// just the wrong role) gets 403, not 401 — the distinction between
    /// "who you are" and "what you may do" that Task 3.14 explains.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public IActionResult PostGrade([FromBody] PostGradeRequest request)
    {
        return Ok(new { message = $"Recorded grade '{request.Grade}' for {request.StudentName}." });
    }

    /// <summary>Any authenticated user (Student or Teacher) can read grades — no role restriction.</summary>
    [HttpGet]
    [Authorize]
    public IActionResult GetGrades()
    {
        return Ok(new[] { new { StudentName = "Sample Student", Grade = "A" } });
    }
}
