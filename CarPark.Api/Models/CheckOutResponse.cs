namespace CarPark.Api.Models
{
    public record CheckOutResponse(string VehicleReg, decimal ParkingCharge, DateTime CheckInTime, DateTime CheckOutTime);
}
