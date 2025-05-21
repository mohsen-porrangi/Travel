using BuildingBlocks.Contracts;
using BuildingBlocks.Extensions;
using Domain.Entities.Account;
using Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Account;
using WalletPayment.Domain.Entities.BankAccount;
using WalletPayment.Domain.Entities.Payment;
using WalletPayment.Domain.Entities.Transaction;
using WalletPayment.Domain.Entities.Wallet;

namespace WalletPayment.Infrastructure.Data.Context;
public class WalletDbContext : DbContext, IWalletDbContext
{
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<CurrencyAccount> Accounts => Set<CurrencyAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CurrencyAccountBalanceSnapshot> AccountBalanceSnapshots => Set<CurrencyAccountBalanceSnapshot>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletDbContext).Assembly);

        // اعمال فیلتر Soft Delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType);
                var propertyMethodInfo = typeof(EF).GetMethod("Property").MakeGenericMethod(typeof(bool));
                var isDeletedProperty = Expression.Call(propertyMethodInfo, parameter, Expression.Constant("IsDeleted"));
                BinaryExpression compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                var deletedCheck = Expression.Lambda(Expression.Equal(Expression.Property(parameter, "IsDeleted"), Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(deletedCheck);
            }
        }
    }

    public override int SaveChanges()
    {
        ChangeTracker.SetAuditProperties();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.SetAuditProperties();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}