namespace BuildingBlocks.Messaging.Events.UserEvents;

using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
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
    public UserActivatedEvent(Guid userId, string mobile)
    {
        UserId = userId;
        Mobile = mobile;
        Source = "UserManagement";
    }

    public Guid UserId { get; init; } = default!;
    public string Mobile { get; init; } = default!;
}

