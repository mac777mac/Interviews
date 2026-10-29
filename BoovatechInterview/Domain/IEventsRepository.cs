using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Domain
{
    public interface IEventsRepository
    {
        Task MarkEventAsAsync(DomainEvent @event, EventProcessingStatus eventStatus, IDomainTransaction transaction);

        /// <summary>Registers a pending event. Returns false if its EventId is already registered.</summary>
        Task<bool> RegisterEventAsync(DomainEvent @event, IDomainTransaction transaction);

        Task<IEnumerable<DomainEvent>> FindSuspendedEventsCandidatesAsync();
        Task<DomainEvent?> FindSuspendedEventCandidatesAsync(string account, string tenant);
        Task<DomainEvent?> FindAndLockAsync(string eventId, string tenant, IDomainTransaction trx);
    }
}
