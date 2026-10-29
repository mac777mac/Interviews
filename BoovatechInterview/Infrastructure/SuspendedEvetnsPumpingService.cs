using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Boovatech.Eventing.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Boovatech.Infrastructure
{
    public class SuspendedEvetnsPumpingService : BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public SuspendedEvetnsPumpingService(IServiceScopeFactory scopeFactory)
        {
            ArgumentNullException.ThrowIfNull(scopeFactory);
            this.scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var pumper = scope.ServiceProvider.GetRequiredService<ISuspendedEventsPumper>();
                await pumper.PumpItAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host shutdown is expected.
            }
        }
    }
}
