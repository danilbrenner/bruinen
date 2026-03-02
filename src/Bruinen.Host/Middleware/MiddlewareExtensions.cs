namespace Bruinen.Host.Middleware;

/// <summary>
/// Extension methods for registering custom middleware.
/// </summary>
public static class MiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Adds the RequestIdMiddleware to the application pipeline.
        /// This middleware ensures every request has a unique X-Request-Id header.
        /// </summary>
        public IApplicationBuilder UseRequestId()
        {
            return app.UseMiddleware<RequestIdMiddleware>();
        }

        /// <summary>
        /// Adds the HeaderAllowListMiddleware to the application pipeline.
        /// This middleware filters incoming request headers based on an allow list.
        /// </summary>
        public IApplicationBuilder UseHeaderAllowList()
        {
            return app.UseMiddleware<HeaderAllowListMiddleware>();
        }
    }
}

