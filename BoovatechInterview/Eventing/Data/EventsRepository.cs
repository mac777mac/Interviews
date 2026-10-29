using Boovatech.Domain;
using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Boovatech.Eventing.Data
{
    public class EventsRepository : IEventsRepository
    {
        private readonly Func<EventingDbContext> contextFactory;

        public EventsRepository(Func<EventingDbContext> contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);
            this.contextFactory = contextFactory;
        }

        public async Task MarkEventAsAsync(DomainEvent @event, EventProcessingStatus eventStatus, IDomainTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(@event);
            ArgumentNullException.ThrowIfNull(transaction);
            if (!Enum.IsDefined(eventStatus))
                throw new ArgumentOutOfRangeException(nameof(eventStatus));

            var context = transaction.Context.AsDbContext();
            // Registration may have staged an insert that is not in the database yet.
            var trackedEvent = await context.Events.SingleAsync(e => e.EventId == @event.EventId);

            var entry = context.Entry(trackedEvent);

            if (eventStatus == EventProcessingStatus.Suspended)
                entry.Property(e => e.SuspendedAt).CurrentValue = DateTime.UtcNow;
            else if (eventStatus == EventProcessingStatus.Processed)
                entry.Property(e => e.PocessedAt).CurrentValue = DateTime.UtcNow;
        }

        public async Task<bool> RegisterEventAsync(DomainEvent @event, IDomainTransaction t)
        {
            ArgumentNullException.ThrowIfNull(@event);

            var context = t.Context.AsDbContext();
            // Hold an exclusive table lock until the duplicate check and insert finish.
            var exists = await context.Events
                .FromSqlInterpolated($"SELECT * FROM [Events] WITH (TABLOCKX, HOLDLOCK) WHERE [EventId] = {@event.EventId} AND Tenant = {@event.Tenant}")
                .AsNoTracking()
                .AnyAsync();

            if (exists)
            {
                return false;
            }

            context.Events.Add(@event);

            return true;
        }

        public async Task<IEnumerable<DomainEvent>> FindSuspendedEventsCandidatesAsync()
        {
            await using var context = contextFactory();

            return await context.Events
                .FromSqlRaw("""
                    SELECT e.*
                    FROM Accounts acc
                    CROSS APPLY (
                        SELECT TOP (1) ei.*
                        FROM Events ei
                        WHERE ei.Tenant = acc.Tenant
                          AND ei.[From] = acc.CodeName
                          AND ei.ProcessedAt IS NULL
                          AND acc.Amount > ei.Amount
                        ORDER BY ei.OccuredAt ASC, ei.EventId ASC
                    ) e
                    """)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DomainEvent?> FindSuspendedEventCandidatesAsync(string account, string tenant)
        {
            await using var context = contextFactory();

            return await context.Events
                .FromSql($"""
                    SELECT e.*
                    FROM Accounts acc
                    CROSS APPLY (
                        SELECT TOP (1) ei.*
                        FROM Events ei
                        WHERE ei.Tenant = acc.Tenant
                          AND ei.[From] = acc.CodeName
                          AND ei.ProcessedAt IS NULL
                          AND acc.Amount > ei.Amount
                        ORDER BY ei.OccuredAt ASC, ei.EventId ASC
                    ) e
                    WHERE acc.CodeName = ${account} AND acc.Tenant = {tenant}
                    """)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<DomainEvent?> FindAndLockAsync(string eventId, string tenant, IDomainTransaction trx)
        {
            var context = trx.Context.AsDbContext();

            var @event = await context.Events
                .FromSqlInterpolated($"SELECT * FROM [Events] WITH (ROWLOCK, HOLDLOCK) WHERE [EventId] = {eventId} AND Tenant = {tenant}")
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return @event;
        }
    }
}
