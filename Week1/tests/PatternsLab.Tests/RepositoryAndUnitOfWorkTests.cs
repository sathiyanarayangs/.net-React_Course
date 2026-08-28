using PatternsLab.Day3.Repository;
using Xunit;

namespace PatternsLab.Tests;

public class RepositoryAndUnitOfWorkTests
{
    private static IUnitOfWork CreateUow() => new UnitOfWork(new StudentRepository(), new CourseRepository());

    [Fact]
    public void Add_AssignsIncrementingIds()
    {
        var uow = CreateUow();

        var s1 = new Student { Name = "Priya", Course = "CSE" };
        var s2 = new Student { Name = "Rahul", Course = "ECE" };
        uow.Students.Add(s1);
        uow.Students.Add(s2);

        Assert.Equal(1, s1.Id);
        Assert.Equal(2, s2.Id);
    }

    [Fact]
    public void GetAll_ReturnsAllAddedEntities()
    {
        var uow = CreateUow();
        uow.Students.Add(new Student { Name = "Priya", Course = "CSE" });
        uow.Students.Add(new Student { Name = "Rahul", Course = "ECE" });

        var all = uow.Students.GetAll().ToList();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public void GetById_ReturnsMatchingEntity_OrNull()
    {
        var uow = CreateUow();
        var student = new Student { Name = "Priya", Course = "CSE" };
        uow.Students.Add(student);

        var found = uow.Students.GetById(student.Id);
        var missing = uow.Students.GetById(999);

        Assert.NotNull(found);
        Assert.Equal("Priya", found!.Name);
        Assert.Null(missing);
    }

    [Fact]
    public void Update_ReplacesStoredEntity()
    {
        var uow = CreateUow();
        var student = new Student { Name = "Priya", Course = "CSE" };
        uow.Students.Add(student);

        student.Course = "AI/ML";
        uow.Students.Update(student);

        var updated = uow.Students.GetById(student.Id);
        Assert.Equal("AI/ML", updated!.Course);
    }

    [Fact]
    public void Delete_RemovesEntity()
    {
        var uow = CreateUow();
        var student = new Student { Name = "Priya", Course = "CSE" };
        uow.Students.Add(student);

        uow.Students.Delete(student.Id);

        Assert.Null(uow.Students.GetById(student.Id));
    }

    [Fact]
    public void Save_DoesNotThrow_AndIsCallable()
    {
        var uow = CreateUow();
        uow.Students.Add(new Student { Name = "Priya", Course = "CSE" });

        var exception = Record.Exception(() => uow.Save());

        Assert.Null(exception);
    }

    [Fact]
    public void Students_And_Courses_AreIndependentRepositories()
    {
        var uow = CreateUow();
        uow.Students.Add(new Student { Name = "Priya", Course = "CSE" });
        uow.Courses.Add(new Course { Title = "Distributed Systems", Credits = 4 });

        Assert.Single(uow.Students.GetAll());
        Assert.Single(uow.Courses.GetAll());
    }
}
