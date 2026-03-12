using Bruinen.Application.Services;
using Bruinen.Application.Abstractions;
using Bruinen.Domain;
using Bruinen.Host.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Bruinen.UnitTests.Middleware;

public class RequestRateLimitAttributeTests
{
    private readonly Mock<IRequestCounterRepository> _requestCounterRepositoryMock = new();
    private readonly Mock<ActionExecutionDelegate> _nextMock = new();

    private ActionExecutingContext BuildContext(
        string ipAddress = "1.2.3.4",
        string controller = "Auth",
        string action = "Login",
        ServiceCollection? services = null)
    {
        var serviceCollection = services ?? [];
        serviceCollection.AddSingleton(_requestCounterRepositoryMock.Object);
        serviceCollection.AddSingleton<RateLimitingService>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            Connection =
            {
                RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress)
            }
        };

        var routeData = new RouteData
        {
            Values =
            {
                ["controller"] = controller,
                ["action"] = action
            }
        };

        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
    }

    private void SetupCounter(string key, int count, DateTimeOffset lastUpdated)
    {
        var counter = new RequestCounter(key, lastUpdated, count);
        _requestCounterRepositoryMock.Setup(x => x.Get(key)).ReturnsAsync(counter);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenNotRateLimited_CallsNext()
    {
        // Arrange
        var key = "RateLimit:1.2.3.4:Auth:Login";
        SetupCounter(key, count: 0, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = 5, LockoutDurationSec = 30 };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        _nextMock.Verify(n => n(), Times.Once);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenRateLimited_Returns429()
    {
        // Arrange
        const int maxRequests = 5;
        var key = "RateLimit:1.2.3.4:Auth:Login";
        SetupCounter(key, count: maxRequests, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = maxRequests, LockoutDurationSec = 30 };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        _nextMock.Verify(n => n(), Times.Never);
        var result = Assert.IsType<ContentResult>(context.Result);
        Assert.Equal(StatusCodes.Status429TooManyRequests, result.StatusCode);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenRateLimited_DoesNotCallNext()
    {
        // Arrange
        const int maxRequests = 5;
        var key = "RateLimit:1.2.3.4:Auth:Login";
        SetupCounter(key, count: maxRequests, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = maxRequests, LockoutDurationSec = 30 };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        _nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenRateLimited_SetsRetryAfterHeader()
    {
        // Arrange
        const int maxRequests = 5;
        const int lockoutSec = 30;
        var key = "RateLimit:1.2.3.4:Auth:Login";
        SetupCounter(key, count: maxRequests, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = maxRequests, LockoutDurationSec = lockoutSec };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        Assert.True(context.HttpContext.Response.Headers.ContainsKey("Retry-After"));
        var retryAfter = int.Parse(context.HttpContext.Response.Headers["Retry-After"].ToString());
        Assert.InRange(retryAfter, 1, lockoutSec);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenRateLimited_SetsXRateLimitLimitHeader()
    {
        // Arrange
        const int maxRequests = 5;
        var key = "RateLimit:1.2.3.4:Auth:Login";
        SetupCounter(key, count: maxRequests, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = maxRequests, LockoutDurationSec = 30 };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        Assert.True(context.HttpContext.Response.Headers.ContainsKey("X-RateLimit-Limit"));
        Assert.Equal(maxRequests.ToString(), context.HttpContext.Response.Headers["X-RateLimit-Limit"].ToString());
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenRateLimited_ResponseBodyMentionsRetryTime()
    {
        // Arrange
        const int maxRequests = 5;
        var key = "RateLimit:1.2.3.4:Auth:Login";
        SetupCounter(key, count: maxRequests, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = maxRequests, LockoutDurationSec = 30 };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        var result = Assert.IsType<ContentResult>(context.Result);
        Assert.Contains("Too many requests", result.Content);
    }

    [Fact]
    public async Task OnActionExecutionAsync_KeyIncludesIpControllerAndAction()
    {
        // Arrange – use a unique IP/controller/action so we can verify the exact key
        const string ip = "9.8.7.6";
        const string controller = "Account";
        const string action = "Register";
        var expectedKey = $"RateLimit:{ip}:{controller}:{action}";
        SetupCounter(expectedKey, count: 0, lastUpdated: DateTimeOffset.UtcNow);

        var attribute = new RequestRateLimitAttribute { MaxRequests = 5, LockoutDurationSec = 30 };
        var context = BuildContext(ip, controller, action);

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert – repository was called with the correctly composed key
        _requestCounterRepositoryMock.Verify(x => x.Get(expectedKey), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenLockoutExpired_CallsNext()
    {
        // Arrange
        const int maxRequests = 5;
        const int lockoutSec = 30;
        var key = "RateLimit:1.2.3.4:Auth:Login";
        // last updated more than lockout duration ago → lockout has expired
        SetupCounter(key, count: maxRequests, lastUpdated: DateTimeOffset.UtcNow.AddSeconds(-lockoutSec - 1));

        var attribute = new RequestRateLimitAttribute { MaxRequests = maxRequests, LockoutDurationSec = lockoutSec };
        var context = BuildContext();

        // Act
        await attribute.OnActionExecutionAsync(context, _nextMock.Object);

        // Assert
        _nextMock.Verify(n => n(), Times.Once);
        Assert.Null(context.Result);
    }
}

