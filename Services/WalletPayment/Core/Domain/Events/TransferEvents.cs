using BuildingBlocks.Messaging.Events;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Domain.Events;

public record TransferInitiatedEvent : IntegrationEvent
{
    public TransferInitiatedEvent(
        Guid sourceWalletId,
        Guid sourceAccountId,
        Guid targetWalletId,
        Guid targetAccountId,
        decimal amount,
        CurrencyCode currency,
        string description)
    {
        SourceWalletId = sourceWalletId;
        SourceAccountId = sourceAccountId;
        TargetWalletId = targetWalletId;
        TargetAccountId = targetAccountId;
        Amount = amount;
        Currency = currency;
        Description = description;
        Source = "WalletPayment";
    }
    public Guid SourceWalletId { get; }
    public Guid SourceAccountId { get; }
    public Guid TargetWalletId { get; }
    public Guid TargetAccountId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string Description { get; }
}

public record TransferCompletedEvent : IntegrationEvent
{
    public TransferCompletedEvent(
        Guid transferId,
        Guid sourceWalletId,
        Guid targetWalletId,
        decimal amount,
        CurrencyCode currency,
        decimal feeAmount)
    {
        TransferId = transferId;
        SourceWalletId = sourceWalletId;
        TargetWalletId = targetWalletId;
        Amount = amount;
        Currency = currency;
        FeeAmount = feeAmount;
        Source = "WalletPayment";
    }

    public Guid TransferId { get; }
    public Guid SourceWalletId { get; }
    public Guid TargetWalletId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public decimal FeeAmount { get; }
}