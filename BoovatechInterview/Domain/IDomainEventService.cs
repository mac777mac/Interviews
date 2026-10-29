using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Domain
{
    public interface IDomainEventService
    {
        Task<EventProcessingStatus> HandleDomainEventAsync(DomainEvent @event, IDomainTransaction trx);
    }
}
