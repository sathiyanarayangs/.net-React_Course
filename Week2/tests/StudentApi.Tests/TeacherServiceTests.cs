using Microsoft.Extensions.Logging.Abstractions;
using StudentApi.Dtos;
using StudentApi.Repositories;
using StudentApi.Services;
using Xunit;

namespace StudentApi.Tests;

public class TeacherServiceTests
{
    private static TeacherService CreateService() =>
        new(new TeacherRepository(), NullLogger<TeacherService>.Instance);

    [Fact]
    public void Create_ThenGetById_RoundTrips()
    {
        var service = CreateService();

        var created = service.Create(new TeacherCreateDto { Name = "Mr. Iyer", Subject = "Physics", Email = "iyer@example.com" });
        var fetched = service.GetById(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal("Mr. Iyer", fetched!.Name);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenMissing()
    {
        var service = CreateService();

        Assert.Null(service.GetById(999));
    }

    [Fact]
    public void Update_ReturnsFalse_WhenMissing()
    {
        var service = CreateService();

        var updated = service.Update(999, new TeacherCreateDto { Name = "X", Subject = "Y", Email = "x@x.com" });

        Assert.False(updated);
    }

    [Fact]
    public void Delete_ReturnsTrue_WhenTeacherExists()
    {
        var service = CreateService();
        var created = service.Create(new TeacherCreateDto { Name = "Mr. Iyer", Subject = "Physics", Email = "iyer@example.com" });

        Assert.True(service.Delete(created.Id));
        Assert.Null(service.GetById(created.Id));
    }
}
