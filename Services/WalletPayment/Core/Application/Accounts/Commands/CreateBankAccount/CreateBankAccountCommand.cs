using BuildingBlocks.CQRS;

namespace WalletPayment.Application.Accounts.Commands.CreateBankAccount;

public record CreateBankAccountCommand : ICommand<CreateBankAccountResponse>
{    
    public string AccountNumber { get; init; }
    public string BankName { get; init; }
    public string CardNumber { get; init; }
    public string ShabaNumber { get; init; }
    public string AccountHolderName { get; init; }
}

public record CreateBankAccountResponse
{
    public Guid BankAccountId { get; init; }
    public string AccountNumber { get; init; }
    public string BankName { get; init; }
    public bool IsVerified { get; init; }
}