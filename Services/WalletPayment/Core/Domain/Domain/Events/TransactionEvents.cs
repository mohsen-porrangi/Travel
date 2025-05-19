using BuildingBlocks.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Domain.Entities.Enums;

namespace Domain.Domain.Events;
public record WalletDepositedEvent : IntegrationEvent
{
    public WalletDepositedEvent(Guid walletId, Guid accountId, decimal amount, CurrencyCode currency, string referenceId)
    {
        WalletId = walletId;
        AccountId = accountId;
        Amount = amount;
        Currency = currency;
        ReferenceId = referenceId;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid AccountId { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string ReferenceId { get; }
    public record WalletWithdrawnEvent : IntegrationEvent
    {
        public WalletWithdrawnEvent(Guid walletId, Guid accountId, decimal amount, CurrencyCode currency, string orderId)
        {
            WalletId = walletId;
            AccountId = accountId;
            Amount = amount;
            Currency = currency;
            OrderId = orderId;
            Source = "WalletPayment";
        }

        public Guid WalletId { get; }
        public Guid AccountId { get; }
        public decimal Amount { get; }
        public CurrencyCode Currency { get; }
        public string OrderId { get; }
    }
}