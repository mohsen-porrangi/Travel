using BuildingBlocks.CQRS;

namespace WalletPayment.Application.Credit.Commands.AssignCredit;

public record AssignCreditCommand(
    Guid UserId,
    decimal Amount,
    DateTime DueDate,
    string Description) : ICommand<AssignCreditResponse>;

public record AssignCreditResponse(
    Guid WalletId,
    decimal CreditLimit,
    decimal CreditBalance,
    DateTime DueDate);