namespace CarPark.Core.Services
{
    public interface ICarParkService
    {
        Task<(string VehicleReg, int SpaceNumber, DateTime CheckInTime)> CheckIn(string vehicleReg, int vehicleType, CancellationToken ct);
        Task<(string VehicleReg, decimal parkingCharge, DateTime CheckInTime, DateTime CheckOutTime)> CheckOut(string vehicleReg, CancellationToken ct);
        Task<(int AvailableSpaces, int OccupiedSpaces)> GetAvailableSpaces(CancellationToken ct);
    }
}