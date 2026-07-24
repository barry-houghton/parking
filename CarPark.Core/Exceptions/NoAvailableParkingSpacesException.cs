namespace CarPark.Core.Exceptions
{
    public class NoAvailableParkingSpacesException(string message) : Exception(message)
    {
    }
}
