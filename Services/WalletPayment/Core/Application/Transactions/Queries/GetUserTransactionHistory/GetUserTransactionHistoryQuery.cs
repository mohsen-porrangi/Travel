using BuildingBlocks.CQRS;
using WalletPayment.Application.Transactions.Queries.Common;

namespace WalletPayment.Application.Transactions.Queries.GetUserTransactionHistory;

public record GetUserTransactionHistoryQuery : IQuery<PaginatedList<TransactionDto>>
{
    public Guid UserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public TransactionFilter Filter { get; init; } = new();

    // Sorting
    public string SortBy { get; init; } = "TransactionDate";
    public bool SortDesc { get; init; } = true;
}