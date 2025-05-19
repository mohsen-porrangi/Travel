using BuildingBlocks.Messaging.Events;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Domain.Events;

public record CurrencyConversionRequestedEvent : IntegrationEvent
{
    public CurrencyConversionRequestedEvent(
        Guid walletId,
        Guid sourceAccountId,
        Guid targetAccountId,
        decimal sourceAmount,
        CurrencyCode sourceCurrency,
        CurrencyCode targetCurrency,
        decimal estimatedTargetAmount,
        decimal exchangeRate)
    {
        WalletId = walletId;
        SourceAccountId = sourceAccountId;
        TargetAccountId = targetAccountId;
        SourceAmount = sourceAmount;
        SourceCurrency = sourceCurrency;
        TargetCurrency = targetCurrency;
        EstimatedTargetAmount = estimatedTargetAmount;
        ExchangeRate = exchangeRate;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid SourceAccountId { get; }
    public Guid TargetAccountId { get; }
    public decimal SourceAmount { get; }
    public CurrencyCode SourceCurrency { get; }
    public CurrencyCode TargetCurrency { get; }
    public decimal EstimatedTargetAmount { get; }
    public decimal ExchangeRate { get; }
}

public record CurrencyConversionCompletedEvent : IntegrationEvent
{
    public CurrencyConversionCompletedEvent(
        Guid walletId,
        Guid sourceAccountId,
        Guid targetAccountId,
        decimal sourceAmount,
        decimal targetAmount,
        CurrencyCode sourceCurrency,
        CurrencyCode targetCurrency,
        decimal exchangeRate,
        decimal feeAmount)
    {
        WalletId = walletId;
        SourceAccountId = sourceAccountId;
        TargetAccountId = targetAccountId;
        SourceAmount = sourceAmount;
        TargetAmount = targetAmount;
        SourceCurrency = sourceCurrency;
        TargetCurrency = targetCurrency;
        ExchangeRate = exchangeRate;
        FeeAmount = feeAmount;
        Source = "WalletPayment";
    }

    public Guid WalletId { get; }
    public Guid SourceAccountId { get; }
    public Guid TargetAccountId { get; }
    public decimal SourceAmount { get; }
    public decimal TargetAmount { get; }
    public CurrencyCode SourceCurrency { get; }
    public CurrencyCode TargetCurrency { get; }
    public decimal ExchangeRate { get; }
    public decimal FeeAmount { get; }
}