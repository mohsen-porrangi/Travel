namespace UserManagement.API.Events;

using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// رویداد فعال‌سازی کاربر
/// </summary>
public record UserActivatedEvent : IntegrationEvent
{
    public UserActivatedEvent(Guid userId)
    {
        UserId = userId;
        Source = "UserManagement";
    }

    public Guid UserId { get; init; }
}

/// <summary>
/// هندلر رویداد فعال‌سازی کاربر
/// </summary>
public class UserActivatedEventHandler(ILogger<UserActivatedEventHandler> logger)
    : IIntegrationEventHandler<UserActivatedEvent>
{
    public Task HandleAsync(UserActivatedEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("پردازش رویداد فعال‌سازی کاربر با شناسه {UserId}", @event.UserId);

        // اینجا منطق پردازش رویداد قرار می‌گیرد

        return Task.CompletedTask;
    }
}