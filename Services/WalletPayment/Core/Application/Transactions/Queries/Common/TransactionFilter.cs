using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Queries.Common;

public record TransactionFilter
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public TransactionDirection? Direction { get; init; }
    public Domain.Entities.Enums.TransactionType? Type { get; init; }
    public Domain.Entities.Enums.TransactionStatus? Status { get; init; }
    public CurrencyCode? Currency { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public bool? IsCredit { get; init; }
    public string? OrderId { get; init; }
    public string? ReferenceId { get; init; }
}