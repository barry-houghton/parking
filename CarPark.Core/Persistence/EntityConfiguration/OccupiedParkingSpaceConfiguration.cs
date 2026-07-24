using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParCark.Api.Models;

namespace CarPark.Core.Persistence.EntityConfiguration
{
    public class OccupiedParkingSpaceConfiguration : IEntityTypeConfiguration<OccupiedParkingSpace>
    {
        public void Configure(EntityTypeBuilder<OccupiedParkingSpace> builder)
        {
            builder.ToTable("OccupiedParkingSpaces");

            builder.HasKey(_ => _.Id);

            builder.Property(_ => _.Id)
                .ValueGeneratedNever();

            builder.Property(_ => _.VehicleReg)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(_ => _.VehicleType)
                .HasConversion(
                    v => v.Value,
                    v => VehicleType.FromValue(v))
                .IsRequired();
        }
    }
}
