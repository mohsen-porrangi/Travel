using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Credit.Queries.GetCreditStatus;

public record GetCreditStatusQuery(Guid UserId) : IQuery<CreditStatusResponse>;

public record CreditStatusResponse(
    Guid WalletId,
    decimal CreditLimit,
    decimal CreditBalance,
    decimal UsedCredit,
    DateTime? DueDate,
    bool IsOverdue,
    ICollection<CreditHistoryDto> CreditHistory);

public record CreditHistoryDto(
    Guid Id,
    decimal Amount,
    DateTime GrantDate,
    DateTime DueDate,
    DateTime? SettlementDate,
    CreditStatus Status,
    string Description);