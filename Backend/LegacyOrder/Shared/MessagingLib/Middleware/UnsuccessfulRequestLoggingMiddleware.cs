

using LoggingLib.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LoggingLib.Middleware;

/// <summary>
/// After the request completes, logs non-2xx responses via <see cref="ILogPublisher"/> (correlation id comes from the same request context).
/// </summary>
public sealed class UnsuccessfulRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogPublisher _logPublisher;

    public UnsuccessfulRequestLoggingMiddleware(RequestDelegate next, ILogPublisher logPublisher)
    {
        _next = next;
        _logPublisher = logPublisher;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        var status = context.Response.StatusCode;
        if (status is >= 200 and <= 299)
            return;

        var path = $"{context.Request.Path}{context.Request.QueryString}";
        var message = $"{context.Request.Method} {path} responded {status}";

        if (status >= 500)
            await _logPublisher.ErrorAsync(message);
        else
            await _logPublisher.WarningAsync(message);
    }
}
