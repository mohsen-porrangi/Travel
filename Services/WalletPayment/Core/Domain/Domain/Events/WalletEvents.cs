using BuildingBlocks.Messaging.Events;
using WalletPayment.Domain.Entities.Enums;

namespace Domain.Domain.Events;
public record WalletCreatedEvent : IntegrationEvent
{
    public WalletCreatedEvent(Guid walletId, Guid userId)
    {
        WalletId = walletId;
        UserId = userId;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid UserId { get; }
}

public record AccountCreatedEvent : IntegrationEvent
{
    public AccountCreatedEvent(Guid walletId, Guid accountId, CurrencyCode currency)
    {
        WalletId = walletId;
        AccountId = accountId;
        Currency = currency;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid AccountId { get; }
    public CurrencyCode Currency { get; }
}
