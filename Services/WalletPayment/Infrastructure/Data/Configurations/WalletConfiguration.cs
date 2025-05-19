using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Domain.Entities.Wallet;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.CreditLimit)
            .HasPrecision(18, 2);
        builder.Property(w => w.CreditBalance)
            .HasPrecision(18, 2);
        builder.Ignore(e => e.DomainEvents);
    }
}