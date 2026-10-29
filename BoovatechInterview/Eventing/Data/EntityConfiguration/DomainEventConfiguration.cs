using Boovatech.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boovatech.Eventing.Data.EntityConfiguration;

public sealed class DomainEventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(e => e.EventId);
        builder.Property(e => e.EventId)
            .HasColumnType("varchar(20)");
        builder.Property(e => e.Tenant).IsRequired().HasMaxLength(128)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.Property(e => e.Kind).IsRequired().HasMaxLength(128);
        builder.Property(e => e.PocessedAt).HasColumnName("ProcessedAt");
        builder.Property(e => e.From).IsRequired().HasMaxLength(128)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.Property(e => e.To).IsRequired().HasMaxLength(128)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.HasIndex(e => new { e.OccuredAt, e.EventId });
    }
}
