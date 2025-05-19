using Microsoft.Extensions.Logging;
using BuildingBlocks.Messaging.Handlers;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class CreditSettledEventHandler(ILogger<CreditSettledEventHandler> logger)
    : IIntegrationEventHandler<CreditSettledEvent>
{
    public Task HandleAsync(CreditSettledEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "اعتبار کیف پول با شناسه {WalletId} به مبلغ {Amount} تسویه شد.",
            @event.WalletId,
            @event.Amount);

        return Task.CompletedTask;
    }
}