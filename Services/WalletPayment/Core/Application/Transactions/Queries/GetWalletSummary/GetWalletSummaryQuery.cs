using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Wallet.Queries.GetWalletSummary;

public record GetWalletSummaryQuery(Guid UserId) : IQuery<WalletSummaryResponse>;

public record WalletSummaryResponse
{
    public Guid WalletId { get; init; }
    public bool IsActive { get; init; }
    public int TotalAccounts { get; init; }
    public List<AccountSummaryDto> Accounts { get; init; } = new();
    public CreditSummaryDto Credit { get; init; }
    public TransactionsSummaryDto Transactions { get; init; }
}

public record AccountSummaryDto
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; }
    public CurrencyCode Currency { get; init; }
    public decimal Balance { get; init; }
    public bool IsActive { get; init; }
}

public record CreditSummaryDto
{
    public decimal CreditLimit { get; init; }
    public decimal AvailableCredit { get; init; }
    public decimal UsedCredit { get; init; }
    public DateTime? DueDate { get; init; }
    public bool HasActiveCredit { get; init; }
    public bool IsOverdue { get; init; }
}

public record TransactionsSummaryDto
{
    public int TotalCount { get; init; }
    public int LastMonthCount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal LastMonthAmount { get; init; }
    public DateTime? LastTransactionDate { get; init; }
}