namespace Boovatech.Domain.SuspendedEvents;

public interface ISuspendedEventPublisher
{
    Task PostAccountDebitedAsync(string account, string tenant);
}
