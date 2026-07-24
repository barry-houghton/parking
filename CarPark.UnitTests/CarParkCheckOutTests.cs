using CarPark.Core;
using CarPark.Core.Exceptions;
using CarPark.Core.Services;
using CarPark.UnitTests.Mocks;
using ParCark.Api.Models;
using Shouldly;

namespace CarPark.UnitTests
{
    public class CarParkCheckOutTests
    {
        [Fact]
        public void ShouldDecreaseOccupiedSpacesWhenCarChecksOut()
        {
            // Arrange
            var carPark = new CarParkService(new MockDateTimeHelper().Object);
            var vehicle = new Vehicle("ABC123", VehicleType.Small);
            carPark.CheckIn(vehicle.VehicleReg, vehicle.Type);
            var initialOccupiedSpaces = carPark.GetAvailableSpaces().OccupiedSpaces;
            
            // Act
            carPark.CheckOut(vehicle.VehicleReg);
            var (AvailableSpaces, OccupiedSpaces) = carPark.GetAvailableSpaces();

            // Assert
            OccupiedSpaces.ShouldBe(initialOccupiedSpaces - 1);
            AvailableSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES - OccupiedSpaces);
        }

        [Fact]
        public void ShouldThrowExceptionWhenVehicleIsNotCheckedIn()
        {
            // Arrange
            var carPark = new CarParkService(new MockDateTimeHelper().Object);
            var vehicleReg = "XYZ999";
         
            // Act
            var exception = Should.Throw<VehicleNotCheckedInException>(() => carPark.CheckOut(vehicleReg));
            
            // Assert
            exception.Message.ShouldBe($"Vehicle with registration {vehicleReg} is not checked in.");
        }

        [Theory]
        [InlineData(1, 13, 3.3)]
        [InlineData(2, 45, 18.0)]
        [InlineData(3, 87, 51.8)]
        public void ShouldCalculateCorrectParkingChargeForVehicleType(int vehicleType, int minutesParked, decimal expectedCharge)
        {
            // Arrange
            var mockDateTimeHelper = new MockDateTimeHelper();
            var carPark = new CarParkService(mockDateTimeHelper.Object);
            var vehicle = new Vehicle("ABC123", VehicleType.FromValue(vehicleType));
            carPark.CheckIn(vehicle.VehicleReg, vehicle.Type);

            mockDateTimeHelper.AdvanceTimeBy(TimeSpan.FromMinutes(minutesParked)); // Simulate the specified number of minutes of parking

            // Act
            var (VehicleReg, ParkingCharge, CheckInTime, CheckOutTime) = carPark.CheckOut(vehicle.VehicleReg);

            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            ParkingCharge.ShouldBe(expectedCharge);
            CheckInTime.ShouldBeLessThan(CheckOutTime);
        }
    }
}
