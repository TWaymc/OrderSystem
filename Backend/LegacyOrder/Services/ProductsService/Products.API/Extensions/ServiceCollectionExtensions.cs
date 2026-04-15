using LoggingLib.Services;
using LoggingLib.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Products.Application.Interfaces;
using Products.Application.Mappings;
using Products.Application.Services;
using Products.Domain.Interfaces;
using Products.Infrastructure.Data;
using Products.Infrastructure.Messaging.Publisher;
using Products.Infrastructure.Messaging.Publisher.Interface;
using Products.Infrastructure.Repositories;

namespace Products.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISequenceRepository, SequenceRepository>();

        services.AddAutoMapper(cfg => cfg.AddProfile<ProductMappingProfile>());

        services.AddScoped<ISequenceService, SequenceService>();
        services.AddScoped<IProductService, ProductService>();
        
        // -- publisher for Product Update Event
        services.AddSingleton<IProductPublisher, ProductPublisher>();
        
        services.AddHttpContextAccessor();

        return services;
    }
}