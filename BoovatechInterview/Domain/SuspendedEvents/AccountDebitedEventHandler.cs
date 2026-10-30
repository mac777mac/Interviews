using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boovatech.Domain.SuspendedEvents
{
    public class AccountDebitedEventHandler
    {
        private readonly IEventsRepository eventsRepository;
        private readonly IDomainTransactionFactory domainTransactionFactory;
        private readonly IDomainEventService eventsService;
        private readonly ISuspendedEventPublisher eventPublisher;
        private readonly ILogger logger;

        public AccountDebitedEventHandler(IEventsRepository eventsRepository, IDomainTransactionFactory domainTransactionFactory, IDomainEventService eventsService, ISuspendedEventPublisher eventPublisher, ILogger logger)
        {
            this.eventsRepository = eventsRepository;
            this.domainTransactionFactory = domainTransactionFactory;
            this.eventsService = eventsService;
            this.eventPublisher = eventPublisher;
            this.logger = logger;
        }
        public async Task HandleEventAsync(AccountDebitedEvent @event)
        {
            try
            {
                var candidateEvent = await eventsRepository.FindSuspendedEventCandidatesAsync(@event.Account, @event.Tenant);
                if (candidateEvent == null) {
                    logger.LogInformation("Suspended event candidate was ot found for Account={account} Tenant={tenant}", @event.Account, @event.Tenant);
                    return;
                }
                using var trx = await domainTransactionFactory.CreateAsync();
                
                var eventToProcess = await eventsRepository.FindAndLockAsync(candidateEvent.EventId, candidateEvent.Tenant, trx);
                if (eventToProcess == null)
                {
                    logger.LogInformation("Cannot lock Suspended event EventId = {eventId} Tenant={tenant}", candidateEvent.EventId, @event.Tenant);
                    return;
                }
                if (eventToProcess.PocessedAt.HasValue)
                {
                    logger.LogInformation("Attempted event has already been processed EventId = {eventId} Tenant={tenant}", candidateEvent.EventId, @event.Tenant);
                    return;
                }

                var result = await eventsService.HandleDomainEventAsync(eventToProcess, trx);

                if (result == Entities.EventProcessingStatus.Processed)
                {
                    await eventsRepository.MarkEventAsAsync(eventToProcess, result, trx);
                    await trx.CommitAsync();
                    await eventPublisher.PostAccountDebitedAsync(eventToProcess.To, eventToProcess.Tenant);
                }

            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Exception has been thrown during suscpnded event processing EventId = {eventId}, Tenant={tenant}", @event.Account, @event.Tenant);
            }
        }
    }
}
