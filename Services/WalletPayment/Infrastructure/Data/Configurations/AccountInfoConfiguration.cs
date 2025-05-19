using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Domain.Entities.Account;

public class AccountInfoConfiguration : IEntityTypeConfiguration<AccountInfo>
{
    public void Configure(EntityTypeBuilder<AccountInfo> builder)
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