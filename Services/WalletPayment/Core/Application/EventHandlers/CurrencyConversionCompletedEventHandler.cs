using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class CurrencyConversionCompletedEventHandler(ILogger<CurrencyConversionCompletedEventHandler> logger)
    : IIntegrationEventHandler<CurrencyConversionCompletedEvent>
{
    public Task HandleAsync(CurrencyConversionCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "تبدیل ارز با موفقیت انجام شد: {SourceAmount} {SourceCurrency} به {TargetAmount} {TargetCurrency} با نرخ {ExchangeRate} و کارمزد {FeeAmount}",
            @event.SourceAmount,
            @event.SourceCurrency,
            @event.TargetAmount,
            @event.TargetCurrency,
            @event.ExchangeRate,
            @event.FeeAmount);

        // انجام اقدامات بیشتر مانند ارسال اعلان و...

        return Task.CompletedTask;
    }
}