using Microsoft.Extensions.Logging;
using Moq;
using StudentApi.Dtos;
using StudentApi.Models;
using StudentApi.Patterns;
using StudentApi.Repositories;
using StudentApi.Services;
using Xunit;

namespace StudentApi.Tests;

public class StudentServiceTests
{
    private readonly Mock<IRepository<Student>> _repositoryMock = new();
    private readonly IGradeStrategyFactory _gradeStrategyFactory =
        new GradeStrategyFactory(new IGradeStrategy[] { new PercentageGradeStrategy(), new GpaGradeStrategy() });
    private readonly Mock<ILogger<StudentService>> _loggerMock = new();

    private StudentService CreateService() =>
        new(_repositoryMock.Object, _gradeStrategyFactory, _loggerMock.Object);

    [Fact]
    public void GetAll_MapsEveryEntityToReadDto()
    {
        _repositoryMock.Setup(r => r.GetAll()).Returns(new[]
        {
            new Student { Id = 1, Name = "Priya", Age = 20, Email = "p@x.com", Score = 90 },
            new Student { Id = 2, Name = "Rahul", Age = 21, Email = "r@x.com", Score = 60 },
        });

        var result = CreateService().GetAll(null).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Grade); // 90 -> A under default percentage strategy
        Assert.Equal("D", result[1].Grade); // 60 -> D
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        _repositoryMock.Setup(r => r.GetById(999)).Returns((Student?)null);

        var result = CreateService().GetById(999, null);

        Assert.Null(result);
    }

    [Fact]
    public void GetById_ReturnsDto_WhenFound()
    {
        _repositoryMock.Setup(r => r.GetById(1)).Returns(new Student { Id = 1, Name = "Priya", Score = 88 });

        var result = CreateService().GetById(1, null);

        Assert.NotNull(result);
        Assert.Equal("Priya", result!.Name);
        Assert.Equal("B", result.Grade);
    }

    [Fact]
    public void GetById_UsesGpaStrategy_WhenRequested()
    {
        _repositoryMock.Setup(r => r.GetById(1)).Returns(new Student { Id = 1, Name = "Priya", Score = 50 });

        var result = CreateService().GetById(1, "gpa");

        Assert.Equal("2.00", result!.Grade);
    }

    [Fact]
    public void Create_AddsEntityViaRepository_AndReturnsMappedDto()
    {
        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Student>()))
            .Returns<Student>(s => { s.Id = 42; return s; });

        var dto = new StudentCreateDto { Name = "Zoya", Age = 19, Email = "z@x.com", Score = 95 };
        var result = CreateService().Create(dto, null);

        Assert.Equal(42, result.Id);
        Assert.Equal("Zoya", result.Name);
        Assert.Equal("A", result.Grade);
        _repositoryMock.Verify(r => r.Add(It.Is<Student>(s => s.Name == "Zoya")), Times.Once);
    }

    [Fact]
    public void Create_NeverExposesInternalNotes_OnTheReturnedDto()
    {
        _repositoryMock
            .Setup(r => r.Add(It.IsAny<Student>()))
            .Returns<Student>(s => { s.Id = 1; s.InternalNotes = "flagged for review"; return s; });

        var dto = new StudentCreateDto { Name = "Zoya", Age = 19, Email = "z@x.com", Score = 95 };
        var result = CreateService().Create(dto, null);

        // StudentReadDto has no InternalNotes property at all — this test
        // documents that guarantee structurally: there is nothing to assert
        // "is empty" on, because the shape itself cannot carry the field.
        var dtoProperties = typeof(StudentReadDto).GetProperties().Select(p => p.Name);
        Assert.DoesNotContain(nameof(Student.InternalNotes), dtoProperties);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenStudentMissing()
    {
        _repositoryMock.Setup(r => r.GetById(1)).Returns((Student?)null);

        var updated = CreateService().Update(1, new StudentCreateDto { Name = "X", Age = 20, Email = "x@x.com", Score = 50 });

        Assert.False(updated);
        _repositoryMock.Verify(r => r.Update(It.IsAny<int>(), It.IsAny<Student>()), Times.Never);
    }

    [Fact]
    public void Update_UpdatesExistingFields_WhenFound()
    {
        var existing = new Student { Id = 1, Name = "Old", Age = 20, Email = "old@x.com", Score = 40 };
        _repositoryMock.Setup(r => r.GetById(1)).Returns(existing);
        _repositoryMock.Setup(r => r.Update(1, It.IsAny<Student>())).Returns(true);

        var updated = CreateService().Update(1, new StudentCreateDto { Name = "New", Age = 21, Email = "new@x.com", Score = 80 });

        Assert.True(updated);
        _repositoryMock.Verify(r => r.Update(1, It.Is<Student>(s => s.Name == "New" && s.Score == 80)), Times.Once);
    }

    [Fact]
    public void Delete_DelegatesToRepository()
    {
        _repositoryMock.Setup(r => r.Delete(1)).Returns(true);

        var deleted = CreateService().Delete(1);

        Assert.True(deleted);
        _repositoryMock.Verify(r => r.Delete(1), Times.Once);
    }

    [Fact]
    public void Search_ReturnsMatches_CaseInsensitively()
    {
        _repositoryMock.Setup(r => r.GetAll()).Returns(new[]
        {
            new Student { Id = 1, Name = "Priya Sharma", Score = 90 },
            new Student { Id = 2, Name = "Rahul Verma", Score = 70 },
        });

        var result = CreateService().Search("priya", null).ToList();

        Assert.Single(result);
        Assert.Equal("Priya Sharma", result[0].Name);
    }

    [Fact]
    public void Search_ReturnsEmpty_WhenNoMatch()
    {
        _repositoryMock.Setup(r => r.GetAll()).Returns(new[]
        {
            new Student { Id = 1, Name = "Priya Sharma", Score = 90 },
        });

        var result = CreateService().Search("nonexistent", null).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Search_ReturnsAll_WhenNameIsNullOrEmpty()
    {
        _repositoryMock.Setup(r => r.GetAll()).Returns(new[]
        {
            new Student { Id = 1, Name = "Priya Sharma", Score = 90 },
            new Student { Id = 2, Name = "Rahul Verma", Score = 70 },
        });

        var result = CreateService().Search(null, null).ToList();

        Assert.Equal(2, result.Count);
    }
}
