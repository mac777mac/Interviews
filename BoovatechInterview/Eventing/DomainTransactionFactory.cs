using Boovatech.Domain;
using Boovatech.Eventing.Data;

namespace Boovatech.Eventing
{
    public class DomainTransactionFactory : IDomainTransactionFactory
    {
        private readonly Func<EventingDbContext> dbContextFactory;

        public DomainTransactionFactory(Func<EventingDbContext> dbContextFactory) {
            this.dbContextFactory = dbContextFactory;
        }
        public async Task<IDomainTransaction> CreateAsync()
        {
            var trx = new DomainTransaction(dbContextFactory());
            await trx.BeginTransactionAsync();
            return trx;
        }
    }
}
