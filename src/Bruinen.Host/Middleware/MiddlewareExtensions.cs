namespace Bruinen.Host.Middleware;

/// <summary>
/// Extension methods for registering custom middleware.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds the RequestIdMiddleware to the application pipeline.
    /// This middleware ensures every request has a unique X-Request-Id header.
    /// </summary>
    public static IApplicationBuilder UseRequestId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestIdMiddleware>();
    }
}

