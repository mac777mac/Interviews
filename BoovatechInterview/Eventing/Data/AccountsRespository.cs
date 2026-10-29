using Boovatech.Domain;
using Boovatech.Domain.Entities;
using Boovatech.Eventing.EventValidation;
using Microsoft.EntityFrameworkCore;

namespace Boovatech.Eventing.Data;

public sealed class AccountsRespository : IAccountsRespository
{
    private readonly EventingDbContext context;

    public AccountsRespository(EventingDbContext context)
    {
        this.context = context;
    }

    public async Task<IDictionary<string, IEnumerable<Account>>> GetAllAsync()
    {
        var accounts = await RepositoryCache<Account[]>.GetOrLoadAsync(
            () => context.Accounts.AsNoTracking().ToArrayAsync());

        return accounts.GroupBy(a => a.Tenant, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IEnumerable<Account>)g.Select(a => new Account
            {
                Id = a.Id,
                CodeName = a.CodeName,
                Tenant = a.Tenant,
                Type = a.Type
            }).ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<Account> GetWithLockAsync(string account, string tenant, IDomainTransaction trx)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant);
        ArgumentNullException.ThrowIfNull(trx);

        var transactionContext = trx.Context.AsDbContext();

        return await transactionContext.Accounts
            .FromSqlInterpolated($"SELECT * FROM [Accounts] WITH (XLOCK, ROWLOCK) WHERE [CodeName] = {account} AND [Tenant] = {tenant}")
            .SingleAsync();
    }

    public Task RegisterTransferAsync(int amount, Account fromAccount, Account toAccount, string eventId,
        IDomainTransaction trx)
    {
        ArgumentNullException.ThrowIfNull(fromAccount);
        ArgumentNullException.ThrowIfNull(toAccount);
        ArgumentNullException.ThrowIfNull(trx);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventId);
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }
        if (!StringComparer.OrdinalIgnoreCase.Equals(fromAccount.Tenant, toAccount.Tenant))
        {
            throw new ArgumentException("Both accounts must belong to the same tenant.");
        }

        var transactionContext = trx.Context.AsDbContext();
        transactionContext.Entry(fromAccount).Property(account => account.Amount).IsModified = true;
        transactionContext.Entry(toAccount).Property(account => account.Amount).IsModified = true;

        var occurredAt = DateTimeOffset.UtcNow;
        // The caller has already adjusted the tracked account balances.
        transactionContext.AccountJournalEntries.AddRange(
            new AccountJournalEntry
            {
                AccountCodeName = fromAccount.CodeName,
                Tenant = fromAccount.Tenant,
                Amount = -amount,
                OccuredAt = occurredAt,
                EventId = eventId
            },
            new AccountJournalEntry
            {
                AccountCodeName = toAccount.CodeName,
                Tenant = toAccount.Tenant,
                Amount = amount,
                OccuredAt = occurredAt,
                EventId = eventId
            });

        return Task.CompletedTask;
    }
}
