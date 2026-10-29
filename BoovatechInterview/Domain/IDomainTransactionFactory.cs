namespace Boovatech.Domain
{
    public interface IDomainTransactionFactory
    {
        Task<IDomainTransaction> CreateAsync();
    }
}
