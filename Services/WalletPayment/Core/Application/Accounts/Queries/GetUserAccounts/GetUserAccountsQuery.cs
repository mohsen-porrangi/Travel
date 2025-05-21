using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Accounts.Queries.GetUserAccounts;

public record GetUserAccountsQuery(Guid UserId) : IQuery<UserAccountsResponse>;

public record UserAccountsResponse
{
    public Guid WalletId { get; init; }
    public int TotalAccounts { get; init; }
    public List<CurrencyAccountDto> Accounts { get; init; } = new();
}

public record CurrencyAccountDto
{
    public Guid Id { get; init; }    
    public CurrencyCode Currency { get; init; }
    public decimal Balance { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}