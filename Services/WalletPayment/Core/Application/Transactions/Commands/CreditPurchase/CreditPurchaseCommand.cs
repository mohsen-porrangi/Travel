using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.CreditPurchase;

public record CreditPurchaseCommand(
    Guid UserId,
    decimal Amount,
    CurrencyCode Currency,
    string OrderId,
    string Description) : ICommand<CreditPurchaseResponse>;

public record CreditPurchaseResponse(
    Guid TransactionId,
    Guid WalletId,
    decimal Amount,
    decimal RemainingCredit,
    DateTime TransactionDate);