using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WalletPayment.Domain.Entities.Payment;

namespace WalletPayment.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.Authority)
            .HasMaxLength(100);

        builder.Property(p => p.ReferenceId)
            .HasMaxLength(100);

        builder.Property(p => p.CallbackUrl)
            .HasMaxLength(500);

        builder.Property(p => p.AdditionalData)
            .HasMaxLength(4000);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(500);

        builder.Property(p => p.RefundTrackingId)
            .HasMaxLength(100);

        // رابطه با تراکنش
        builder.HasOne(p => p.Transaction)
            .WithMany()
            .HasForeignKey(p => p.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}