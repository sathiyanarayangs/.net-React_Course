using Microsoft.AspNetCore.Mvc;
using StudentApi.Dtos;
using StudentApi.Services;

namespace StudentApi.Controllers;

/// <summary>
/// Task 2.2-2.9. Deliberately thin: every action just calls the service and
/// maps the result to a status code. No business logic, no LINQ over
/// entities, no direct repository access — that all lives in StudentService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    /// <summary>GET api/students?gradeStrategy=percentage|gpa</summary>
    [HttpGet]
    public ActionResult<IEnumerable<StudentReadDto>> GetAll([FromQuery] string? gradeStrategy)
    {
        return Ok(_studentService.GetAll(gradeStrategy));
    }

    /// <summary>GET api/students/5?gradeStrategy=percentage|gpa -> 200 or 404</summary>
    [HttpGet("{id:int}")]
    public ActionResult<StudentReadDto> GetById(int id, [FromQuery] string? gradeStrategy)
    {
        var student = _studentService.GetById(id, gradeStrategy);
        if (student is null)
        {
            _logger.LogWarning("Student {StudentId} not found.", id);
            return NotFound();
        }
        return Ok(student);
    }

    /// <summary>GET api/students/search?name=...&amp;gradeStrategy=... -> 200 with matches or empty list</summary>
    [HttpGet("search")]
    public ActionResult<IEnumerable<StudentReadDto>> Search([FromQuery] string? name, [FromQuery] string? gradeStrategy)
    {
        return Ok(_studentService.Search(name, gradeStrategy));
    }

    /// <summary>POST api/students?gradeStrategy=... -> 201 Created + Location header, or 400 (automatic via [ApiController])</summary>
    [HttpPost]
    public ActionResult<StudentReadDto> Create([FromBody] StudentCreateDto dto, [FromQuery] string? gradeStrategy)
    {
        var created = _studentService.Create(dto, gradeStrategy);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT api/students/5 -> 204 or 404 (400 automatic for invalid body)</summary>
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StudentCreateDto dto)
    {
        var updated = _studentService.Update(id, dto);
        if (!updated)
        {
            _logger.LogWarning("Attempted to update missing student {StudentId}.", id);
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>DELETE api/students/5 -> 204 or 404</summary>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleted = _studentService.Delete(id);
        if (!deleted)
        {
            _logger.LogWarning("Attempted to delete missing student {StudentId}.", id);
            return NotFound();
        }
        return NoContent();
    }
}
