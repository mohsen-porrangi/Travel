using Domain.Entities.Account;
using Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WalletPayment.Domain.Entities.Account;
using WalletPayment.Domain.Entities.BankAccount;
using WalletPayment.Domain.Entities.Transaction;


namespace WalletPayment.Application.Common.Contracts;

public interface IWalletDbContext
{
    DbSet<Domain.Entities.Wallet.Wallet> Wallets { get; }
    DbSet<CurrencyAccount> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<CurrencyAccountBalanceSnapshot> AccountBalanceSnapshots { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }
    DbSet<Domain.Entities.Payment.Payment> Payments { get; }
    DbSet<BankAccount> BankAccounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DatabaseFacade Database { get; }
}