using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class RefundInitiatedEventHandler(ILogger<RefundInitiatedEventHandler> logger)
    : IIntegrationEventHandler<RefundInitiatedEvent>
{
    public Task HandleAsync(RefundInitiatedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "شروع استرداد به مبلغ {Amount} {Currency} برای تراکنش {OriginalTransactionId}. دلیل: {Reason}",
            @event.Amount,
            @event.Currency,
            @event.OriginalTransactionId,
            @event.Reason);

        return Task.CompletedTask;
    }
}