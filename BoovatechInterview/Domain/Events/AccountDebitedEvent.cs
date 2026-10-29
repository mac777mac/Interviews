namespace Boovatech.Domain.Events
{
    public record AccountDebitedEvent(string Account, string Tenant);
}
