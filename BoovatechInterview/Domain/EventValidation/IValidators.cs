using Boovatech.Domain.Events;

namespace Boovatech.Eventing.EventValidation
{
    public interface IValidators
    {
        Task<ValidationResult> ValidateEventAsync(DomainEvent @event);
    }
}
