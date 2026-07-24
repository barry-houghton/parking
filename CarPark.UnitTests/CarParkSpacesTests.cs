using CarPark.Core;
using CarPark.Core.Services;
using ParCark.Api.Models;
using Shouldly;

namespace CarPark.UnitTests
{
    public class CarParkSpacesTests
    {
        [Fact]
        public void ShouldReturnMaximumAvailableSpacesWhenCarParkIsEmpty()
        {
            // Arrange
            var carPark = new CarParkService();

            // Act
            var (AvailableSpaces, OccupiedSpaces) = carPark.GetAvailableSpaces();

            // Assert
            AvailableSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES);
            OccupiedSpaces.ShouldBe(0);
        }

        [Fact]
        public void ShouldReturnZeroAvailableSpacesWhenCarParkIsFull()
        {
            // Arrange
            var carPark = new CarParkService();

            foreach (var i in Enumerable.Range(1, Configuration.TOTAL_PARKING_SPACES))
            {
                carPark.CheckIn(new Vehicle($"ABC{i:000}", VehicleType.Small));
            }

            // Act
            var (AvailableSpaces, OccupiedSpaces) = carPark.GetAvailableSpaces();

            // Assert
            AvailableSpaces.ShouldBe(0);
            OccupiedSpaces.ShouldBe(Configuration.TOTAL_PARKING_SPACES);
        }

        [Theory]
        [InlineData(2, 8)]
        [InlineData(5, 5)]
        [InlineData(9, 1)]
        public void ShouldReturnCorrectAvailableSpacesWhenCarParkIsPartiallyOccupied(int occupiedSpaces, int availableSpaces)
        {
            // Arrange
            var carPark = new CarParkService();
            foreach (int i in Enumerable.Range(1, occupiedSpaces))
            {
                carPark.CheckIn(new Vehicle($"ABC{i:000}", VehicleType.Small));
            }

            // Act
            var (AvailableSpaces, OccupiedSpaces) = carPark.GetAvailableSpaces();
     
            // Assert   
            AvailableSpaces.ShouldBe(availableSpaces);
            OccupiedSpaces.ShouldBe(occupiedSpaces);
        }
    }    
}
