using Boovatech.Domain;
using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using Boovatech.Domain.SuspendedEvents;
using Boovatech.Eventing.EventValidation;

namespace Boovatech.Eventing
{

    public class DomainEventsService: IDomainEventService
    {
        private readonly IAccountsRespository accountsRespository;

        public DomainEventsService(IAccountsRespository accountsRespository)
        {
            this.accountsRespository = accountsRespository;
        }
        public async Task<EventProcessingStatus> HandleDomainEventAsync(DomainEvent @event, IDomainTransaction trx)
        {
            var fromAccount = await accountsRespository.GetWithLockAsync(@event.From, @event.Tenant, trx);
            if (fromAccount.IsPhysical && (fromAccount.Amount - @event.Amount) < 0)
            {
                return EventProcessingStatus.Suspended;
            }

            var toAccount = await accountsRespository.GetWithLockAsync(@event.To, @event.Tenant, trx);
            fromAccount.Amount -= @event.Amount;
            toAccount.Amount += @event.Amount;

            await accountsRespository.RegisterTransferAsync(@event.Amount, fromAccount, toAccount, @event.EventId, trx);

            return EventProcessingStatus.Processed;
        }
    }
}
