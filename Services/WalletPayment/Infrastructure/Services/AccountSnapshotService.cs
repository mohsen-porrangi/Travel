using Application.Common.Contracts;
using Domain.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace Infrastructure.Services
{
    public class AccountSnapshotService : IAccountSnapshotService
    {
        private readonly IWalletDbContext _dbContext;
        private readonly ILogger<AccountSnapshotService> _logger;

        public AccountSnapshotService(IWalletDbContext dbContext, ILogger<AccountSnapshotService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task CreateSnapshotAsync(Guid accountId, SnapshotType type, CancellationToken cancellationToken = default)
        {
            var account = await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.IsActive, cancellationToken);

            if (account == null)
            {
                _logger.LogWarning("تلاش برای ایجاد اسنپ‌شات حساب ناموجود یا غیرفعال: {AccountId}", accountId);
                return;
            }

            var snapshot = new CurrencyAccountBalanceSnapshot(accountId, account.Balance, type);
            await _dbContext.AccountBalanceSnapshots.AddAsync(snapshot, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "اسنپ‌شات با موفقیت برای حساب {AccountId} ایجاد شد. موجودی: {Balance}, نوع: {Type}",
                accountId, account.Balance, type);
        }

        public async Task CreateSnapshotsForAllAccountsAsync(SnapshotType type, CancellationToken cancellationToken = default)
        {
            var activeAccounts = await _dbContext.Accounts
                .Where(a => a.IsActive && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            var snapshots = activeAccounts.Select(account =>
                new CurrencyAccountBalanceSnapshot(account.Id, account.Balance, type)).ToList();

            await _dbContext.AccountBalanceSnapshots.AddRangeAsync(snapshots, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "اسنپ‌شات با موفقیت برای {Count} حساب فعال ایجاد شد. نوع: {Type}",
                activeAccounts.Count, type);
        }

        public async Task<IEnumerable<CurrencyAccountBalanceSnapshot>> GetAccountSnapshotsAsync(
            Guid accountId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            SnapshotType? type = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AccountBalanceSnapshots
                .Where(s => s.AccountInfoId == accountId && !s.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(s => s.SnapshotDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SnapshotDate <= endDate.Value);

            if (type.HasValue)
                query = query.Where(s => s.Type == type.Value);

            return await query
                .OrderByDescending(s => s.SnapshotDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<CurrencyAccountBalanceSnapshot?> GetLastSnapshotAsync(
            Guid accountId,
            SnapshotType? type = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.AccountBalanceSnapshots
                .Where(s => s.AccountInfoId == accountId && !s.IsDeleted);

            if (type.HasValue)
                query = query.Where(s => s.Type == type.Value);

            return await query
                .OrderByDescending(s => s.SnapshotDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
