namespace PatternsLab.Day5;

/// <summary>
/// Task 1.13 (part 1) - Interface vs Abstract class.
///
/// | Aspect        | Interface                                   | Abstract class                                  |
/// |---------------|----------------------------------------------|--------------------------------------------------|
/// | Inheritance   | A type can implement many interfaces          | A type can inherit only one abstract class        |
/// | State         | No instance fields (only members/defaults)    | Can hold fields and shared implementation state   |
/// | Constructors  | None                                          | Can define constructors run by derived types       |
/// | Versioning    | Adding a member breaks all implementers        | Can add a virtual member with a default body      |
///                   unless given a default implementation            without breaking existing subclasses
///
/// Scenario favouring an interface: unrelated types that all need one
/// capability. IComparable&lt;T&gt; is implemented by Employee, DateTime, string,
/// etc. — none of them share a base class, they just share a contract.
///
/// Scenario favouring an abstract class: a family of types that share both a
/// contract AND real implementation/state. Below, ShapeBase stores a shared
/// `Name` and provides a common `Describe()` built on top of an abstract
/// `Area()` that each shape must supply.
/// </summary>
public interface IResizable
{
    void Resize(double factor);
}

public abstract class ShapeBase
{
    public string Name { get; }

    protected ShapeBase(string name)
    {
        Name = name; // only an abstract class can run constructor logic like this
    }

    public abstract double Area();

    // Shared implementation every subclass gets for free.
    public string Describe() => $"{Name} has area {Area():F2}";
}

public class Circle : ShapeBase, IResizable
{
    private double _radius;
    public Circle(double radius) : base("Circle") => _radius = radius;

    public override double Area() => Math.PI * _radius * _radius;

    public void Resize(double factor) => _radius *= factor;
}

public class Square : ShapeBase, IResizable
{
    private double _side;
    public Square(double side) : base("Square") => _side = side;

    public override double Area() => _side * _side;

    public void Resize(double factor) => _side *= factor;
}

public static class InterfaceVsAbstractDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.13: Interface vs Abstract class ---");

        ShapeBase[] shapes = { new Circle(3), new Square(4) };
        foreach (var shape in shapes)
        {
            Console.WriteLine(shape.Describe()); // shared abstract-class implementation

            if (shape is IResizable resizable)
            {
                resizable.Resize(2.0); // capability from the interface
                Console.WriteLine($"After resizing: {shape.Describe()}");
            }
        }
    }
}
