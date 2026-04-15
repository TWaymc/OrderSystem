using Microsoft.AspNetCore.Http;

namespace LoggingLib.Middleware;


public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string HttpContextItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;
        if (context.Request.Headers.TryGetValue(HeaderName, out var incoming) &&
            !string.IsNullOrWhiteSpace(incoming))
        {
            correlationId = incoming.ToString();
        }
        else
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[HttpContextItemKey] = correlationId;

        context.Response.OnStarting(OnStartingCallback, (context, correlationId));

        await _next(context);
    }

    private static Task OnStartingCallback(object state)
    {
        var (ctx, correlationId) = ((HttpContext, string))state!;
        if (!ctx.Response.Headers.ContainsKey(HeaderName))
            ctx.Response.Headers.Append(HeaderName, correlationId);
        return Task.CompletedTask;
    }
}
