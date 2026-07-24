using CarPark.Core;
using CarPark.Core.Exceptions;
using CarPark.Core.Persistence;
using CarPark.Core.Services;
using CarPark.UnitTests.Mocks;
using Microsoft.EntityFrameworkCore;
using ParCark.Api.Models;
using Shouldly;

namespace CarPark.UnitTests
{
    public class CarParkCheckOutTests
    {
        private readonly CarParkDbContext _dbContext;

        public CarParkCheckOutTests()
        {
            var options = new DbContextOptionsBuilder<CarParkDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CarParkDbContext(options);
        }

        [Fact]
        public async Task ShouldDecreaseOccupiedSpacesWhenCarChecksOut()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);
            var vehicle = new Vehicle("ABC123", VehicleType.Small);
            await carPark.CheckIn(vehicle.VehicleReg, vehicle.Type, TestContext.Current.CancellationToken);
            var initialOccupiedSpaces = (await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken)).OccupiedSpaces;

            // Act
            await carPark.CheckOut(vehicle.VehicleReg, TestContext.Current.CancellationToken);
            var (AvailableSpaces, OccupiedSpaces) = await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken);

            // Assert
            OccupiedSpaces.ShouldBe(initialOccupiedSpaces - 1);
            AvailableSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES - OccupiedSpaces);
        }

        [Fact]
        public void ShouldThrowExceptionWhenVehicleIsNotCheckedIn()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);
            var vehicleReg = "XYZ999";

            // Act
            var exception = Should.Throw<VehicleNotCheckedInException>(() => carPark.CheckOut(vehicleReg, TestContext.Current.CancellationToken));

            // Assert
            exception.Message.ShouldBe($"Vehicle with registration {vehicleReg} is not checked in.");
        }

        [Theory]
        [InlineData(1, 13, 3.3)]
        [InlineData(2, 45, 18.0)]
        [InlineData(3, 87, 51.8)]
        public async Task ShouldCalculateCorrectParkingChargeForVehicleType(int vehicleType, int minutesParked, decimal expectedCharge)
        {
            // Arrange
            var mockDateTimeHelper = new MockDateTimeHelper();
            var carPark = new CarParkService(_dbContext, mockDateTimeHelper.Object);
            var vehicle = new Vehicle("ABC123", VehicleType.FromValue(vehicleType));
            await carPark.CheckIn(vehicle.VehicleReg, vehicle.Type, TestContext.Current.CancellationToken);

            mockDateTimeHelper.AdvanceTimeBy(TimeSpan.FromMinutes(minutesParked)); // Simulate the specified number of minutes of parking

            // Act
            var (VehicleReg, ParkingCharge, CheckInTime, CheckOutTime) = await carPark.CheckOut(vehicle.VehicleReg, TestContext.Current.CancellationToken);

            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            ParkingCharge.ShouldBe(expectedCharge);
            CheckInTime.ShouldBeLessThan(CheckOutTime);
        }
    }
}
