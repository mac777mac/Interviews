namespace Boovatech.Domain
{
    public interface IDomainTransaction : IDisposable
    {
        IDomainTransactionContext Context {get;}

        Task CommitAsync();
    }

    public interface IDomainTransactionContext
    {
        
    }
}
