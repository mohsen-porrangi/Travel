using BuildingBlocks.Messaging.Events;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Domain.Events;

public record RefundInitiatedEvent : IntegrationEvent
{
    public RefundInitiatedEvent(
        Guid originalTransactionId,
        Guid walletId,
        Guid accountId,
        decimal amount,
        CurrencyCode currency,
        string reason)
    {
        OriginalTransactionId = originalTransactionId;
        WalletId = walletId;
        AccountId = accountId;
        Amount = amount;
        Currency = currency;
        Reason = reason;
        Source = "WalletPayment";
    }
    public Guid OriginalTransactionId { get; }
    public Guid WalletId { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string Reason { get; }
}

public record RefundCompletedEvent : IntegrationEvent
{
    public RefundCompletedEvent(
        Guid refundTransactionId,
        Guid originalTransactionId,
        Guid walletId,
        decimal amount,
        CurrencyCode currency)
    {
        RefundTransactionId = refundTransactionId;
        OriginalTransactionId = originalTransactionId;
        WalletId = walletId;
        Amount = amount;
        Currency = currency;
        Source = "WalletPayment";
    }

    public Guid RefundTransactionId { get; }
    public Guid OriginalTransactionId { get; }
    public Guid WalletId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
}