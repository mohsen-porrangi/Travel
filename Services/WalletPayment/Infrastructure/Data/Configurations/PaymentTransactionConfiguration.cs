using Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WalletPayment.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.GatewayToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ReferenceId)
            .HasMaxLength(100);

        builder.Property(p => p.GatewayResponse)
            .HasMaxLength(2000);

        builder.Property(p => p.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .HasMaxLength(100);

        builder.Ignore(e => e.DomainEvents);
    }
}