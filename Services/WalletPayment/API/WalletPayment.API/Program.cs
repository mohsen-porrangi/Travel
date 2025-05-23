using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WalletPayment.API.Middleware;
using WalletPayment.Infrastructure;
using WalletPayment.Application;
using WalletPayment.API.Services;
using BuildingBlocks.Contracts;
using WalletPayment.Application.EventHandlers;
using BuildingBlocks.Messaging.Contracts;

namespace WalletPayment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembly = typeof(Program).Assembly;
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            // Health Checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<WalletPayment.Infrastructure.Data.Context.WalletDbContext>("Database");

            // Infrastructure
            builder.Services.AddWalletPaymentInfrastructure(builder.Configuration);

            // Application
            builder.Services.AddWalletPaymentApplication(builder.Configuration);

            // API
            builder.Services.AddValidatorsFromAssembly(assembly);
            builder.Services.AddCarter();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddExceptionHandler<ErrorHandlerMiddleware>();
            builder.Services.AddProblemDetails();

            builder.Services.AddHttpContextAccessor();

            // Authentication & Authorization
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            //Inject Service TODO move to dependency incejection
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            var app = builder.Build();







            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // تست 1: Handler در DI
            var handler = scope.ServiceProvider.GetService<UserActivatedEventHandler>();
            logger.LogInformation("Handler in DI: {HasHandler}", handler != null ? "✅ YES" : "❌ NO");

            // تست 2: Registrations
            var registrations = scope.ServiceProvider.GetServices<BuildingBlocks.Messaging.Registration.EventHandlerRegistration>();
            logger.LogInformation("EventHandlerRegistrations count: {Count}", registrations.Count());

            // تست 3: HostedServices  
            var hostedServices = scope.ServiceProvider.GetServices<IHostedService>();
            logger.LogInformation("HostedServices count: {Count}", hostedServices.Count());
            foreach (var service in hostedServices)
            {
                logger.LogInformation("HostedService: {Type}", service.GetType().Name);
            }

            // تست 4: Subscription
            var subscriptionsManager = scope.ServiceProvider.GetService<IMessageBusSubscriptionsManager>();
            var hasSubscription = subscriptionsManager?.HasSubscriptionsForEvent("UserActivatedEvent") ?? false;
            logger.LogInformation("Has subscription for UserActivatedEvent: {HasSub}", hasSubscription ? "✅ YES" : "❌ NO");

            // تست 5: لیست تمام subscriptions
            if (subscriptionsManager != null)
            {
                // فراخوانی متد GetEventKey برای چک کردن نام
                var eventKey = subscriptionsManager.GetEventKey<BuildingBlocks.Messaging.Events.UserEvents.UserActivatedEvent>();
                logger.LogInformation("Event key for UserActivatedEvent: '{EventKey}'", eventKey);
            }







            app.UseExceptionHandler();
            app.UseStatusCodePages();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseCurrentUser();
            app.UseAuthorization();

            app.MapCarter();
            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/test-subscriptions", (IServiceProvider serviceProvider) =>
                {
                    var subscriptionsManager = serviceProvider.GetService<IMessageBusSubscriptionsManager>();

                    var hasSubscription = subscriptionsManager?.HasSubscriptionsForEvent("UserActivatedEvent") ?? false;

                    return new
                    {
                        hasSubscription,
                        eventKey = "UserActivatedEvent",
                        message = hasSubscription ? "✅ Subscription found" : "❌ No subscription"
                    };
                });

                app.MapPost("/test-event", async (IServiceProvider serviceProvider) =>
                {
                    var messageBus = serviceProvider.GetService<IMessageBus>();
                    var testEvent = new BuildingBlocks.Messaging.Events.UserEvents.UserActivatedEvent(Guid.NewGuid(), "09123456789");

                    await messageBus.PublishAsync(testEvent);

                    return new { message = "Test event published", eventId = testEvent.Id };
                });
            }

            app.UseHealthChecks("/health",
                new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });


            app.Run();
        }
    }
}
