using Boovatech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boovatech.Eventing.Data.EntityConfiguration;

public sealed class AccountJournalEntryConfiguration : IEntityTypeConfiguration<AccountJournalEntry>
{
    public void Configure(EntityTypeBuilder<AccountJournalEntry> builder)
    {
        builder.ToTable("AccountJournalEntries");
        builder.HasKey(e => e.EntryId);
        builder.Property(e => e.EntryId).ValueGeneratedOnAdd();
        builder.Property(e => e.AccountCodeName).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Tenant).IsRequired().HasMaxLength(128);
        builder.Property(e => e.Amount).IsRequired();
        builder.Property(e => e.OccuredAt).IsRequired();
        builder.Property(e => e.EventId).IsRequired().HasColumnType("varchar(20)");

        builder.HasIndex(e => new { e.Tenant, e.AccountCodeName });
    }
}
