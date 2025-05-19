// File: BuildingBlocks.Messaging/BuildingBlocks.Messaging/Registration/EventHandlerRegistrationService.cs
using BuildingBlocks.Messaging.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Messaging.Registration;

/// <summary>
/// سرویس راه‌انداز برای ثبت هندلرهای رویداد در زمان اجرا
/// </summary>
public class EventHandlerRegistrationService(
    IServiceProvider serviceProvider,
    IEnumerable<EventHandlerRegistration> registrations) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriptionsManager = serviceProvider.GetRequiredService<IMessageBusSubscriptionsManager>();

        foreach (var registration in registrations)
        {
            var addSubscriptionMethod = typeof(IMessageBusSubscriptionsManager)
                .GetMethod("AddSubscription")
                ?.MakeGenericMethod(registration.EventType, registration.HandlerType);

            addSubscriptionMethod?.Invoke(subscriptionsManager, Array.Empty<object>());
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}