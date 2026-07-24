using CarPark.Core.Exceptions;
using ParCark.Api.Models;

namespace CarPark.Core.Services
{
    public class CarParkService
    {
        private readonly OccupiedParkingSpace?[] _parkingSpaces = new OccupiedParkingSpace?[Configuration.TOTAL_PARKING_SPACES];

        public (int AvailableSpaces, int OccupiedSpaces) GetAvailableSpaces()
        {
            return (
                AvailableSpaces: Configuration.TOTAL_PARKING_SPACES - _parkingSpaces.Count(s => s != null),
                OccupiedSpaces: _parkingSpaces.Count(s => s != null)
            );
        }

        public (string VehicleReg, int SpaceNumber, DateTime CheckInTime) CheckIn(Vehicle vehicle)
        {
            // check if vehicle is already checked in
            var existingSpaceNumber = Array.FindIndex(_parkingSpaces, x => x?.Vehicle.VehicleReg == vehicle.VehicleReg);
            if (existingSpaceNumber != -1)
            {
                throw new VehicleAlreadyParkedException($"Vehicle with registration {vehicle.VehicleReg} is already parked.");
            }

            // find next available parking space
            var spaceNumber = FindNextAvailableParkingSpace();

            // add vehicle to parking space
            _parkingSpaces[spaceNumber - 1] = new OccupiedParkingSpace(vehicle, DateTime.UtcNow);

            // return the vehicle reg, parking space number and check-in time
            return (vehicle.VehicleReg, spaceNumber, DateTime.UtcNow);
        }

        public (string VehicleReg, decimal parkingCharge, DateTime CheckInTime, DateTime now) CheckOut(string vehicleReg)
        {
            // find the parking space occupied by the vehicle
            var spaceNumber = Array.FindIndex(_parkingSpaces, x => x?.Vehicle.VehicleReg == vehicleReg);
            if (spaceNumber == -1)
            {
                throw new VehicleNotCheckedInException($"Vehicle with registration {vehicleReg} is not checked in.");
            }

            // TODO: need to refactor this out to a separate service so it can be injected and tested separately
            var now = DateTime.UtcNow;
            var vehicle = _parkingSpaces[spaceNumber]!.Vehicle;

            // calculate the charge based on the time spent in the parking space
            var parkingCharge = CalculateParkingCharge(_parkingSpaces[spaceNumber]!.CheckInTime, now, vehicle.Type);

            // return the vehicle registration, parking charge, and check-in / out times
            var data = (vehicle.VehicleReg, parkingCharge, _parkingSpaces[spaceNumber]!.CheckInTime, now);

            // de-allocate the parking space
            _parkingSpaces[spaceNumber] = null;

            return data;
        }

        private static decimal CalculateParkingCharge(DateTime checkInTime, DateTime now, VehicleType type)
        {
            // calculate the length of time spent in the parking space in minutes
            var timeSpent = (now - checkInTime.AddMinutes(-100)).TotalMinutes;

            // calculate parking charge based on vehicle type and time spent in the parking space
            var parkingCharge = type.ChargePerMinute * (decimal)timeSpent;

            // add additional charges
            var additionalCharge = (decimal)timeSpent / 5;

            return parkingCharge + additionalCharge;
        }

        private int FindNextAvailableParkingSpace()
        {
            var index = Array.FindIndex(_parkingSpaces, x => x is null);

            int? spaceNumber = index == -1
                ? null
                : index + 1;

            return spaceNumber ?? throw new NoAvailableParkingSpacesException("No available parking spaces.");
        }
    }
}
