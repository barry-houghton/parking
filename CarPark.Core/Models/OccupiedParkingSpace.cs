namespace ParCark.Api.Models
{
    public class OccupiedParkingSpace
    {
        public OccupiedParkingSpace(int id, string vehicleReg, VehicleType vehicleType, DateTime checkInTime)
        {
            Id = id;
            VehicleReg = vehicleReg;
            VehicleType = vehicleType;
            CheckInTime = checkInTime;
        }

        private OccupiedParkingSpace() { } // EF

        public int Id { get; private set; }
        public string VehicleReg { get; private set; } = null!;
        public VehicleType VehicleType { get; private set; } = null!;
        public DateTime CheckInTime { get; private set; }
    }
}
