namespace BuildingBlocks.Messaging.InMemory;

using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Messaging.Contracts;

/// <summary>
/// پیاده‌سازی درون‌حافظه‌ای باس پیام برای استفاده در مونولیت
/// </summary>
public class InMemoryMessageBus(
    IServiceProvider serviceProvider,
    IMessageBusSubscriptionsManager subscriptionsManager,
    ILogger<InMemoryMessageBus> logger
) : IMessageBus
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        var eventName = subscriptionsManager.GetEventKey<T>();

        logger.LogInformation("انتشار رویداد {EventName} با شناسه {EventId}", eventName, message.Id);

        if (!subscriptionsManager.HasSubscriptionsForEvent<T>())
        {
            logger.LogWarning("هیچ اشتراکی برای رویداد {EventName} وجود ندارد", eventName);
            return;
        }

        var handlers = subscriptionsManager.GetHandlersForEvent<T>();

        using var scope = serviceProvider.CreateScope();

        foreach (var handlerType in handlers)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                logger.LogWarning("هندلر {HandlerType} برای رویداد {EventName} پیدا نشد", handlerType.Name, eventName);
                continue;
            }

            logger.LogDebug("پردازش رویداد {EventName} توسط هندلر {HandlerType}", eventName, handlerType.Name);

            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(typeof(T));
            var handleMethod = concreteType.GetMethod("HandleAsync");

            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(handler, new object[] { message, cancellationToken });
            }
        }
    }

    public Task SendAsync<T>(T message, CancellationToken cancellationToken = default) where T : IntegrationEvent
    {
        // در پیاده‌سازی درون‌حافظه‌ای، تفاوتی بین انتشار و ارسال وجود ندارد
        return PublishAsync(message, cancellationToken);
    }
}