using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Boovatech.Domain.Events;
using Boovatech.Domain.SuspendedEvents;

namespace Boovatech.Eventing.Infrastructure
{
    public class SuspendedEventPublisher : ISuspendedEventPublisher
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<SuspendedEventPublisher> logger;

        public SuspendedEventPublisher(
            IServiceScopeFactory scopeFactory,
            ILogger<SuspendedEventPublisher> logger)
        {
            ArgumentNullException.ThrowIfNull(scopeFactory);
            ArgumentNullException.ThrowIfNull(logger);
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public Task PostAccountDebitedAsync(string account, string tenant)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var handlerFactory = scope.ServiceProvider.GetRequiredService<Func<AccountDebitedEventHandler>>();
                    var handler = handlerFactory();
                    await handler.HandleEventAsync(new AccountDebitedEvent(account, tenant));
                }
                catch (Exception exception)
                {
                    logger.LogError(exception,
                        "Failed to process account notification for Account={Account} Tenant={Tenant}", account, tenant);
                }
            });
            return Task.CompletedTask;
        }
    }
}

