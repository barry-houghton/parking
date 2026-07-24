using CarPark.Core;
using CarPark.Core.Exceptions;
using CarPark.Core.Persistence;
using CarPark.Core.Services;
using CarPark.UnitTests.Mocks;
using Microsoft.EntityFrameworkCore;
using ParCark.Api.Models;
using Shouldly;
using System.Runtime.CompilerServices;

namespace CarPark.UnitTests
{
    public class CarParkCheckInTests
    {
        private readonly CarParkDbContext _dbContext;

        public CarParkCheckInTests()
        {
            var options = new DbContextOptionsBuilder<CarParkDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CarParkDbContext(options);
        }

        [Fact]
        public async Task ShouldIncreaseOccupiedSpacesWhenCarChecksIn()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);
            var initialOccupiedSpaces = (await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken)).OccupiedSpaces;

            // Act
            await carPark.CheckIn("ABC123", VehicleType.Small, TestContext.Current.CancellationToken);
            var (AvailableSpaces, OccupiedSpaces) = await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken);

            // Assert
            OccupiedSpaces.ShouldBe(initialOccupiedSpaces + 1);
            AvailableSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES - OccupiedSpaces);
        }

        [Fact]
        public async Task ShouldThrowExceptionWhenCarParkIsFull()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);
            foreach (var i in Enumerable.Range(1, Configuration.TOTAL_PARKING_SPACES))
            {
                await carPark.CheckIn($"ABC{i:000}", VehicleType.Small, TestContext.Current.CancellationToken);
            }

            // Act
            var exception = Should.Throw<NoAvailableParkingSpacesException>(() => carPark.CheckIn("XYZ999", VehicleType.Small, TestContext.Current.CancellationToken));
            
            // Assert
            exception.Message.ShouldBe("No available parking spaces.");
        }

        [Fact]
        public async Task ShouldThrowExceptionWhenVehicleIsAlreadyParked()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);
            var vehicle = new Vehicle("ABC123", VehicleType.Small);
            await carPark.CheckIn(vehicle.VehicleReg, vehicle.Type, TestContext.Current.CancellationToken);

            // Act
            var exception = Should.Throw<VehicleAlreadyParkedException>(() => carPark.CheckIn(vehicle.VehicleReg, vehicle.Type, TestContext.Current.CancellationToken));

            // Assert
            exception.Message.ShouldBe($"Vehicle with registration {vehicle.VehicleReg} is already parked.");
        }

        [Theory]
        [InlineData("ABC123", 2)]
        [InlineData("XYZ789", 7)]
        public async Task ShouldReturnCorrectCheckInDetails(string vehicleReg, int numberOfVehiclesAlreadyParked)
        {
            // Arrange
            var mockDateTimeHelper = new MockDateTimeHelper();
            var carPark = new CarParkService(_dbContext, mockDateTimeHelper.Object);
            var vehicle = new Vehicle(vehicleReg, VehicleType.Large);

            for (int i = 1; i <= numberOfVehiclesAlreadyParked; i++)
            {
                await carPark.CheckIn($"ABC{i:000}", VehicleType.Small, TestContext.Current.CancellationToken);
            }

            // Act
            var (VehicleReg, SpaceNumber, CheckInTime) = await carPark.CheckIn(vehicle.VehicleReg, vehicle.Type, TestContext.Current.CancellationToken);
            
            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            SpaceNumber.ShouldBe(numberOfVehiclesAlreadyParked + 1);
            CheckInTime.ShouldBe(mockDateTimeHelper.Object.GetUtcNow(), TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData("ABC123", 5, 3)]
        [InlineData("XYZ789", 7, 5)]
        public async Task ShouldAllocateNextAvailableParkingSpace(string vehicleReg, int numberOfVehiclesAlreadyParked, int checkOutVehicleFromSpace)
        {
            // Arrange
            var mockDateTimeHelper = new MockDateTimeHelper();
            var carPark = new CarParkService(_dbContext, mockDateTimeHelper.Object);
            var vehicle = new Vehicle(vehicleReg, VehicleType.Large);

            for (int i = 1; i <= numberOfVehiclesAlreadyParked; i++)
            {
                await carPark.CheckIn($"ABC{i:000}", VehicleType.Small, TestContext.Current.CancellationToken);
            }

            // Check out a vehicle from a specific space
            await carPark.CheckOut($"ABC{checkOutVehicleFromSpace:000}", TestContext.Current.CancellationToken);

            // Act
            var (VehicleReg, SpaceNumber, CheckInTime) = await carPark.CheckIn(vehicle.VehicleReg, vehicle.Type, TestContext.Current.CancellationToken);

            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            SpaceNumber.ShouldBe(checkOutVehicleFromSpace);
            CheckInTime.ShouldBe(mockDateTimeHelper.Object.GetUtcNow(), TimeSpan.FromSeconds(1));
        }
    }
}
