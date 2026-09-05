using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using StudentApi.Controllers;
using StudentApi.Dtos;
using StudentApi.Patterns;
using StudentApi.Repositories;
using StudentApi.Services;
using Xunit;

namespace StudentApi.Tests;

/// <summary>
/// Task's testing focus: "a small set of controller tests asserting the
/// right status code per outcome (200/201/204/404) using an in-memory
/// service." Each test gets a fresh StudentRepository/StudentService pair
/// so tests never leak state into each other.
/// </summary>
public class StudentsControllerTests
{
    private static StudentsController CreateController()
    {
        var repository = new StudentRepository();
        var gradeStrategyFactory = new GradeStrategyFactory(
            new IGradeStrategy[] { new PercentageGradeStrategy(), new GpaGradeStrategy() });
        var service = new StudentService(repository, gradeStrategyFactory, NullLogger<StudentService>.Instance);
        return new StudentsController(service, NullLogger<StudentsController>.Instance);
    }

    private static StudentCreateDto SampleDto(string name = "Priya", decimal score = 88) =>
        new() { Name = name, Age = 20, Email = "priya@example.com", Score = score };

    [Fact]
    public void GetAll_ReturnsOk_WithEmptyList_WhenNoStudents()
    {
        var controller = CreateController();

        var result = controller.GetAll(null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var students = Assert.IsAssignableFrom<IEnumerable<StudentReadDto>>(okResult.Value);
        Assert.Empty(students);
    }

    [Fact]
    public void Create_Returns201_WithLocationHeaderPointingAtGetById()
    {
        var controller = CreateController();

        var result = controller.Create(SampleDto(), null);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(StudentsController.GetById), created.ActionName);
        var dto = Assert.IsType<StudentReadDto>(created.Value);
        Assert.Equal("Priya", dto.Name);
        Assert.Equal(dto.Id, created.RouteValues!["id"]);
    }

    [Fact]
    public void GetById_Returns200_WhenStudentExists()
    {
        var controller = CreateController();
        var created = ((StudentReadDto)((CreatedAtActionResult)controller.Create(SampleDto(), null).Result!).Value!);

        var result = controller.GetById(created.Id, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<StudentReadDto>(ok.Value);
        Assert.Equal(created.Id, dto.Id);
    }

    [Fact]
    public void GetById_Returns404_WhenStudentMissing()
    {
        var controller = CreateController();

        var result = controller.GetById(999, null);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void Update_Returns204_WhenStudentExists()
    {
        var controller = CreateController();
        var created = ((StudentReadDto)((CreatedAtActionResult)controller.Create(SampleDto(), null).Result!).Value!);

        var result = controller.Update(created.Id, SampleDto("Priya Updated", 91));

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_Returns404_WhenStudentMissing()
    {
        var controller = CreateController();

        var result = controller.Update(999, SampleDto());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Delete_Returns204_WhenStudentExists()
    {
        var controller = CreateController();
        var created = ((StudentReadDto)((CreatedAtActionResult)controller.Create(SampleDto(), null).Result!).Value!);

        var result = controller.Delete(created.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_Returns404_WhenStudentMissing()
    {
        var controller = CreateController();

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Search_Returns200_WithMatches_OnHit()
    {
        var controller = CreateController();
        controller.Create(SampleDto("Priya Sharma"), null);
        controller.Create(SampleDto("Rahul Verma"), null);

        var result = controller.Search("priya", null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var matches = Assert.IsAssignableFrom<IEnumerable<StudentReadDto>>(ok.Value).ToList();
        Assert.Single(matches);
    }

    [Fact]
    public void Search_Returns200_WithEmptyList_OnMiss()
    {
        var controller = CreateController();
        controller.Create(SampleDto("Priya Sharma"), null);

        var result = controller.Search("nonexistent", null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var matches = Assert.IsAssignableFrom<IEnumerable<StudentReadDto>>(ok.Value).ToList();
        Assert.Empty(matches);
    }

    [Fact]
    public void Search_Returns200_WithAllStudents_OnEmptyQuery()
    {
        var controller = CreateController();
        controller.Create(SampleDto("Priya Sharma"), null);
        controller.Create(SampleDto("Rahul Verma"), null);

        var result = controller.Search(null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var matches = Assert.IsAssignableFrom<IEnumerable<StudentReadDto>>(ok.Value).ToList();
        Assert.Equal(2, matches.Count);
    }
}
