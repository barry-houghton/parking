using CarPark.Core;
using CarPark.Core.Exceptions;
using CarPark.Core.Services;
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
            var carPark = new CarParkService();
            var vehicle = new Vehicle("ABC123", VehicleType.Small);
            carPark.CheckIn(vehicle);
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
            var carPark = new CarParkService();
            var vehicleReg = "XYZ999";
         
            // Act
            var exception = Should.Throw<VehicleNotCheckedInException>(() => carPark.CheckOut(vehicleReg));
            
            // Assert
            exception.Message.ShouldBe($"Vehicle with registration {vehicleReg} is not checked in.");
        }

        [Fact]
        public void ShouldCalculateCorrectParkingChargeForSmallVehicle()
        {
            // Arrange
            var carPark = new CarParkService();
            var vehicle = new Vehicle("ABC123", VehicleType.Small);
            carPark.CheckIn(vehicle);

            // Act
            var (VehicleReg, ParkingCharge, CheckInTime, CheckOutTime) = carPark.CheckOut(vehicle.VehicleReg);

            // Assert
            VehicleReg.ShouldBe(vehicle.VehicleReg);
            ParkingCharge.ShouldBeGreaterThan(0); // Assuming the charge is greater than 0 for a small vehicle
            CheckInTime.ShouldBeLessThan(CheckOutTime);
        }
    }
}
