using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Interfaces;
using Orders.Application.Mappings;
using Orders.Application.Services;
using Orders.Infrastructure.Extensions;
using Orders.Infrastructure.HttpClients;

namespace Orders.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOrdersInfrastructure(configuration);

        services.AddAutoMapper(cfg => cfg.AddProfile<OrderMappingProfile>());

        services.AddScoped<ISequenceService, SequenceService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddHttpContextAccessor();
        
        services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalApis:ProductsBaseUrl"]
                                         ?? throw new InvalidOperationException("ExternalApis:ProductsBaseUrl is not configured."));
        });

        services.AddHttpClient<IContactApiClient, ContactApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalApis:ContactsBaseUrl"]
                                         ?? throw new InvalidOperationException("ExternalApis:ContactsBaseUrl is not configured."));
        });

        return services;
    }
}
