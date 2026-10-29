namespace Boovatech.Domain.Events
{
    public record DomainEvent(string EventId, string Tenant, string Kind, string From, string To, int Amount,
                              DateTime OccuredAt, DateTime? SuspendedAt, DateTime? PocessedAt);


}
