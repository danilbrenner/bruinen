using Bruinen.Domain;

namespace Bruinen.UnitTests.Domain;

public class RequestCounterTests
{
    [Theory]
    [AutoData]
    public void Constructor_ShouldSetKeyCountAndLastUpdated(
        string key,
        DateTimeOffset lastUpdated,
        int initialCount)
    {
        // Arrange & Act
        var counter = new RequestCounter(key, lastUpdated, initialCount);

        // Assert
        Assert.Equal(key, counter.Key);
        Assert.Equal(initialCount, counter.Count);
        Assert.Equal(lastUpdated, counter.LastUpdated);
    }

    [Theory]
    [AutoData]
    public void Constructor_WhenNoInitialCount_ShouldDefaultCountToZero(
        string key,
        DateTimeOffset lastUpdated)
    {
        // Arrange & Act
        var counter = new RequestCounter(key, lastUpdated);

        // Assert
        Assert.Equal(0, counter.Count);
    }

    [Theory]
    [AutoData]
    public void Increment_ShouldIncreaseCountByOne(
        string key,
        DateTimeOffset lastUpdated,
        int initialCount)
    {
        // Arrange
        var counter = new RequestCounter(key, lastUpdated, initialCount);

        // Act
        counter.Increment();

        // Assert
        Assert.Equal(initialCount + 1, counter.Count);
    }

    [Theory]
    [AutoData]
    public void Increment_ShouldUpdateLastUpdatedToUtcNow(
        string key,
        int initialCount)
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var counter = new RequestCounter(key, before.AddDays(-1), initialCount);

        // Act
        counter.Increment();

        // Assert
        Assert.True(counter.LastUpdated >= before);
        Assert.True(counter.LastUpdated <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [AutoData]
    public void Increment_CalledMultipleTimes_ShouldAccumulateCount(
        string key,
        DateTimeOffset lastUpdated)
    {
        // Arrange
        var counter = new RequestCounter(key, lastUpdated, 0);

        // Act
        counter.Increment();
        counter.Increment();
        counter.Increment();

        // Assert
        Assert.Equal(3, counter.Count);
    }
}

