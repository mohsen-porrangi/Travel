using Domain.Entities.Account;
using Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WalletPayment.Domain.Entities.Account;
using WalletPayment.Domain.Entities.Transaction;


namespace WalletPayment.Application.Common.Contracts;

public interface IWalletDbContext
{
    DbSet<Domain.Entities.Wallet.Wallet> Wallets { get; }
    DbSet<AccountInfo> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<AccountBalanceSnapshot> AccountBalanceSnapshots { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Domain.Entities.Payment.Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DatabaseFacade Database { get; }
}