using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.ProcessTransaction;

public record ProcessWalletTransactionCommand(
    Guid UserId,
    decimal Amount,
    CurrencyCode Currency,
    TransactionDirection Direction,
    string? ReferenceId,
    string? OrderId,
    string Description) : ICommand<WalletTransactionResponse>;

public record WalletTransactionResponse
{
    public Guid TransactionId { get; init; }
    public Guid WalletId { get; init; }
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
    public TransactionDirection Direction { get; init; }
    public DateTime TransactionDate { get; init; }
}