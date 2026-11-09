using System.Diagnostics;

namespace SchoolAdmission.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            var startedAt = Stopwatch.GetTimestamp();

            logger.LogDebug(
                "HTTP {Method} {Path} started.",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception escaped the exception handler.");
                throw;
            }
            finally
            {
                var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
                var level = context.Response.StatusCode >= 500
                    ? LogLevel.Warning
                    : LogLevel.Information;

                logger.Log(
                    level,
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:0.0} ms.",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs);
            }
        }
    }
}
