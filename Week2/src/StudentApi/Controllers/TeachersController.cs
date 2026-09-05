using Microsoft.AspNetCore.Mvc;
using StudentApi.Dtos;
using StudentApi.Services;

namespace StudentApi.Controllers;

/// <summary>Task 2.10 (stretch) - same thin-controller shape as StudentsController.</summary>
[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;

    public TeachersController(ITeacherService teacherService)
    {
        _teacherService = teacherService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TeacherReadDto>> GetAll() => Ok(_teacherService.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<TeacherReadDto> GetById(int id)
    {
        var teacher = _teacherService.GetById(id);
        return teacher is null ? NotFound() : Ok(teacher);
    }

    [HttpPost]
    public ActionResult<TeacherReadDto> Create([FromBody] TeacherCreateDto dto)
    {
        var created = _teacherService.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] TeacherCreateDto dto)
    {
        return _teacherService.Update(id, dto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return _teacherService.Delete(id) ? NoContent() : NotFound();
    }
}
