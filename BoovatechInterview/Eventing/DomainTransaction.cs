using Boovatech.Domain;
using Boovatech.Eventing.Data;

namespace Boovatech.Eventing
{
    public class DomainTransaction : IDomainTransaction
    {
        private bool hasBeenCommited;
        private EventingDbContext context;
        public DomainTransaction(EventingDbContext context)
        {
            this.context = context; 
            hasBeenCommited = false;

        }

        internal async Task BeginTransactionAsync()
        {
            await context.Database.BeginTransactionAsync();
        } 
        public IDomainTransactionContext Context => this.context;


        public async Task CommitAsync()
        {
            hasBeenCommited = true;
            await context.SaveChangesAsync();
            await context.Database.CommitTransactionAsync();
        }

        public void Dispose()
        {
            if (hasBeenCommited)
            {
                return;
            }
            context.Database.RollbackTransaction();
        }
    }
}
