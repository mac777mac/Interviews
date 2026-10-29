using Boovatech.Domain;
using Boovatech.Domain.Events;
using Boovatech.Eventing.EventValidation;
using System.Text.Json;

namespace Boovatech.Eventing.Infrastructure
{
    public class MessageEntryHandler : IMessageEntryHandler
    {
        private readonly IValidators eventValidator;
        private readonly IEventsRepository eventRespository;
        private readonly IDomainTransactionFactory transactionFactory;
        private readonly IDomainEventService domainEventService;

        public MessageEntryHandler(IValidators eventValidator, IEventsRepository eventRespository, IDomainTransactionFactory transactionFactory, IDomainEventService domainEventService)
        {
            this.eventValidator = eventValidator;
            this.eventRespository = eventRespository;
            this.transactionFactory = transactionFactory;
            this.domainEventService = domainEventService;
        }
        public async Task ProcessAsync(MessageEntry entry, MessageEntryProcessingContext context, CancellationToken cancellation)
        {
            if (String.IsNullOrWhiteSpace(entry.PayloadJson))
            {
                await context.RejectAsync("Empty message payload");
            }
            var @event = await TryDeserializeAsync(entry.PayloadJson);
            if(@event == null)
            {
                await context.RejectAsync("Cannot deserialize message payload");
            }

             var validationResult = await PreValidateEventAsync(@event);
            if (!validationResult.IsOk)
            {
                await context.RejectAsync(validationResult.ErrorMessage);
                return;
            }

            using (var transaction = await transactionFactory.CreateAsync())
            {
                var registered = await eventRespository.RegisterEventAsync(@event, transaction);
                if (!registered)
                {
                    await context.RejectAsync("Duplicate");
                    return;
                }

                var eventStatus = await domainEventService.HandleDomainEventAsync(@event, transaction);
                await eventRespository.MarkEventAsAsync(@event, eventStatus, transaction);

                transaction.Commit();
            }
        }

        private async Task<DomainEvent?> TryDeserializeAsync(string payloadJson)
        {
            try
            {
                return JsonSerializer.Deserialize<DomainEvent>(payloadJson);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        private async Task<ValidationResult> PreValidateEventAsync(DomainEvent @event)
        {
            return await eventValidator.ValidateEventAsync(@event);
        }
    }
}