namespace PatternsLab.Day2.Vehicles;

public interface IVehicle
{
    string Describe();
}

public class Car : IVehicle
{
    public string Describe() => "Car: 4 wheels, built for the road.";
}

public class Bike : IVehicle
{
    public string Describe() => "Bike: 2 wheels, cheap to run.";
}

public class Truck : IVehicle
{
    public string Describe() => "Truck: heavy hauling, many wheels.";
}

public enum VehicleType
{
    Car,
    Bike,
    Truck
}

/// <summary>
/// Task 1.5 (part 1) - simple Factory: one static/instance method that switches
/// on a type parameter and returns the right concrete IVehicle. The caller
/// never writes `new Car()` directly.
/// </summary>
public static class VehicleFactory
{
    public static IVehicle CreateVehicle(VehicleType type) => type switch
    {
        VehicleType.Car => new Car(),
        VehicleType.Bike => new Bike(),
        VehicleType.Truck => new Truck(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown vehicle type.")
    };
}

/// <summary>
/// Task 1.5 (part 2) - Factory Method: instead of one factory branching on a
/// type code, each concrete creator overrides CreateVehicle() to decide what
/// to instantiate. New vehicle types only require a new creator subclass.
/// </summary>
public abstract class VehicleCreator
{
    public abstract IVehicle CreateVehicle();

    // Template-ish convenience: creators can also do setup common to all vehicles.
    public IVehicle CreateAndPrepare()
    {
        var vehicle = CreateVehicle();
        Console.WriteLine($"[{GetType().Name}] prepared -> {vehicle.Describe()}");
        return vehicle;
    }
}

public class CarFactory : VehicleCreator
{
    public override IVehicle CreateVehicle() => new Car();
}

public class BikeFactory : VehicleCreator
{
    public override IVehicle CreateVehicle() => new Bike();
}

public static class VehicleFactoryDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Task 1.5: Factory & Factory Method ---");

        // Simple factory
        foreach (var type in Enum.GetValues<VehicleType>())
        {
            IVehicle vehicle = VehicleFactory.CreateVehicle(type);
            Console.WriteLine(vehicle.Describe());
        }

        // Factory Method
        VehicleCreator[] creators = { new CarFactory(), new BikeFactory() };
        foreach (var creator in creators)
        {
            creator.CreateAndPrepare();
        }
    }
}
