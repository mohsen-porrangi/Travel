using BuildingBlocks.CQRS;

namespace WalletPayment.Application.Accounts.Queries.GetUserBankAccounts;

public record GetUserBankAccountsQuery(Guid UserId) : IQuery<UserBankAccountsResponse>;

public record UserBankAccountsResponse
{
    public Guid UserId { get; init; }
    public List<BankAccountDto> BankAccounts { get; init; } = new();
}

public record BankAccountDto
{
    public Guid Id { get; init; }
    public string AccountNumber { get; init; }
    public string BankName { get; init; }
    public string CardNumber { get; init; }
    public string ShabaNumber { get; init; }
    public string AccountHolderName { get; init; }
    public bool IsVerified { get; init; }
    public DateTime CreatedAt { get; init; }
}