using Serilog.Context;

namespace Bruinen.Host.Middleware;

/// <summary>
/// Middleware that ensures every request has a unique X-Request-Id header.
/// If the header is not present in the incoming request, a new GUID is generated.
/// The X-Request-Id is added to the response headers and to the logging context.
/// </summary>
public class RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
{
    private const string RequestIdHeaderName = "X-Request-Id";
    
    private static string? TryGetRequestId(HttpContext context)
    {
        return 
            context.Request.Headers.TryGetValue(RequestIdHeaderName, out var requestIdHeader) 
                ? requestIdHeader.ToString() 
                : null;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = TryGetRequestId(context) ?? Guid.NewGuid().ToString();
        
        context.Response.Headers[RequestIdHeaderName] = requestId;

        using (LogContext.PushProperty("RequestId", requestId))
        {
            logger.LogDebug("Processing request {RequestId} for {Path}", requestId, context.Request.Path);
            
            await next(context);
            
            logger.LogDebug("Completed request {RequestId} with status {StatusCode}", 
                requestId, context.Response.StatusCode);
        }
    }
}

