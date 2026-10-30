using Boovatech.Domain;
using Boovatech.Domain.Entities;

namespace Boovatech.Eventing.EventValidation
{
    public interface IAccountsRespository
    {
        Task<IDictionary<string, IEnumerable<Account>>> GetAllAsync();
        Task<Account> GetWithLockAsync(string from, string tenant, IDomainTransaction trx);
        Task RegisterTransferAsync(int amount, Account fromAccount, Account toAccount, string eventId, IDomainTransaction trx);
    }
}
