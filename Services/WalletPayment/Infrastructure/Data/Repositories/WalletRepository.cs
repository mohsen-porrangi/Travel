using Microsoft.EntityFrameworkCore;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Transaction;
using WalletPayment.Domain.Entities.Wallet;
using WalletPayment.Infrastructure.Data.Context;

namespace Infrastructure.Data.Repositories;
public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _dbContext;

    public WalletRepository(WalletDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .Include(w => w.Accounts)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .Include(w => w.Accounts)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .AnyAsync(w => w.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
    }

    public void Update(Wallet wallet)
    {
        _dbContext.Wallets.Update(wallet);
    }
    public async Task<Wallet?> GetByUserIdWithCreditHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .Include(w => w.Accounts)
            .Include(w => w.CreditHistory)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
    }

    public async Task AddTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
    }
}
