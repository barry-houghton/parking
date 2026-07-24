namespace CarPark.Core.Services
{
    public interface ICarParkService
    {
        (string VehicleReg, int SpaceNumber, DateTime CheckInTime) CheckIn(string vehicleReg, int vehicleType);
        (string VehicleReg, decimal parkingCharge, DateTime CheckInTime, DateTime CheckOutTime) CheckOut(string vehicleReg);
        (int AvailableSpaces, int OccupiedSpaces) GetAvailableSpaces();
    }
}