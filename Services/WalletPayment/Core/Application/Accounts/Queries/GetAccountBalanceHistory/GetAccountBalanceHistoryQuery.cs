using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Accounts.Queries.GetAccountBalanceHistory;

public record GetAccountBalanceHistoryQuery(
    Guid AccountId,
    DateTime StartDate,
    DateTime EndDate,
    SnapshotType? SnapshotType) : IQuery<AccountBalanceHistoryResponse>;

public record AccountBalanceHistoryResponse
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; }
    public string Currency { get; init; }
    public decimal CurrentBalance { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public List<BalanceHistoryItem> HistoryItems { get; init; } = new();
}

public record BalanceHistoryItem
{
    public DateTime Date { get; init; }
    public decimal Balance { get; init; }
    public string Type { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePercentage { get; init; }
}