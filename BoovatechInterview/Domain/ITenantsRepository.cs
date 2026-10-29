using Boovatech.Domain.Entities;

namespace Boovatech.Eventing.EventValidation
{
    public interface ITenantsRepository
    {
        Task<IEnumerable<Tenant>> GetAllAsync();
    }
}
