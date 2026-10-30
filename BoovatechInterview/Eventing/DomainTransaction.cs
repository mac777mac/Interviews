using Boovatech.Domain;
using Boovatech.Eventing.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Boovatech.Eventing
{
    public class DomainTransaction : IDomainTransaction
    {
        private bool hasBeenCommited;
        private EventingDbContext context;
        private IDbContextTransaction trx;

        public DomainTransaction(EventingDbContext context)
        {
            this.context = context;
            hasBeenCommited = false;

        }

        internal async Task BeginTransactionAsync()
        {
            trx = await context.Database.BeginTransactionAsync();
        }
        public IDomainTransactionContext Context => this.context;


        public async Task CommitAsync()
        {
            hasBeenCommited = true;
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
            await context.Database.CommitTransactionAsync();
        }

        public void Dispose()
        {
            try
            {
                if (!hasBeenCommited && trx != null)
                {
                    trx.Rollback();
                }
            }
            finally
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (trx != null)
            {
                trx.Dispose();
            }
            context.Dispose();
        }
    }
}
