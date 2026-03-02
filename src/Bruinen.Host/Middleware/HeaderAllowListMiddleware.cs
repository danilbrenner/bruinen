using Microsoft.Extensions.Options;

namespace Bruinen.Host.Middleware;

/// <summary>
/// Middleware that filters incoming request headers based on an allow list.
/// Only headers specified in the allow list will be passed through.
/// This helps prevent header injection attacks and limits exposed header information.
/// </summary>
public class HeaderAllowListMiddleware(
    RequestDelegate next,
    IOptions<HeaderAllowListOptions> options,
    ILogger<HeaderAllowListMiddleware> logger)
{
    private readonly HashSet<string> _allowedHeaders = new(
        options.Value.AllowedHeaders ?? [],
        StringComparer.OrdinalIgnoreCase);

    public async Task InvokeAsync(HttpContext context)
    {
        if (_allowedHeaders.Count == 0)
        {
            await next(context);
            return;
        }

        context
            .Request
            .Headers
            .Keys
            .Where(headerName => !_allowedHeaders.Contains(headerName))
            .ToList()
            .ForEach(headerName =>
            {
                logger.LogDebug("Removing disallowed header: {HeaderName}", headerName);
                context.Request.Headers.Remove(headerName);
            });

        await next(context);
    }
}

/// <summary>
/// Configuration options for HeaderAllowListMiddleware.
/// </summary>
public class HeaderAllowListOptions
{
    /// <summary>
    /// List of header names that are allowed to pass through.
    /// Header name comparison is case-insensitive.
    /// If empty or null, all headers are allowed.
    /// </summary>
    public List<string>? AllowedHeaders { get; set; }
}