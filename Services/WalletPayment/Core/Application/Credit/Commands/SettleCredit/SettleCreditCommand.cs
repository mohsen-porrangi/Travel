using BuildingBlocks.CQRS;

namespace WalletPayment.Application.Credit.Commands.SettleCredit;

public record SettleCreditCommand(
    Guid UserId,
    string PaymentReferenceId) : ICommand<SettleCreditResponse>;

public record SettleCreditResponse(
    Guid TransactionId,
    Guid WalletId,
    decimal SettledAmount,
    DateTime SettlementDate);