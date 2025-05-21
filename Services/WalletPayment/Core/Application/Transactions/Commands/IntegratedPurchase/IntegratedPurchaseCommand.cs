using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.IntegratedPurchase;

public record IntegratedPurchaseCommand(
    Guid UserId,
    decimal Amount,
    CurrencyCode Currency,
    string OrderId,
    string Description,
    string PaymentReferenceId,
    bool AutoConvertCurrency = true) // پارامتر جدید برای تبدیل خودکار ارز
    : ICommand<IntegratedPurchaseResponse>;

public record IntegratedPurchaseResponse(
    Guid DepositTransactionId,
    Guid WithdrawTransactionId,
    decimal Amount,
    decimal RemainingBalance,
    DateTime PurchaseDate);