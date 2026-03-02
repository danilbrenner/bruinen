using Bruinen.Host.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bruinen.UnitTests.Middleware;

public class HeaderAllowListMiddlewareTests
{
    private readonly Mock<ILogger<HeaderAllowListMiddleware>> _loggerMock = new();
    private readonly Mock<RequestDelegate> _nextMock = new();

    [Fact]
    public async Task InvokeAsync_WithEmptyAllowList_AllowsAllHeaders()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = []
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Custom-Header"] = "value1";
        context.Request.Headers["X-Another-Header"] = "value2";
        context.Request.Headers["Authorization"] = "Bearer token";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(3, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("X-Custom-Header"));
        Assert.True(context.Request.Headers.ContainsKey("X-Another-Header"));
        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithNullAllowList_AllowsAllHeaders()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = null
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Custom-Header"] = "value1";
        context.Request.Headers["Authorization"] = "Bearer token";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(2, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("X-Custom-Header"));
        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAllowList_RemovesDisallowedHeaders()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["Authorization", "Content-Type", "Accept"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer token";
        context.Request.Headers["Content-Type"] = "application/json";
        context.Request.Headers["X-Custom-Header"] = "should-be-removed";
        context.Request.Headers["X-Another-Header"] = "should-be-removed-too";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(2, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
        Assert.True(context.Request.Headers.ContainsKey("Content-Type"));
        Assert.False(context.Request.Headers.ContainsKey("X-Custom-Header"));
        Assert.False(context.Request.Headers.ContainsKey("X-Another-Header"));
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAllowList_KeepsOnlyAllowedHeaders()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["Host", "User-Agent"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["Host"] = "example.com";
        context.Request.Headers["User-Agent"] = "TestAgent/1.0";
        context.Request.Headers["Cookie"] = "session=abc123";
        context.Request.Headers["Referer"] = "https://example.com/page";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(2, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("Host"));
        Assert.True(context.Request.Headers.ContainsKey("User-Agent"));
        Assert.False(context.Request.Headers.ContainsKey("Cookie"));
        Assert.False(context.Request.Headers.ContainsKey("Referer"));
    }

    [Fact]
    public async Task InvokeAsync_WithCaseInsensitiveMatch_AllowsHeadersRegardlessOfCase()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["authorization", "content-type"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer token"; // Different case
        context.Request.Headers["CONTENT-TYPE"] = "application/json"; // Different case
        context.Request.Headers["X-Custom"] = "should-be-removed";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(2, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
        Assert.True(context.Request.Headers.ContainsKey("CONTENT-TYPE"));
        Assert.False(context.Request.Headers.ContainsKey("X-Custom"));
    }

    [Fact]
    public async Task InvokeAsync_WithReverseProxyHeaders_AllowsForwardedHeaders()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = [
                "X-Forwarded-For",
                "X-Forwarded-Host",
                "X-Forwarded-Proto",
                "X-Forwarded-Uri",
                "X-Real-IP"
            ]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
        context.Request.Headers["X-Forwarded-Host"] = "example.com";
        context.Request.Headers["X-Forwarded-Proto"] = "https";
        context.Request.Headers["X-Forwarded-Uri"] = "/path";
        context.Request.Headers["X-Real-IP"] = "192.168.1.1";
        context.Request.Headers["X-Custom"] = "should-be-removed";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(5, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("X-Forwarded-For"));
        Assert.True(context.Request.Headers.ContainsKey("X-Forwarded-Host"));
        Assert.True(context.Request.Headers.ContainsKey("X-Forwarded-Proto"));
        Assert.True(context.Request.Headers.ContainsKey("X-Forwarded-Uri"));
        Assert.True(context.Request.Headers.ContainsKey("X-Real-IP"));
        Assert.False(context.Request.Headers.ContainsKey("X-Custom"));
    }

    [Fact]
    public async Task InvokeAsync_WithNoHeaders_CallsNext()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["Authorization"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Empty(context.Request.Headers);
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAllHeadersAllowed_CallsNext()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["Authorization", "Cookie", "Host"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer token";
        context.Request.Headers["Cookie"] = "session=abc";
        context.Request.Headers["Host"] = "example.com";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(3, context.Request.Headers.Count);
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithRemovedHeaders_LogsDebugMessage()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["Authorization"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer token";
        context.Request.Headers["X-Custom-Header"] = "value";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("X-Custom-Header")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("accept")]
    [InlineData("ACCEPT")]
    [InlineData("Accept")]
    [InlineData("aCcEpT")]
    public async Task InvokeAsync_WithVariousCasing_HandlesHeadersCaseInsensitively(string headerName)
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["accept"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers[headerName] = "application/json";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Single(context.Request.Headers);
        Assert.True(context.Request.Headers.ContainsKey(headerName));
    }

    [Fact]
    public async Task InvokeAsync_WithCommonHttpHeaders_FiltersCorrectly()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = [
                "Accept",
                "Accept-Encoding",
                "Accept-Language",
                "Authorization",
                "Content-Type",
                "Cookie",
                "Host",
                "User-Agent"
            ]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        
        // Add allowed headers
        context.Request.Headers["Accept"] = "application/json";
        context.Request.Headers["Content-Type"] = "application/json";
        context.Request.Headers["Authorization"] = "Bearer token";
        context.Request.Headers["Host"] = "example.com";
        
        // Add disallowed headers
        context.Request.Headers["X-Custom"] = "custom-value";
        context.Request.Headers["X-Debug"] = "debug-info";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(4, context.Request.Headers.Count);
        Assert.True(context.Request.Headers.ContainsKey("Accept"));
        Assert.True(context.Request.Headers.ContainsKey("Content-Type"));
        Assert.True(context.Request.Headers.ContainsKey("Authorization"));
        Assert.True(context.Request.Headers.ContainsKey("Host"));
        Assert.False(context.Request.Headers.ContainsKey("X-Custom"));
        Assert.False(context.Request.Headers.ContainsKey("X-Debug"));
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleDisallowedHeaders_RemovesAllOfThem()
    {
        // Arrange
        var options = Options.Create(new HeaderAllowListOptions
        {
            AllowedHeaders = ["Host"]
        });

        var middleware = new HeaderAllowListMiddleware(_nextMock.Object, options, _loggerMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers["Host"] = "example.com";
        context.Request.Headers["Header1"] = "value1";
        context.Request.Headers["Header2"] = "value2";
        context.Request.Headers["Header3"] = "value3";
        context.Request.Headers["Header4"] = "value4";
        context.Request.Headers["Header5"] = "value5";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Single(context.Request.Headers);
        Assert.True(context.Request.Headers.ContainsKey("Host"));
        Assert.False(context.Request.Headers.ContainsKey("Header1"));
        Assert.False(context.Request.Headers.ContainsKey("Header2"));
        Assert.False(context.Request.Headers.ContainsKey("Header3"));
        Assert.False(context.Request.Headers.ContainsKey("Header4"));
        Assert.False(context.Request.Headers.ContainsKey("Header5"));
    }
}

