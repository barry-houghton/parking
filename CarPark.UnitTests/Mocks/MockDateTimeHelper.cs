using CarPark.Core.Services;
using Moq;

namespace CarPark.UnitTests.Mocks
{
    public class MockDateTimeHelper : Mock<IDateTimeHelper>
    {
        public MockDateTimeHelper() 
        {
            Setup(m => m.GetUtcNow()).Returns(DateTime.UtcNow);
        }

        public void AdvanceTimeBy(TimeSpan timeSpan)
        {
            var currentTime = DateTime.UtcNow;
            Setup(m => m.GetUtcNow()).Returns(currentTime.Add(timeSpan));
        }
    }
}
