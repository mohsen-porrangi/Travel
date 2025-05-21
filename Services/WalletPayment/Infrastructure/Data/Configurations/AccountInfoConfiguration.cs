using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Domain.Entities.Account;

public class AccountInfoConfiguration : IEntityTypeConfiguration<CurrencyAccount>
{
    public void Configure(EntityTypeBuilder<CurrencyAccount> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Balance)
            .HasPrecision(18, 2);
        builder.Property(a => a.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();
        builder.Ignore(e => e.DomainEvents);
    }
}