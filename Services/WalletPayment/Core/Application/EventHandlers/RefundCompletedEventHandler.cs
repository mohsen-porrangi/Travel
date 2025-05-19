using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class RefundCompletedEventHandler(ILogger<RefundCompletedEventHandler> logger)
    : IIntegrationEventHandler<RefundCompletedEvent>
{
    public Task HandleAsync(RefundCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "استرداد با شناسه {RefundTransactionId} به مبلغ {Amount} {Currency} برای تراکنش {OriginalTransactionId} با موفقیت انجام شد",
            @event.RefundTransactionId,
            @event.Amount,
            @event.Currency,
            @event.OriginalTransactionId);

        // اقدامات بیشتر مانند ارسال اعلان، ثبت لاگ و...

        return Task.CompletedTask;
    }
}