using Boovatech.Eventing.Data;
using Microsoft.EntityFrameworkCore;

namespace Boovatech.Eventing.Infrastructure;

public sealed class InboxMessagesStore : IInboxMessagesStore
{
    private readonly Func<EventingDbContext> contextFactory;
    private readonly int leaseSeconds;
    private readonly int retriesCount;

    public InboxMessagesStore(Func<EventingDbContext> contextFactory, TimeSpan leaseDuration,
        int retriesCount = 3)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);
        // PeekTimes includes the initial attempt and must fit in ushort.
        if (retriesCount < 0 || retriesCount >= ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(retriesCount));
        this.retriesCount = retriesCount;
        this.contextFactory = contextFactory;
        var seconds = leaseDuration.TotalSeconds;
        if (seconds <= 0 || seconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        leaseSeconds = (int)seconds;
    }

    public async Task<IEnumerable<MessageEntry>> PrefetchNextMessagesAsync(ushort count = 1)
    {
        if (count == 0)
            return Array.Empty<MessageEntry>();

        await using var context = contextFactory();
        // Prefetching does not acquire ownership. TryLockMessageAsync arbitrates competing workers.
        return await context.InboxMessages
            .AsNoTracking()
            .Where(entry => entry.ProcessedAt == null && entry.PeekTimes <= retriesCount &&
                (entry.PeekExpAt == null || entry.PeekExpAt <= DateTimeOffset.UtcNow))
            .OrderBy(entry => entry.EntryId)
            .Take(count)
            .ToListAsync();
    }

    public async Task<MessageEntry?> TryLockMessageAsync(MessageEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entry.Token);

        await using var context = contextFactory();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var affected = await context.InboxMessages
            .Where(message => message.EntryId == entry.EntryId && message.Token == entry.Token &&
                message.ProcessedAt == null && message.PeekTimes <= retriesCount &&
                (message.PeekExpAt == null || message.PeekExpAt <= DateTimeOffset.UtcNow))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(message => message.PeekExpAt,
                    message => (DateTimeOffset?)DateTimeOffset.UtcNow.AddSeconds(leaseSeconds))
                .SetProperty(message => message.PeekTimes,
                    message => message.PeekTimes < ushort.MaxValue
                        ? (ushort)(message.PeekTimes + 1) : message.PeekTimes));

        if (affected == 0)
            return null;

        // Keep the update lock until the new rowversion has been read.
        var claimed = await context.InboxMessages.AsNoTracking()
            .SingleAsync(message => message.EntryId == entry.EntryId);

        await transaction.CommitAsync();

        return claimed;
    }

    public async Task<bool> TryMarkAsProcessedAsync(MessageEntry entry, bool success, string? reason)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(entry.Token);

        await using var context = contextFactory();
        var affected = await context.InboxMessages
            .Where(message => message.EntryId == entry.EntryId && message.Token == entry.Token &&
                message.ProcessedAt == null && message.PeekExpAt > DateTimeOffset.UtcNow)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(message => message.ProcessedAt, message => (DateTimeOffset?)DateTimeOffset.UtcNow)
                .SetProperty(message => message.Success, (bool?)success)
                .SetProperty(message => message.Reason, reason)
                .SetProperty(message => message.PeekExpAt, (DateTimeOffset?)null));

        return affected == 1;
    }

}



