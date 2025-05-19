using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using static Domain.Domain.Events.WalletDepositedEvent;

namespace WalletPayment.Application.EventHandlers;

public class WalletWithdrawnEventHandler(ILogger<WalletWithdrawnEventHandler> logger)
    : IIntegrationEventHandler<WalletWithdrawnEvent>
{
    public Task HandleAsync(WalletWithdrawnEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "از کیف پول با شناسه {WalletId} مبلغ {Amount} {Currency} برداشت شد. شناسه سفارش: {OrderId}",
            @event.WalletId,
            @event.Amount,
            @event.Currency,
            @event.OrderId ?? "نامشخص");

        // می‌توان عملیات مختلفی را پس از برداشت از کیف پول انجام داد
        // مثلاً ارسال اعلان به کاربر، ثبت اطلاعات آماری و ...

        return Task.CompletedTask;
    }
}