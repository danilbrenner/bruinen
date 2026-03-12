using Bruinen.Application.Abstractions;
using Bruinen.Application.Services;
using Bruinen.Domain;

namespace Bruinen.UnitTests.ApplicationServices;

public class RateLimitingServiceTests
{
    private readonly Mock<IRequestCounterRepository> _requestCounterRepositoryMock = new();
    private readonly RateLimitingService _sut;

    public RateLimitingServiceTests()
    {
        _sut = new RateLimitingService(_requestCounterRepositoryMock.Object);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenCounterIsZero_ReturnsNull(string key)
    {
        // Arrange
        var counter = new RequestCounter(key, DateTimeOffset.UtcNow);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        var result = await _sut.VerifyRequestAllowed(key, maxRequests: 5, lockoutDuration: TimeSpan.FromMinutes(1));

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenCounterIsZero_IncrementsAndSavesCounter(string key)
    {
        // Arrange
        var counter = new RequestCounter(key, DateTimeOffset.UtcNow);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        await _sut.VerifyRequestAllowed(key, maxRequests: 5, lockoutDuration: TimeSpan.FromMinutes(1));

        // Assert
        Assert.Equal(1, counter.Count);
        _requestCounterRepositoryMock.Verify(x => x.Save(counter), Times.Once);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenBelowMaxRequests_ReturnsNull(string key)
    {
        // Arrange
        const int maxRequests = 5;
        var counter = new RequestCounter(key, DateTimeOffset.UtcNow, initialCount: maxRequests - 1);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        var result = await _sut.VerifyRequestAllowed(key, maxRequests, lockoutDuration: TimeSpan.FromMinutes(1));

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenExactlyAtMaxRequestsAndLockoutActive_ReturnsLockoutExpiry(string key)
    {
        // Arrange
        const int maxRequests = 5;
        var lockoutDuration = TimeSpan.FromMinutes(1);
        var lastUpdated = DateTimeOffset.UtcNow; // lockout still active
        var counter = new RequestCounter(key, lastUpdated, initialCount: maxRequests);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        var result = await _sut.VerifyRequestAllowed(key, maxRequests, lockoutDuration);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lastUpdated.Add(lockoutDuration), result);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenAtMultipleOfMaxRequestsAndLockoutActive_ReturnsLockoutExpiry(string key)
    {
        // Arrange
        const int maxRequests = 5;
        var lockoutDuration = TimeSpan.FromMinutes(1);
        var lastUpdated = DateTimeOffset.UtcNow;
        var counter = new RequestCounter(key, lastUpdated, initialCount: maxRequests * 3);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        var result = await _sut.VerifyRequestAllowed(key, maxRequests, lockoutDuration);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lastUpdated.Add(lockoutDuration), result);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenAtMultipleOfMaxRequestsAndLockoutActive_DoesNotSaveCounter(string key)
    {
        // Arrange
        const int maxRequests = 5;
        var lockoutDuration = TimeSpan.FromMinutes(1);
        var counter = new RequestCounter(key, DateTimeOffset.UtcNow, initialCount: maxRequests);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        await _sut.VerifyRequestAllowed(key, maxRequests, lockoutDuration);

        // Assert
        _requestCounterRepositoryMock.Verify(x => x.Save(It.IsAny<RequestCounter>()), Times.Never);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenAtMaxRequestsButLockoutExpired_ReturnsNull(string key)
    {
        // Arrange
        const int maxRequests = 5;
        var lockoutDuration = TimeSpan.FromMinutes(1);
        var lastUpdated = DateTimeOffset.UtcNow.Subtract(lockoutDuration).AddSeconds(-1); // expired
        var counter = new RequestCounter(key, lastUpdated, initialCount: maxRequests);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        var result = await _sut.VerifyRequestAllowed(key, maxRequests, lockoutDuration);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [AutoData]
    public async Task VerifyRequestAllowed_WhenAtMaxRequestsButLockoutExpired_IncrementsAndSavesCounter(string key)
    {
        // Arrange
        const int maxRequests = 5;
        var lockoutDuration = TimeSpan.FromMinutes(1);
        var lastUpdated = DateTimeOffset.UtcNow.Subtract(lockoutDuration).AddSeconds(-1); // expired
        var counter = new RequestCounter(key, lastUpdated, initialCount: maxRequests);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);

        // Act
        await _sut.VerifyRequestAllowed(key, maxRequests, lockoutDuration);

        // Assert
        Assert.Equal(maxRequests + 1, counter.Count);
        _requestCounterRepositoryMock.Verify(x => x.Save(counter), Times.Once);
    }
}

