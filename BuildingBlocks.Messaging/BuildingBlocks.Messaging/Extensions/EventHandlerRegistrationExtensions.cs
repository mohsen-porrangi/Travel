// File: BuildingBlocks.Messaging/BuildingBlocks.Messaging/Extensions/EventHandlerRegistrationExtensions.cs
using BuildingBlocks.Messaging.Handlers;
using BuildingBlocks.Messaging.Registration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.Extensions;

/// <summary>
/// متدهای کمکی برای ثبت خودکار هندلرهای رویداد
/// </summary>
public static class EventHandlerRegistrationExtensions
{
    /// <summary>
    /// ثبت خودکار هندلرهای رویداد از assembly مشخص شده
    /// </summary>
    /// <param name="services">مجموعه سرویس‌ها</param>
    /// <param name="assembly">assembly حاوی هندلرهای رویداد</param>
    /// <returns>مجموعه سرویس‌های به‌روزشده</returns>
    public static IServiceCollection RegisterEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        // یافتن تمام کلاس‌هایی که IIntegrationEventHandler را پیاده‌سازی می‌کنند
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // یافتن رابط IIntegrationEventHandler که این کلاس پیاده‌سازی می‌کند
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            // دریافت نوع رویداد مرتبط با این هندلر
            var eventType = handlerInterface.GetGenericArguments()[0];

            // ثبت هندلر در DI
            services.AddTransient(handlerType);

            // ثبت یک راه‌انداز (startup task) برای پیکربندی اشتراک‌ها
            services.AddSingleton(new EventHandlerRegistration(eventType, handlerType));
        }

        // ثبت یک سرویس hosted برای پیکربندی اشتراک‌ها پس از اینکه همه سرویس‌ها ثبت شدند
        services.AddHostedService<EventHandlerRegistrationService>();

        return services;
    }
}