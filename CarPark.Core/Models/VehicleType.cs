using Ardalis.SmartEnum;

namespace ParCark.Api.Models
{
    public sealed class VehicleType : SmartEnum<VehicleType>
    {
        public static readonly VehicleType Small = new("Small", 1, 0.1m);
        public static readonly VehicleType Medium = new("Medium", 2, 0.2m);
        public static readonly VehicleType Large = new("Large", 3, 0.4m);

        public decimal ChargePerMinute { get; }

        private VehicleType(string name, int value, decimal chargePerMinute) : base(name, value)
        {
            ChargePerMinute = chargePerMinute;
        }
    }
}
