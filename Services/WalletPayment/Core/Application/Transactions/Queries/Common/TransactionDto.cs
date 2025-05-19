using BuildingBlocks.Contracts.Services;
using System.Transactions;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Queries.Common;

public record TransactionDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public Guid AccountId { get; init; }
    public Guid? RelatedTransactionId { get; init; }
    public decimal Amount { get; init; }
    public TransactionDirection Direction { get; init; }
    public Domain.Entities.Enums.TransactionType Type { get; init; }
    public Domain.Entities.Enums.TransactionStatus Status { get; init; }
    public DateTime TransactionDate { get; init; }
    public CurrencyCode Currency { get; init; }
    public string Description { get; init; }
    public bool IsCredit { get; init; }
    public DateTime? DueDate { get; init; }
    public string PaymentReferenceId { get; init; }
    public string OrderId { get; init; }
}