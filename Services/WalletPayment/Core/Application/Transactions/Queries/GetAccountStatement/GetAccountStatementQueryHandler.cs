using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Entities.Transaction;

namespace WalletPayment.Application.Transactions.Queries.GetAccountStatement;

public class GetAccountStatementQueryHandler(
    IWalletRepository walletRepository,
    IWalletDbContext dbContext)
    : IQueryHandler<GetAccountStatementQuery, AccountStatementResponse>
{
    public async Task<AccountStatementResponse> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        // بررسی وجود کیف پول
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        // یافتن حساب با ارز درخواستی
        var account = wallet.Accounts.FirstOrDefault(a => a.Currency == request.Currency);
        if (account == null)
            throw new NotFoundException($"حساب با ارز {request.Currency} برای کاربر یافت نشد", request.UserId);

        // محاسبه موجودی ابتدایی (قبل از شروع بازه گزارش)
        var openingBalanceTransactions = await dbContext.Transactions
            .Where(t => t.AccountInfoId == account.Id && t.TransactionDate < request.StartDate)
            .ToListAsync(cancellationToken);

        decimal openingBalance = CalculateBalance(openingBalanceTransactions);

        // دریافت تراکنش‌های بازه زمانی
        var periodTransactions = await dbContext.Transactions
            .Where(t => t.AccountInfoId == account.Id &&
                  t.TransactionDate >= request.StartDate &&
                  t.TransactionDate <= request.EndDate)
            .OrderBy(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        // محاسبه مقادیر خلاصه
        var totalDeposits = periodTransactions
            .Where(t => t.Direction == TransactionDirection.In)
            .Sum(t => t.Amount);

        var totalWithdrawals = periodTransactions
            .Where(t => t.Direction == TransactionDirection.Out)
            .Sum(t => t.Amount);

        decimal closingBalance = openingBalance + totalDeposits - totalWithdrawals;

        // ساخت ورودی‌های صورتحساب با موجودی لحظه‌ای
        decimal runningBalance = openingBalance;
        var entries = new List<StatementEntryDto>();

        foreach (var transaction in periodTransactions)
        {
            // بروزرسانی موجودی لحظه‌ای
            if (transaction.Direction == TransactionDirection.In)
                runningBalance += transaction.Amount;
            else
                runningBalance -= transaction.Amount;

            // ساخت ورودی صورتحساب
            entries.Add(new StatementEntryDto
            {
                Date = transaction.TransactionDate,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Direction = transaction.Direction,
                RunningBalance = runningBalance,
                Type = transaction.Type,
                Reference = transaction.PaymentReferenceId ?? transaction.OrderId ?? transaction.Id.ToString()
            });
        }

        // بازگشت پاسخ نهایی
        return new AccountStatementResponse
        {
            OpeningBalance = openingBalance,
            ClosingBalance = closingBalance,
            TotalDeposits = totalDeposits,
            TotalWithdrawals = totalWithdrawals,
            TotalTransactions = periodTransactions.Count,
            StatementStartDate = request.StartDate,
            StatementEndDate = request.EndDate,
            Currency = request.Currency,
            Entries = entries
        };
    }

    private static decimal CalculateBalance(IEnumerable<Transaction> transactions)
    {
        decimal balance = 0;

        foreach (var transaction in transactions)
        {
            if (transaction.Direction == TransactionDirection.In)
                balance += transaction.Amount;
            else
                balance -= transaction.Amount;
        }

        return balance;
    }
}