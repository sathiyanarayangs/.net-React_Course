using StudentApi.Dtos;
using StudentApi.Models;
using Xunit;

namespace StudentApi.Tests;

/// <summary>
/// Task 2.7's specific proof point, called out separately in the brief as
/// "a cheap, valuable test": StudentReadDto must never be able to carry
/// Student.InternalNotes, no matter what the entity holds.
/// </summary>
public class DtoMappingTests
{
    [Fact]
    public void StudentReadDto_HasNoInternalNotesProperty()
    {
        var readDtoProperties = typeof(StudentReadDto).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain(nameof(Student.InternalNotes), readDtoProperties);
    }

    [Fact]
    public void StudentCreateDto_HasNoIdProperty_ServerAssignsIt()
    {
        var createDtoProperties = typeof(StudentCreateDto).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain(nameof(Student.Id), createDtoProperties);
    }

    [Fact]
    public void StudentCreateDto_HasNoInternalNotesProperty_ClientsCannotSetIt()
    {
        var createDtoProperties = typeof(StudentCreateDto).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain(nameof(Student.InternalNotes), createDtoProperties);
    }
}
