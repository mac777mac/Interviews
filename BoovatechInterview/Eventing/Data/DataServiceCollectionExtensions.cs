using Boovatech.Eventing.EventValidation;
using Boovatech.Domain;
using Boovatech.Eventing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Boovatech.Eventing.Data;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddEventingData(this IServiceCollection services, string connectionString,
        TimeSpan leaseDuration, int retriesCount = 3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.AddDbContextFactory<EventingDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<Func<EventingDbContext>>(provider =>
            provider.GetRequiredService<IDbContextFactory<EventingDbContext>>().CreateDbContext);
        services.AddScoped<ITenantsRepository, TenantsRepository>();
        services.AddScoped<IAccountsRespository, AccountsRespository>();
        services.AddScoped<IEventsRepository, EventsRepository>();
        services.AddScoped<IInboxMessagesStore>(provider => new InboxMessagesStore(
            provider.GetRequiredService<Func<EventingDbContext>>(), leaseDuration, retriesCount));
        return services;
    }
}
