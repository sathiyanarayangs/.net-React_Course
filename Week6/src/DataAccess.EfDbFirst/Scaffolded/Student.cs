using System;
using System.Collections.Generic;

namespace DataAccess.EfDbFirst.Scaffolded;

public partial class Student
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Age { get; set; }

    public string Email { get; set; } = null!;

    public DateTime? EnrolledOn { get; set; }
}
