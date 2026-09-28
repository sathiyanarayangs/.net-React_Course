using DataAccess.Core.Models;
using DataAccess.Core.Repositories;
using DataAccess.Core.Services;
using Moq;
using Xunit;

namespace DataAccess.Tests;

/// <summary>
/// The service is tested ENTIRELY against a mocked IRepository&lt;Student&gt;
/// — never against AdoNetStudentRepository, EfStudentRepository, or
/// EfDbFirstStudentRepository directly. That's deliberate: these tests
/// prove the service's logic doesn't depend on, or need to know about,
/// which concrete data layer Task 4.11 has wired up at runtime.
/// </summary>
public class StudentServiceTests
{
    private readonly Mock<IRepository<Student>> _repositoryMock = new();
    private StudentService CreateService() => new(_repositoryMock.Object);

    [Fact]
    public async Task GetAllAsync_ReturnsWhatRepositoryReturns()
    {
        var students = new[] { new Student { Id = 1, Name = "Priya" }, new Student { Id = 2, Name = "Rahul" } };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(students);

        var result = await CreateService().GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenRepositoryReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Student?)null);

        var result = await CreateService().GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsStudent_WhenFound()
    {
        var student = new Student { Id = 1, Name = "Priya" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);

        var result = await CreateService().GetByIdAsync(1);

        Assert.Equal("Priya", result!.Name);
    }

    [Fact]
    public async Task CreateAsync_DelegatesToRepositoryAdd()
    {
        var student = new Student { Name = "Zoya" };
        _repositoryMock.Setup(r => r.AddAsync(student)).ReturnsAsync(student);

        var result = await CreateService().CreateAsync(student);

        Assert.Same(student, result);
        _repositoryMock.Verify(r => r.AddAsync(student), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenRepositoryReturnsFalse()
    {
        _repositoryMock.Setup(r => r.UpdateAsync(999, It.IsAny<Student>())).ReturnsAsync(false);

        bool result = await CreateService().UpdateAsync(999, new Student());

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenRepositoryReturnsTrue()
    {
        _repositoryMock.Setup(r => r.UpdateAsync(1, It.IsAny<Student>())).ReturnsAsync(true);

        bool result = await CreateService().UpdateAsync(1, new Student());

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepositoryDelete()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        bool result = await CreateService().DeleteAsync(1);

        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task Service_BehavesIdentically_RegardlessOfWhichRepositoryImplementationIsMocked()
    {
        // Two separately-mocked IRepository<Student> instances, standing in
        // for "could be ADO.NET, could be EF Code First, could be EF DB
        // First" — the service's output only depends on what the interface
        // returns, never on which concrete type implements it.
        var mockA = new Mock<IRepository<Student>>();
        var mockB = new Mock<IRepository<Student>>();
        var student = new Student { Id = 5, Name = "Same Student" };
        mockA.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(student);
        mockB.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(student);

        var resultA = await new StudentService(mockA.Object).GetByIdAsync(5);
        var resultB = await new StudentService(mockB.Object).GetByIdAsync(5);

        Assert.Equal(resultA!.Name, resultB!.Name);
    }
}
