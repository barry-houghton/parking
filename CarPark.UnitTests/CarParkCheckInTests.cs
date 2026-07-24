using CarPark.Core;
using CarPark.Core.Exceptions;
using CarPark.Core.Services;
using CarPark.UnitTests.Mocks;
using ParCark.Api.Models;
using Shouldly;

namespace CarPark.UnitTests
{
    public class CarParkCheckInTests
    {
        [Fact]
        public void ShouldIncreaseOccupiedSpacesWhenCarChecksIn()
        {
            // Arrange
            var carPark = new CarParkService(new MockDateTimeHelper().Object);
            var initialOccupiedSpaces = carPark.GetAvailableSpaces().OccupiedSpaces;

            // Act
            carPark.CheckIn("ABC123", VehicleType.Small);
            var (AvailableSpaces, OccupiedSpaces) = carPark.GetAvailableSpaces();

            // Assert
            OccupiedSpaces.ShouldBe(initialOccupiedSpaces + 1);
            AvailableSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES - OccupiedSpaces);
        }

        [Fact]
        public void ShouldThrowExceptionWhenCarParkIsFull()
        {
            // Arrange
            var carPark = new CarParkService(new MockDateTimeHelper().Object);
            foreach (var i in Enumerable.Range(1, Configuration.TOTAL_PARKING_SPACES))
            {
                carPark.CheckIn($"ABC{i:000}", VehicleType.Small);
            }

            // Act
            var exception = Should.Throw<NoAvailableParkingSpacesException>(() => carPark.CheckIn("XYZ999", VehicleType.Small));
            
            // Assert
            exception.Message.ShouldBe("No available parking spaces.");
        }

        [Fact]
        public void ShouldThrowExceptionWhenVehicleIsAlreadyParked()
        {
            // Arrange
            var carPark = new CarParkService(new MockDateTimeHelper().Object);
            var vehicle = new Vehicle("ABC123", VehicleType.Small);
            carPark.CheckIn(vehicle.VehicleReg, vehicle.Type);

            // Act
            var exception = Should.Throw<VehicleAlreadyParkedException>(() => carPark.CheckIn(vehicle.VehicleReg, vehicle.Type));

            // Assert
            exception.Message.ShouldBe($"Vehicle with registration {vehicle.VehicleReg} is already parked.");
        }

        [Theory]
        [InlineData("ABC123", 2)]
        [InlineData("XYZ789", 7)]
        public void ShouldReturnCorrectCheckInDetails(string vehicleReg, int numberOfVehiclesAlreadyParked)
        {
            // Arrange
            var mockDateTimeHelper = new MockDateTimeHelper();
            var carPark = new CarParkService(mockDateTimeHelper.Object);
            var vehicle = new Vehicle(vehicleReg, VehicleType.Large);

            for (int i = 1; i <= numberOfVehiclesAlreadyParked; i++)
            {
                carPark.CheckIn($"ABC{i:000}", VehicleType.Small);
            }

            // Act
            var (VehicleReg, SpaceNumber, CheckInTime) = carPark.CheckIn(vehicle.VehicleReg, vehicle.Type);
            
            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            SpaceNumber.ShouldBe(numberOfVehiclesAlreadyParked + 1);
            CheckInTime.ShouldBeLessThanOrEqualTo(mockDateTimeHelper.Object.GetUtcNow());
        }

        [Theory]
        [InlineData("ABC123", 5, 3)]
        [InlineData("XYZ789", 7, 5)]
        public void ShouldAllocateNextAvailableParkingSpace(string vehicleReg, int numberOfVehiclesAlreadyParked, int checkOutVehicleFromSpace)
        {
            // Arrange
            var mockDateTimeHelper = new MockDateTimeHelper();
            var carPark = new CarParkService(mockDateTimeHelper.Object);
            var vehicle = new Vehicle(vehicleReg, VehicleType.Large);

            for (int i = 1; i <= numberOfVehiclesAlreadyParked; i++)
            {
                carPark.CheckIn($"ABC{i:000}", VehicleType.Small);
            }

            // Check out a vehicle from a specific space
            carPark.CheckOut($"ABC{checkOutVehicleFromSpace:000}");

            // Act
            var (VehicleReg, SpaceNumber, CheckInTime) = carPark.CheckIn(vehicle.VehicleReg, vehicle.Type);

            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            SpaceNumber.ShouldBe(checkOutVehicleFromSpace);
            CheckInTime.ShouldBeLessThanOrEqualTo(mockDateTimeHelper.Object.GetUtcNow());
        }
    }
}
