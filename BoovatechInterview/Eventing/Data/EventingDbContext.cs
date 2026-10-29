using Boovatech.Domain;
using Boovatech.Domain.Entities;
using Boovatech.Domain.Events;
using Boovatech.Eventing.EventValidation;
using Boovatech.Eventing.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Boovatech.Eventing.Data;

public sealed class EventingDbContext : DbContext, IDomainTransactionContext
{
    public EventingDbContext(DbContextOptions<EventingDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<MessageEntry> InboxMessages => Set<MessageEntry>();
    public DbSet<DomainEvent> Events => Set<DomainEvent>();
    public DbSet<AccountJournalEntry> AccountJournalEntries => Set<AccountJournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventingDbContext).Assembly);
    }
}
