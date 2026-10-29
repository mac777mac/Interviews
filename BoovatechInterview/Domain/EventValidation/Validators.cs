using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using System.Collections.Generic;

namespace Boovatech.Eventing.EventValidation
{
    public class Validators: IValidators
    {
        public Validators(ITenantsRepository tenantsRepository, IAccountsRespository accountsRespository)
        {
            this.tenantsRepository = tenantsRepository;
            this.accountsRespository = accountsRespository;
        }
        public async Task<ValidationResult> ValidateEventAsync(DomainEvent @event)
        {
            await foreach (var validation in ValidateEventInternalAsync(@event))
            {
                if (!validation.IsOk)
                {
                    return validation;
                }
            }
            return ValidationResult.Ok;
        }

        private async IAsyncEnumerable<ValidationResult> ValidateEventInternalAsync(DomainEvent @event)
        {
            yield return EventInPast(@event);
            yield return NonNegativeAmount(@event);
            yield return KnownEventKind(@event);

            var tenants = (await this.tenantsRepository.GetAllAsync()).Select(t => t.CodeName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            yield return KnownTenant(@event, tenants);

            var tenantAccounts = (await accountsRespository.GetAllAsync());
            var tenantAccountsOptimized = tenantAccounts
                .Select(kv => new
                    {
                        tenant = kv.Key,
                        accounts = kv.Value.Select(v => v.CodeName).ToHashSet(StringComparer.OrdinalIgnoreCase)
                    })
                .ToDictionary(o => o.tenant, o => o.accounts as ISet<string>, StringComparer.OrdinalIgnoreCase);

            yield return KnownFrom(@event, tenantAccountsOptimized);
            yield return KnownTo(@event, tenantAccountsOptimized);

            var currentTenant = @event.Tenant;
            var accounts = tenantAccounts[currentTenant];
            var fromAccount = accounts.Single(a => StringComparer.OrdinalIgnoreCase.Equals(a.CodeName, @event.From));
            var toAccount = accounts.Single(a => StringComparer.OrdinalIgnoreCase.Equals(a.CodeName, @event.To));

            yield return AlowedTransferDirection(@event, fromAccount.Type, toAccount.Type);
        }

        private static readonly ISet<(AccountType from, AccountType to)> AllowedTransfers
            = new HashSet<(AccountType from, AccountType to)> {
                (AccountType.Bank, AccountType.Vault),
                (AccountType.Vault, AccountType.Truck),
                (AccountType.Truck, AccountType.Atm),
                (AccountType.Atm, AccountType.Clients)
            };
        private readonly ITenantsRepository tenantsRepository;
        private readonly IAccountsRespository accountsRespository;

        public static ValidationResult EventInPast(DomainEvent @event) =>
            @event.OccuredAt >= DateTime.UtcNow
                ? ValidationResult.Error("Event occured in future")
                : ValidationResult.Ok;

        public static ValidationResult NonNegativeAmount(DomainEvent @event) =>
            @event.Amount >= 0 ? ValidationResult.Ok : ValidationResult.Error("Negative amount");

        public static ValidationResult KnownEventKind(DomainEvent @event) =>
            @event.Kind is not null && EventKind.All.Contains(@event.Kind)
                ? ValidationResult.Ok : ValidationResult.Error("Unknown event kind");

        public static ValidationResult KnownTenant(DomainEvent @event, ISet<string> knownTenants) =>
            knownTenants.Contains(@event.Tenant) ? ValidationResult.Ok : ValidationResult.Error("Unknown Tenant");

        public static ValidationResult KnownFrom(DomainEvent @event, IDictionary<string, ISet<string>> tenantAccounts) =>
            tenantAccounts.TryGetValue(@event.Tenant, out var knownAccounts) && knownAccounts.Contains(@event.From)
                ? ValidationResult.Ok : ValidationResult.Error("Unknown From");

        public static ValidationResult KnownTo(DomainEvent @event, IDictionary<string, ISet<string>> tenantAccounts) =>
            tenantAccounts.TryGetValue(@event.Tenant, out var knownAccounts) && knownAccounts.Contains(@event.To)
                ? ValidationResult.Ok : ValidationResult.Error("Unknown To");

        public static ValidationResult AlowedTransferDirection(DomainEvent @event, AccountType from, AccountType to) =>
            AllowedTransfers.Contains((from, to))
                ? ValidationResult.Ok : ValidationResult.Error("Not allowed transer direction");
    }
}
