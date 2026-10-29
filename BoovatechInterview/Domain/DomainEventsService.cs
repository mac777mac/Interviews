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
        private readonly ISuspendedEventPublisher eventPublisher;

        public DomainEventsService(IAccountsRespository accountsRespository, ISuspendedEventPublisher eventPublisher)
        {
            this.accountsRespository = accountsRespository;
            this.eventPublisher = eventPublisher;
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

            await PostAccountDebitedEvent(@event.To, @event.Tenant);

            return EventProcessingStatus.Processed;
        }

        /// <summary>
        /// Posting event about chnage in the Amount of debited account so suspended events could be taken into consideration 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private Task PostAccountDebitedEvent(string account, string tenant) => eventPublisher.PostAccountDebitedAsync(account, tenant);
    }
}
