using Application.Common.Contracts;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Domain.Entities.Account;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Accounts.Queries.GetAccountBalanceHistory;

public class GetAccountBalanceHistoryQueryHandler(
    IWalletDbContext dbContext,
    IAccountSnapshotService snapshotService)
    : IQueryHandler<GetAccountBalanceHistoryQuery, AccountBalanceHistoryResponse>
{
    public async Task<AccountBalanceHistoryResponse> Handle(GetAccountBalanceHistoryQuery request, CancellationToken cancellationToken)
    {
        // دریافت اطلاعات حساب
        var currencyAccount = await dbContext.CurrencyAccount
            .FirstOrDefaultAsync(a => a.Id == request.CurrencyAccountId, cancellationToken);

        if (currencyAccount == null)
            throw new NotFoundException("حساب مورد نظر یافت نشد", request.CurrencyAccountId);

        // دریافت اسنپ‌شات‌های حساب
        var snapshots = await snapshotService.GetAccountSnapshotsAsync(
            request.CurrencyAccountId,
            request.StartDate,
            request.EndDate,
            request.SnapshotType,
            cancellationToken);

        var snapshotsList = snapshots.OrderBy(s => s.SnapshotDate).ToList();

        // محاسبه تغییرات
        var historyItems = new List<BalanceHistoryItem>();
        decimal? previousBalance = null;

        foreach (var snapshot in snapshotsList)
        {
            if (request.Detailed)
            {
                // محاسبات کامل برای حالت detailed
                decimal change = 0;
                decimal changePercentage = 0;

                if (previousBalance.HasValue)
                {
                    change = snapshot.Balance - previousBalance.Value;
                    changePercentage = previousBalance.Value != 0
                        ? (change / previousBalance.Value) * 100
                        : 0;
                }

                historyItems.Add(new BalanceHistoryItem
                {
                    Date = snapshot.SnapshotDate,
                    Balance = snapshot.Balance,
                    Type = snapshot.Type.ToString(),
                    Change = change,
                    ChangePercentage = Math.Round(changePercentage, 2)
                });
            }
            else
            {
                // نسخه ساده برای حالت غیر detailed (مشابه endpoint /snapshots قبلی)
                historyItems.Add(new BalanceHistoryItem
                {
                    Date = snapshot.SnapshotDate,
                    Balance = snapshot.Balance,
                    Type = snapshot.Type.ToString(),
                    Change = 0,
                    ChangePercentage = 0
                });
            }

            previousBalance = snapshot.Balance;
        }

        return new AccountBalanceHistoryResponse
        {
            CurrencyAccountId = currencyAccount.Id,           
            Currency = currencyAccount.Currency.ToString(),
            CurrentBalance = currencyAccount.Balance,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            HistoryItems = historyItems
        };
    }
}