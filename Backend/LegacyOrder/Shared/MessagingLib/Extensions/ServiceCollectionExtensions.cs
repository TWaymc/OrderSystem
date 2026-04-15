using LoggingLib.Middleware;
using LoggingLib.Services;
using LoggingLib.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoggingLib.Extensions;

public static  class ServiceCollectionExtensions
{
    
    // ----    Include both in the program.cs
    
    public static IServiceCollection AddLoggingMessages(this IServiceCollection services, string defaultServiceName)
    {
        services.AddSingleton<ILogPublisher>(provider =>
            new LogPublisher(
                provider.GetRequiredService<IHttpContextAccessor>(),
                defaultServiceName));

        return services;
    }
    
    public static WebApplication AddLoggingMessages(this WebApplication app)
    {
        // -- Middleware to get set Correlation Id 
        app.UseMiddleware<CorrelationIdMiddleware>();
        // -- Middleware to log by default any query not successful 
        app.UseMiddleware<UnsuccessfulRequestLoggingMiddleware>();

        return app;
    }
}