using CarPark.Core.Exceptions;
using CarPark.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using ParCark.Api.Models;

namespace CarPark.Core.Services
{
    public class CarParkService : ICarParkService
    {
        private readonly CarParkDbContext _carParkDbContext;
        private readonly IDateTimeHelper _dateTimeHelper;

        public CarParkService(CarParkDbContext carParkDbContext, IDateTimeHelper dateTimeHelper)
        {
            _carParkDbContext = carParkDbContext;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<(int AvailableSpaces, int OccupiedSpaces)> GetAvailableSpaces(CancellationToken ct = default)
        {
            int occupiedSpaces = await _carParkDbContext.OccupiedParkingSpaces.CountAsync(ct);
            return (
                AvailableSpaces: Configuration.TOTAL_PARKING_SPACES - occupiedSpaces,
                OccupiedSpaces: occupiedSpaces
            );
        }

        public async Task<(string VehicleReg, int SpaceNumber, DateTime CheckInTime)> CheckIn(string vehicleReg, int vehicleTypeValue, CancellationToken ct)
        {
            if (!VehicleType.TryFromValue(vehicleTypeValue, out var vehicleType))
            {
                throw new InvalidVehicleTypeException($"Invalid vehicle type: {vehicleTypeValue}");
            }

            // check if vehicle is already checked in
            var existingVehicle = await _carParkDbContext.OccupiedParkingSpaces.FirstOrDefaultAsync(x => x.VehicleReg == vehicleReg, ct);
            if (existingVehicle != null)
            {
                throw new VehicleAlreadyParkedException($"Vehicle with registration {vehicleReg} is already parked.");
            }

            // find next available parking space
            var spaceNumber = await FindNextAvailableParkingSpace(ct);

            // add vehicle to parking space
            _carParkDbContext.OccupiedParkingSpaces.Add(new OccupiedParkingSpace(spaceNumber, vehicleReg, vehicleType, _dateTimeHelper.GetUtcNow()));
            await _carParkDbContext.SaveChangesAsync(ct);

            // return the vehicle reg, parking space number and check-in time
            return (vehicleReg, spaceNumber, _dateTimeHelper.GetUtcNow());
        }

        public async Task<(string VehicleReg, decimal parkingCharge, DateTime CheckInTime, DateTime CheckOutTime)> CheckOut(string vehicleReg, CancellationToken ct)
        {
            // find the parking space occupied by the vehicle
            var existingVehicle = 
                await _carParkDbContext.OccupiedParkingSpaces.FirstOrDefaultAsync(x => x.VehicleReg == vehicleReg, ct)
                ?? throw new VehicleNotCheckedInException($"Vehicle with registration {vehicleReg} is not checked in.");

            var now = _dateTimeHelper.GetUtcNow();

            // calculate the charge based on the time spent in the parking space
            var parkingCharge = CalculateParkingCharge(existingVehicle.CheckInTime, now, existingVehicle.VehicleType);

            // return the vehicle registration, parking charge, and check-in / out times
            var data = (vehicleReg, parkingCharge, existingVehicle.CheckInTime, now);

            // de-allocate the parking space
            _carParkDbContext.Remove(existingVehicle);
            await _carParkDbContext.SaveChangesAsync(ct);

            return data;
        }

        private static decimal CalculateParkingCharge(DateTime checkInTime, DateTime now, VehicleType type)
        {
            // calculate the length of time spent in the parking space in minutes
            var timeSpent = (int)((now - checkInTime).TotalMinutes);

            // calculate parking charge based on vehicle type and time spent in the parking space
            var parkingCharge = type.ChargePerMinute * (decimal)timeSpent;

            // add additional charges - add £1 charge for every 5 minutes
            var additionalCharge = (int)(timeSpent / 5);

            return parkingCharge + additionalCharge;
        }

        private async Task<int> FindNextAvailableParkingSpace(CancellationToken ct)
        {
            if (await _carParkDbContext.OccupiedParkingSpaces.CountAsync(ct) == Configuration.TOTAL_PARKING_SPACES)
            {
                throw new NoAvailableParkingSpacesException("No available parking spaces.");
            }

            var occupiedSpaces = await _carParkDbContext.OccupiedParkingSpaces.Select(x => x.Id).ToListAsync(ct);

            var nextFreeSpace = Enumerable.Range(1, Configuration.TOTAL_PARKING_SPACES)
                .Except(occupiedSpaces)
                .FirstOrDefault();

            return nextFreeSpace;
        }
    }
}
