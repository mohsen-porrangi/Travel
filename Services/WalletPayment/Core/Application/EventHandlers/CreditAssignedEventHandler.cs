using Microsoft.Extensions.Logging;
using BuildingBlocks.Messaging.Handlers;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class CreditAssignedEventHandler(ILogger<CreditAssignedEventHandler> logger)
    : IIntegrationEventHandler<CreditAssignedEvent>
{
    public Task HandleAsync(CreditAssignedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "اعتبار به مبلغ {Amount} به کیف پول با شناسه {WalletId} اختصاص داده شد. تاریخ سررسید: {DueDate}",
            @event.Amount,
            @event.WalletId,
            @event.DueDate.ToString("yyyy/MM/dd"));

        // اقدامات بیشتر مانند ارسال اعلان به کاربر و...

        return Task.CompletedTask;
    }
}