using BuildingBlocks.Contracts.Services;
using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Queries.GetAccountStatement;

public record GetAccountStatementQuery : IQuery<AccountStatementResponse>
{
    public Guid UserId { get; init; }
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;
    public DateTime StartDate { get; init; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime EndDate { get; init; } = DateTime.UtcNow;
}

public record AccountStatementResponse
{
    public decimal OpeningBalance { get; init; }
    public decimal ClosingBalance { get; init; }
    public decimal TotalDeposits { get; init; }
    public decimal TotalWithdrawals { get; init; }
    public int TotalTransactions { get; init; }
    public DateTime StatementStartDate { get; init; }
    public DateTime StatementEndDate { get; init; }
    public CurrencyCode Currency { get; init; }
    public List<StatementEntryDto> Entries { get; init; } = new();
}

public record StatementEntryDto
{
    public DateTime Date { get; init; }
    public string Description { get; init; }
    public decimal Amount { get; init; }
    public TransactionDirection Direction { get; init; }
    public decimal RunningBalance { get; init; }
    public Domain.Entities.Enums.TransactionType Type { get; init; }
    public string Reference { get; init; }
}