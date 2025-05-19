using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Queries.GetRefundableTransaction;

public record GetRefundableTransactionQuery(Guid TransactionId) : IQuery<RefundableTransactionDto>;

public record RefundableTransactionDto
{
    public Guid TransactionId { get; init; }
    public Guid WalletId { get; init; }
    public Guid AccountId { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal AlreadyRefundedAmount { get; init; }
    public decimal RefundableAmount { get; init; }
    public CurrencyCode Currency { get; init; }
    public DateTime TransactionDate { get; init; }
    public string Description { get; init; }
    public bool IsFullyRefundable { get; init; }
    public bool HasPartialRefunds { get; init; }
    public List<RefundHistoryItemDto> RefundHistory { get; init; } = new();
}

public record RefundHistoryItemDto
{
    public Guid RefundTransactionId { get; init; }
    public decimal Amount { get; init; }
    public DateTime RefundDate { get; init; }
    public string Reason { get; init; }
}