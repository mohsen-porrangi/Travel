using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace Application.Wallet.Queries.GetWalletByUserId;
public record GetWalletByUserIdQuery(Guid UserId) : IQuery<WalletDetailsDto>;

public record WalletDetailsDto(
    Guid Id,
    Guid UserId,
    decimal CreditLimit,
    decimal CreditBalance,
    DateTime? CreditDueDate,
    bool IsActive,
    ICollection<AccountDto> Accounts);

public record AccountDto(
    Guid Id,
    string AccountNumber,
    CurrencyCode Currency,
    decimal Balance);