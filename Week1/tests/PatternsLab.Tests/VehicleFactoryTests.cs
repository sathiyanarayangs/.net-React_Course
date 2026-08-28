using PatternsLab.Day2.Vehicles;
using Xunit;

namespace PatternsLab.Tests;

public class VehicleFactoryTests
{
    [Theory]
    [InlineData(VehicleType.Car, typeof(Car))]
    [InlineData(VehicleType.Bike, typeof(Bike))]
    [InlineData(VehicleType.Truck, typeof(Truck))]
    public void CreateVehicle_ReturnsCorrectConcreteType_ForEachKnownType(VehicleType type, Type expected)
    {
        IVehicle vehicle = VehicleFactory.CreateVehicle(type);
        Assert.IsType(expected, vehicle);
    }

    [Fact]
    public void CreateVehicle_ThrowsForUnknownType()
    {
        var unknownType = (VehicleType)999;
        Assert.Throws<ArgumentOutOfRangeException>(() => VehicleFactory.CreateVehicle(unknownType));
    }

    [Fact]
    public void CarFactory_CreateVehicle_ReturnsCar()
    {
        VehicleCreator creator = new CarFactory();
        IVehicle vehicle = creator.CreateVehicle();
        Assert.IsType<Car>(vehicle);
    }

    [Fact]
    public void BikeFactory_CreateVehicle_ReturnsBike()
    {
        VehicleCreator creator = new BikeFactory();
        IVehicle vehicle = creator.CreateVehicle();
        Assert.IsType<Bike>(vehicle);
    }
}
