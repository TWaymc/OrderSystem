using Contacts.Application.Interfaces;
using Contacts.Application.Mappings;
using Contacts.Application.Services;
using Contacts.Domain.Interfaces;
using Contacts.Infrastructure.Data;
using Contacts.Infrastructure.Messaging.Publisher;
using Contacts.Infrastructure.Messaging.Publisher.Interface;
using Contacts.Infrastructure.Repositories;
using LoggingLib.Services;
using LoggingLib.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Contacts.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ISequenceRepository, SequenceRepository>();

        services.AddAutoMapper(cfg => cfg.AddProfile<ContactMappingProfile>());

        services.AddScoped<ISequenceService, SequenceService>();
        services.AddScoped<IContactService, ContactService>();

        services.AddSingleton<IContactPublisher, ContactPublisher>();

        services.AddHttpContextAccessor();

        return services;
    }
}
