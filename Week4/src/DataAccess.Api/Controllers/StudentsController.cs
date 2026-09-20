using DataAccess.Core.Models;
using DataAccess.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataAccess.Api.Controllers;

/// <summary>
/// Task 4.11's proof point lives here: this controller is identical no
/// matter which DataLayer setting is active. Run the exact same Postman
/// requests against api/students with DataLayer=AdoNet, then
/// EfCodeFirst, then EfDbFirst (restarting the app between each) — the
/// responses should be indistinguishable.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetAll() =>
        Ok(await _studentService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Create([FromBody] Student student)
    {
        var created = await _studentService.CreateAsync(student);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Student student)
    {
        bool updated = await _studentService.UpdateAsync(id, student);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _studentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
