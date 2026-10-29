using Boovatech.Domain.Entities;
using Boovatech.Eventing.EventValidation;
using Microsoft.EntityFrameworkCore;

namespace Boovatech.Eventing.Data;

public sealed class TenantsRepository : ITenantsRepository
{
    private readonly EventingDbContext context;

    public TenantsRepository(EventingDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        var tenants = await RepositoryCache<Tenant[]>.GetOrLoadAsync(
            () => context.Tenants.AsNoTracking().ToArrayAsync());
        return tenants.Select(t => new Tenant { Id = t.Id, CodeName = t.CodeName }).ToArray();
    }
}
