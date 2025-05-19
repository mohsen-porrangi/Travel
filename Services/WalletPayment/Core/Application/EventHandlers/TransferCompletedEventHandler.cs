using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class TransferCompletedEventHandler(ILogger<TransferCompletedEventHandler> logger)
    : IIntegrationEventHandler<TransferCompletedEvent>
{
    public Task HandleAsync(TransferCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "انتقال با شناسه {TransferId} به مبلغ {Amount} {Currency} از کیف پول {SourceWalletId} به کیف پول {TargetWalletId} با موفقیت انجام شد. کارمزد: {FeeAmount}",
            @event.TransferId,
            @event.Amount,
            @event.Currency,
            @event.SourceWalletId,
            @event.TargetWalletId,
            @event.FeeAmount);

        // انجام اقدامات بیشتر مانند ارسال اعلان به کاربران یا...

        return Task.CompletedTask;
    }
}