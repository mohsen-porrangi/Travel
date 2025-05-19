using BuildingBlocks.Messaging.Handlers;
using Domain.Domain.Events;
using Microsoft.Extensions.Logging;

namespace WalletPayment.Application.EventHandlers;
public class WalletDepositedEventHandler(ILogger<WalletDepositedEventHandler> logger)
    : IIntegrationEventHandler<WalletDepositedEvent>
{
    public Task HandleAsync(WalletDepositedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "کیف پول با شناسه {WalletId} به مبلغ {Amount} {Currency} شارژ شد. شناسه مرجع: {ReferenceId}",
            @event.WalletId,
            @event.Amount,
            @event.Currency,
            @event.ReferenceId);

        // می‌توان عملیات مختلفی را پس از شارژ کیف پول انجام داد
        // مثلاً ارسال اعلان به کاربر، ثبت اطلاعات آماری و ...

        return Task.CompletedTask;
    }
}