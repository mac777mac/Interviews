using Boovatech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Boovatech.Eventing.Data.EntityConfiguration;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.CodeName).IsRequired().HasMaxLength(128)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.Property(a => a.Type).HasConversion<int>().IsRequired();
        builder.Property(a => a.Tenant).IsRequired().HasMaxLength(128)
            .UseCollation("Latin1_General_100_CI_AS");
        builder.HasIndex(a => new { a.Tenant, a.CodeName }).IsUnique();
        builder.HasOne<Tenant>().WithMany()
            .HasForeignKey(a => a.Tenant).HasPrincipalKey(t => t.CodeName)
            .IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
