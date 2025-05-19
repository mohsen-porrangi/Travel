using Domain.Entities.Account;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configurations
{
    public class AccountBalanceSnapshotConfiguration : IEntityTypeConfiguration<AccountBalanceSnapshot>
    {
        public void Configure(EntityTypeBuilder<AccountBalanceSnapshot> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Balance)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(s => s.SnapshotDate)
                .IsRequired();

            builder.Property(s => s.Type)
                .IsRequired();

            // رابطه با AccountInfo
            builder.HasOne(s => s.Account)
                .WithMany()
                .HasForeignKey(s => s.AccountInfoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(e => e.DomainEvents);
        }
    }
}