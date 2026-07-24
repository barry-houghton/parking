using CarPark.Core.Persistence.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using ParCark.Api.Models;

namespace CarPark.Core.Persistence
{
    public class CarParkDbContext(DbContextOptions<CarParkDbContext> options) : DbContext(options)
    {
        public DbSet<OccupiedParkingSpace> OccupiedParkingSpaces => Set<OccupiedParkingSpace>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OccupiedParkingSpaceConfiguration());
        }
    }
}
