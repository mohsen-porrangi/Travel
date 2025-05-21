using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Entities.BankAccount;

namespace WalletPayment.Infrastructure.Data.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BankName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.AccountNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(b => b.CardNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.ShabaNumber)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(b => b.AccountHolderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Ignore(e => e.DomainEvents);
    }
}