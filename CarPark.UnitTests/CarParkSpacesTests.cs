using CarPark.Core;
using CarPark.Core.Persistence;
using CarPark.Core.Services;
using CarPark.UnitTests.Mocks;
using Microsoft.EntityFrameworkCore;
using ParCark.Api.Models;
using Shouldly;

namespace CarPark.UnitTests
{
    public class CarParkSpacesTests
    {
        private readonly CarParkDbContext _dbContext;

        public CarParkSpacesTests()
        {
            var options = new DbContextOptionsBuilder<CarParkDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CarParkDbContext(options);
        }

        [Fact]
        public async Task ShouldReturnMaximumAvailableSpacesWhenCarParkIsEmpty()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);

            // Act
            var (AvailableSpaces, OccupiedSpaces) = await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken);

            // Assert
            AvailableSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES);
            OccupiedSpaces.ShouldBe(0);
        }

        [Fact]
        public async Task ShouldReturnZeroAvailableSpacesWhenCarParkIsFull()
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);

            foreach (var i in Enumerable.Range(1, Configuration.TOTAL_PARKING_SPACES))
            {
                await carPark.CheckIn($"ABC{i:000}", VehicleType.Small, TestContext.Current.CancellationToken);
            }

            // Act
            var (AvailableSpaces, OccupiedSpaces) = await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken);

            // Assert
            AvailableSpaces.ShouldBe(0);
            OccupiedSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES);
        }

        [Theory]
        [InlineData(2, 8)]
        [InlineData(5, 5)]
        [InlineData(9, 1)]
        public async Task ShouldReturnCorrectAvailableSpacesWhenCarParkIsPartiallyOccupied(int occupiedSpaces, int availableSpaces)
        {
            // Arrange
            var carPark = new CarParkService(_dbContext, new MockDateTimeHelper().Object);
            foreach (int i in Enumerable.Range(1, occupiedSpaces))
            {
                await carPark.CheckIn($"ABC{i:000}", VehicleType.Small, TestContext.Current.CancellationToken);
            }

            // Act
            var (AvailableSpaces, OccupiedSpaces) = await carPark.GetAvailableSpaces(TestContext.Current.CancellationToken);

            // Assert   
            AvailableSpaces.ShouldBe(availableSpaces);
            OccupiedSpaces.ShouldBe(occupiedSpaces);
        }
    }
}
