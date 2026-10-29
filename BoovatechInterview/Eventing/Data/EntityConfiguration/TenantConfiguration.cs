using Boovatech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boovatech.Eventing.Data.EntityConfiguration;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.CodeName).IsRequired().HasMaxLength(128)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.HasAlternateKey(t => t.CodeName);
    }
}
