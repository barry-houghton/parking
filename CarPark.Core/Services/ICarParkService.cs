using ParCark.Api.Models;

namespace CarPark.Core.Services
{
    public interface ICarParkService
    {
        (string VehicleReg, int SpaceNumber, DateTime CheckInTime) CheckIn(Vehicle vehicle);
        (string VehicleReg, decimal parkingCharge, DateTime CheckInTime, DateTime CheckOutTime) CheckOut(string vehicleReg);
        (int AvailableSpaces, int OccupiedSpaces) GetAvailableSpaces();
    }
}