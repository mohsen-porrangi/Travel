
using WalletPayment.Domain.Entities.Transaction;

namespace WalletPayment.Application.Common.Contracts;

public interface IWalletRepository
{
    // متدهای قبلی
    Task<WalletPayment.Domain.Entities.Wallet.Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Wallet.Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Wallet.Wallet wallet, CancellationToken cancellationToken = default);
    void Update(Domain.Entities.Wallet.Wallet wallet);

    // متدهای جدید
    Task<Domain.Entities.Wallet.Wallet?> GetByUserIdWithCreditHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
}