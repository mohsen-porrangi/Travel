using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Entities.Credit;

namespace WalletPayment.Infrastructure.Data.Configurations;

public class CreditHistoryConfiguration : IEntityTypeConfiguration<CreditHistory>
{
    public void Configure(EntityTypeBuilder<CreditHistory> builder)
    {
        builder.HasKey(ch => ch.Id);

        builder.Property(ch => ch.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ch => ch.Description)
            .HasMaxLength(500);

        builder.Ignore(e => e.DomainEvents);
    }
}