using Boovatech.Eventing.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boovatech.Eventing.Data.EntityConfiguration;

public sealed class MessageEntryConfiguration : IEntityTypeConfiguration<MessageEntry>
{
    public void Configure(EntityTypeBuilder<MessageEntry> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(e => e.EntryId);
        builder.Property(e => e.EntryId).HasConversion<decimal>().HasColumnType("decimal(20,0)")
            .ValueGeneratedOnAdd();
        builder.Property(e => e.PayloadJson).IsRequired();
        builder.Property(e => e.Token)
            .IsRowVersion()
            .IsConcurrencyToken();
        builder.Property(e => e.PeekTimes).HasConversion<int>();
        builder.HasIndex(e => new { e.PeekExpAt, e.EntryId })
            .HasFilter("[ProcessedAt] IS NULL");
    }
}
