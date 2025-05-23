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
        Console.WriteLine("🚀 EventHandlerRegistrationService.StartAsync started");

        try
        {
            var subscriptionsManager = serviceProvider.GetRequiredService<IMessageBusSubscriptionsManager>();
            var registrationsList = registrations.ToList();

            Console.WriteLine($"📝 Total registrations: {registrationsList.Count}");

            foreach (var registration in registrationsList)
            {
                Console.WriteLine($"📌 Registering: {registration.EventType.Name} -> {registration.HandlerType.Name}");

                try
                {
                    var addSubscriptionMethod = typeof(IMessageBusSubscriptionsManager)
                        .GetMethod("AddSubscription")
                        ?.MakeGenericMethod(registration.EventType, registration.HandlerType);

                    if (addSubscriptionMethod == null)
                    {
                        Console.WriteLine($"❌ AddSubscription method not found for {registration.EventType.Name}");
                        continue;
                    }

                    addSubscriptionMethod.Invoke(subscriptionsManager, Array.Empty<object>());
                    Console.WriteLine($"✅ Registered: {registration.EventType.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error registering {registration.EventType.Name}: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("✅ EventHandlerRegistrationService.StartAsync completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 General error in StartAsync: {ex.Message}");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}