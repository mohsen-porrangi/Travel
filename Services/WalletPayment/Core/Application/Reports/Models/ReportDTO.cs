using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Reports.Models;
#region DTO Classes

/// <summary>
/// DTO برای نمایش ورودی‌های صورتحساب
/// </summary>
public record StatementEntryDto
{
    public DateTime Date { get; init; }
    public string Description { get; init; }
    public decimal Amount { get; init; }
    public TransactionDirection Direction { get; init; }
    public decimal RunningBalance { get; init; }
    public TransactionType Type { get; init; }
    public string Reference { get; init; }
}

/// <summary>
/// داده‌های صورتحساب حساب
/// </summary>
public record AccountStatementData
{
    public decimal OpeningBalance { get; init; }
    public decimal ClosingBalance { get; init; }
    public decimal TotalDeposits { get; init; }
    public decimal TotalWithdrawals { get; init; }
    public int TotalTransactions { get; init; }
    public DateTime StatementStartDate { get; init; }
    public DateTime StatementEndDate { get; init; }
    public CurrencyCode Currency { get; init; }
    public List<StatementEntryDto> Entries { get; init; } = new();
}

/// <summary>
/// DTO برای نمایش خلاصه حساب
/// </summary>
public record AccountSummaryDto
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; }
    public CurrencyCode Currency { get; init; }
    public decimal Balance { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// DTO برای نمایش خلاصه اعتبار
/// </summary>
public record CreditSummaryDto
{
    public decimal CreditLimit { get; init; }
    public decimal AvailableCredit { get; init; }
    public decimal UsedCredit { get; init; }
    public DateTime? DueDate { get; init; }
    public bool HasActiveCredit { get; init; }
    public bool IsOverdue { get; init; }
}

/// <summary>
/// DTO برای نمایش خلاصه تراکنش‌ها
/// </summary>
public record TransactionsSummaryDto
{
    public int TotalCount { get; init; }
    public int LastMonthCount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal LastMonthAmount { get; init; }
    public DateTime? LastTransactionDate { get; init; }
}

/// <summary>
/// DTO برای نمایش خلاصه کیف پول
/// </summary>
public record WalletSummaryDto
{
    public Guid WalletId { get; init; }
    public bool IsActive { get; init; }
    public int TotalAccounts { get; init; }
    public List<AccountSummaryDto> Accounts { get; init; } = new();
    public CreditSummaryDto Credit { get; init; }
    public TransactionsSummaryDto Transactions { get; init; }
    public decimal TotalBalance { get; init; }
    public CurrencyCode DisplayCurrency { get; init; }
}

#endregion
