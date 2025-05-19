using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class TransferInitiatedEventHandler(ILogger<TransferInitiatedEventHandler> logger)
    : IIntegrationEventHandler<TransferInitiatedEvent>
{
    public Task HandleAsync(TransferInitiatedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "شروع انتقال به مبلغ {Amount} {Currency} از کیف پول {SourceWalletId} به کیف پول {TargetWalletId}",
            @event.Amount,
            @event.Currency,
            @event.SourceWalletId,
            @event.TargetWalletId);

        return Task.CompletedTask;
    }
}