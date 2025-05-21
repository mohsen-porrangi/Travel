using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Wallet.Queries.GetWalletSummary;

public class GetWalletSummaryQueryHandler(
    IWalletRepository walletRepository,
    IWalletDbContext dbContext)
    : IQueryHandler<GetWalletSummaryQuery, WalletSummaryResponse>
{
    public async Task<WalletSummaryResponse> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        // بررسی وجود کیف پول
        var wallet = await walletRepository.GetByUserIdWithCreditHistoryAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // بررسی وضعیت سررسید اعتبار
        wallet.CheckCreditDueDate();

        // خلاصه حساب‌ها
        var accountSummaries = wallet.CurrencyAccount
            .Where(a => !a.IsDeleted)
            .Select(a => new AccountSummaryDto
            {
                AccountId = a.Id,
                CurrencyAccountId = a.Id,
                Currency = a.Currency,
                Balance = a.Balance,
                IsActive = a.IsActive
            })
            .ToList();

        // خلاصه اعتبار
        bool hasCredit = wallet.CreditLimit > 0 && wallet.CreditDueDate.HasValue;
        bool isOverdue = hasCredit && wallet.CreditDueDate.Value < DateTime.UtcNow;

        var creditSummary = new CreditSummaryDto
        {
            CreditLimit = wallet.CreditLimit,
            AvailableCredit = wallet.CreditBalance,
            UsedCredit = wallet.CreditLimit - wallet.CreditBalance,
            DueDate = wallet.CreditDueDate,
            HasActiveCredit = hasCredit,
            IsOverdue = isOverdue
        };

        // آمار تراکنش‌ها
        var lastMonth = DateTime.UtcNow.AddMonths(-1);

        // استفاده از اجرای یک کوئری واحد برای کاهش تعداد درخواست‌ها به دیتابیس
        var transactionStats = await dbContext.Transactions
            .Where(t => t.WalletId == wallet.Id)
            .GroupBy(t => 1) // گروه‌بندی همه با هم
            .Select(g => new
            {
                TotalCount = g.Count(),
                LastMonthCount = g.Count(t => t.TransactionDate >= lastMonth),
                TotalAmount = g.Sum(t => t.Amount),
                LastMonthAmount = g.Sum(t => t.TransactionDate >= lastMonth ? t.Amount : 0),
                LastTransactionDate = g.Max(t => t.TransactionDate)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var transactionSummary = new TransactionsSummaryDto
        {
            TotalCount = transactionStats?.TotalCount ?? 0,
            LastMonthCount = transactionStats?.LastMonthCount ?? 0,
            TotalAmount = transactionStats?.TotalAmount ?? 0,
            LastMonthAmount = transactionStats?.LastMonthAmount ?? 0,
            LastTransactionDate = transactionStats?.LastTransactionDate
        };

        // بازگشت پاسخ نهایی
        return new WalletSummaryResponse
        {
            WalletId = wallet.Id,
            IsActive = wallet.IsActive,
            TotalAccounts = accountSummaries.Count,
            Accounts = accountSummaries,
            Credit = creditSummary,
            Transactions = transactionSummary
        };
    }
}