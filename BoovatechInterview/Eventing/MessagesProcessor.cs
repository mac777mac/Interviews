using Boovatech.Eventing.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Boovatech.Eventing
{
    public class MessagesProcessor
    {
        private readonly IInboxMessagesStore store;
        private readonly Func<IMessageEntryHandler> handlerFactory;
        private readonly ILogger logger;

        public MessagesProcessor(IInboxMessagesStore store, Func<IMessageEntryHandler> handlerFactory, ILogger logger)
        {
            this.store = store;
            this.handlerFactory = handlerFactory;
            this.logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                var prefetchedEntries = await store.PrefetchNextMessagesAsync();
                var waitTime = WaitTime(prefetchedEntries.Count());

                foreach (var prefetchedEntry in prefetchedEntries)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }

                    var lockedMessage = await store.TryLockMessageAsync(prefetchedEntry);
                    if (lockedMessage == null)
                    {
                        //...messsage has been aquired by someone else
                        continue;
                    }

                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }

                    var handler = handlerFactory();
                    var processingContext = new MessageEntryProcessingContext(
                        ack: () => MarkAsProcessedAsync(lockedMessage, true),
                        reject: reason => MarkAsProcessedAsync(lockedMessage, false, reason));
                    try
                    {
                        await handler.ProcessAsync(lockedMessage, processingContext, cancellation);
                        if (!processingContext.HasBeenMarked)
                        {
                            await processingContext.AckAsync();
                        }
                    }
                    catch (Exception ex)
                    {

                        logger.LogError(ex, "Error has been raised during processing the message with EntryId = ${Id}", lockedMessage.EntryId);
                    }
                }
                await Task.Delay(waitTime, cancellation);
            }
        }


        private async Task MarkAsProcessedAsync(MessageEntry entry, bool success, string? reason = null)
        {
            if (!await store.TryMarkAsProcessedAsync(entry, success, reason))
            {
                throw new InvalidOperationException(
                    $"Cannot complete message {entry.EntryId}: the lease expired or ownership changed.");
            }
        }

        private TimeSpan WaitTime(int numberOfArrivals)
        {
            return numberOfArrivals == 0 ? TimeSpan.FromMicroseconds(5) : TimeSpan.Zero; 
        } 
    }


}
