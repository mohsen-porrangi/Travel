using BuildingBlocks.Messaging.Events;

namespace WalletPayment.Domain.Events;

public record CreditAssignedEvent : IntegrationEvent
{
    public CreditAssignedEvent(Guid walletId, Guid userId, decimal amount, DateTime dueDate)
    {
        WalletId = walletId;
        UserId = userId;
        Amount = amount;
        DueDate = dueDate;
        Source = "WalletPayment";
    }
    public Guid WalletId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public DateTime DueDate { get; }
}

public record CreditSettledEvent : IntegrationEvent
{
    public CreditSettledEvent(Guid walletId, Guid creditHistoryId, decimal amount)
    {
        WalletId = walletId;
        CreditHistoryId = creditHistoryId;
        Amount = amount;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid CreditHistoryId { get; }
    public decimal Amount { get; }
}

public record CreditOverdueEvent : IntegrationEvent
{
    public CreditOverdueEvent(Guid walletId, Guid userId, decimal amount, DateTime dueDate)
    {
        WalletId = walletId;
        UserId = userId;
        Amount = amount;
        DueDate = dueDate;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid UserId { get; }
    public decimal Amount { get; }
    public DateTime DueDate { get; }
}