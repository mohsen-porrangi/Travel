using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.TransferMoney;

public record TransferMoneyCommand : ICommand<TransferMoneyResponse>
{
    public Guid SourceUserId { get; init; }
    public Guid TargetUserId { get; init; }
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; } = CurrencyCode.IRR;
    public string Description { get; init; } = "انتقال وجه";
}

public record TransferMoneyResponse
{
    public Guid TransferId { get; init; }
    public Guid SourceTransactionId { get; init; }
    public Guid TargetTransactionId { get; init; }
    public decimal TransferredAmount { get; init; }
    public decimal FeeAmount { get; init; }
    public decimal SourceRemainingBalance { get; init; }
    public decimal TargetNewBalance { get; init; }
    public DateTime TransferDate { get; init; }
}