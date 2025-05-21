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
    ICollection<CurrencyAccountDTO> Accounts);

public record CurrencyAccountDTO(
    Guid Id,    
    CurrencyCode Currency,
    decimal Balance);