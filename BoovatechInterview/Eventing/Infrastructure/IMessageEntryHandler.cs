namespace Boovatech.Eventing.Infrastructure
{
    public interface IMessageEntryHandler
    {
        Task ProcessAsync(MessageEntry entry, MessageEntryProcessingContext context, CancellationToken cancellation);

    }


}