using Application.Wallet.Commands.CreateWallet;
using BuildingBlocks.Messaging.Events.UserEvents;
using BuildingBlocks.Messaging.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WalletPayment.Application.EventHandlers;

/// <summary>
/// هندلر رویداد فعال‌سازی کاربر
/// </summary>
public class UserActivatedEventHandler(ILogger<UserActivatedEventHandler> logger, ISender sender)
    : IIntegrationEventHandler<UserActivatedEvent>
{
    public async Task HandleAsync(UserActivatedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("پردازش رویداد فعال‌سازی کاربر با شناسه {UserId}", @event.UserId);

        // اینجا منطق پردازش رویداد قرار می‌گیرد
        var command = new CreateWalletCommand(@event.UserId);
        await sender.Send(command, cancellationToken);
        
    }
}
