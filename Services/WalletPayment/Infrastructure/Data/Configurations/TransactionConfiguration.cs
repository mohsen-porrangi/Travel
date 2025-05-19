using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Entities.Transaction;

namespace WalletPayment.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.PaymentReferenceId)
            .HasMaxLength(100);

        builder.Property(t => t.OrderId)
            .HasMaxLength(100);

        builder.Ignore(e => e.DomainEvents);
    }
}