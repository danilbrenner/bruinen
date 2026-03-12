using Bruinen.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bruinen.Host.Middleware;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequestRateLimitAttribute : Attribute, IAsyncActionFilter
{
    public int MaxRequests { get; set; } = 5;
    public int LockoutDurationSec { get; set; } = 30;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "unknown";
        var action = context.RouteData.Values["action"]?.ToString() ?? "unknown";
        var key = $"RateLimit:{ipAddress}:{controller}:{action}";

        var rateLimitingService = context.HttpContext.RequestServices.GetRequiredService<RateLimitingService>();
        var lockedUntil = await rateLimitingService.VerifyRequestAllowed(
            key,
            MaxRequests,
            TimeSpan.FromSeconds(LockoutDurationSec));
        
        if (!lockedUntil.HasValue)
        {
            await next();
            return;
        }
        
        var retryAfterSeconds = (int)Math.Ceiling((lockedUntil.Value - DateTimeOffset.UtcNow).TotalSeconds);
        context.HttpContext.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
        context.HttpContext.Response.Headers["X-RateLimit-Limit"] = MaxRequests.ToString();

        context.Result = new ContentResult
        {
            StatusCode = StatusCodes.Status429TooManyRequests,
            Content = $"Too many requests. Please retry after {lockedUntil}.",
            ContentType = "text/plain"
        };
    }
}