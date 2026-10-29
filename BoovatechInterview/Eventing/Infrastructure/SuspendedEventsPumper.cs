using Boovatech.Domain;
using Boovatech.Domain.SuspendedEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Eventing.Infrastructure
{
    public interface ISuspendedEventsPumper
    {
        Task PumpItAsync(CancellationToken cancellation);
    }

    public class SuspendedEventsPumper : ISuspendedEventsPumper
    {
        private readonly IEventsRepository eventsRepository;
        private readonly SuspendedEventPublisher publisher;

        public SuspendedEventsPumper(IEventsRepository eventsRepository, SuspendedEventPublisher publisher)
        {
            this.eventsRepository = eventsRepository;
            this.publisher = publisher;
        }
        public async Task PumpItAsync(CancellationToken cancellation)
        {
            var events = await eventsRepository.FindSuspendedEventsCandidatesAsync();
            foreach (var @event in events)
            {
                await publisher.PostAccountDebitedAsync(@event.From, @event.Tenant);
            }
        }
    }
}
