using Microsoft.Extensions.Logging;
using BuildingBlocks.Messaging.Handlers;
using WalletPayment.Domain.Events;

namespace WalletPayment.Application.EventHandlers;

public class CreditOverdueEventHandler(ILogger<CreditOverdueEventHandler> logger)
    : IIntegrationEventHandler<CreditOverdueEvent>
{
    public Task HandleAsync(CreditOverdueEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "اعتبار کیف پول با شناسه {WalletId} به مبلغ {Amount} سررسید شده است. تاریخ سررسید: {DueDate}",
            @event.WalletId,
            @event.Amount,
            @event.DueDate.ToString("yyyy/MM/dd"));

        // اقدامات مربوط به اعتبارهای سررسید شده
        // مانند ارسال اعلان به کاربر، ارسال درخواست تسویه و غیره

        return Task.CompletedTask;
    }
}