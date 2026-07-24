namespace CarPark.Api.Models
{
    public record CheckInResponse(string VehicleReg, int SpaceNumber, DateTime CheckInTime);
}
