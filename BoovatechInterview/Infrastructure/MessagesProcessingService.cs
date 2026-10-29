using Microsoft.Extensions.Hosting;

using Boovatech.Eventing;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public class MessagesProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public MessagesProcessingService(IServiceScopeFactory scopeFactory)
        {
            ArgumentNullException.ThrowIfNull(scopeFactory);
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<MessagesProcessor>();

            try
            {
                await processor.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host shutdown is expected; the scope is disposed after processing stops.
            }
        }
    }
}
