using Application.Common.Contracts;
using BuildingBlocks.Messaging.Contracts;
using Carter;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Infrastructure.BackgroundServices;

namespace WalletPayment.API.Endpoints.Admin;

public class BackgroundJobsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/jobs/credit-due-check", async (
            IWalletDbContext dbContext,
            INotificationService notificationService,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken) =>
        {
            // ایجاد یک instance موقت از سرویس برای اجرای فوری
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<CreditDueDateCheckingService>();
            var messageBus = serviceProvider.GetRequiredService<IMessageBus>();

            var service = new CreditDueDateCheckingService(
                serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                logger);

            // اجرای فوری عملیات
            await service.StartAsync(cancellationToken);

            return Results.Ok(new { Message = "بررسی سررسید اعتبارها با موفقیت انجام شد" });
        })
        .WithTags("AdminJobs")
        .RequireAuthorization("Admin");

        app.MapPost("/admin/jobs/account-snapshots", async (
            [FromQuery] SnapshotType type,
            IAccountSnapshotService snapshotService,
            CancellationToken cancellationToken) =>
        {
            await snapshotService.CreateSnapshotsForAllAccountsAsync(type, cancellationToken);
            return Results.Ok(new { Message = $"ایجاد اسنپ‌شات {type} برای همه حساب‌ها با موفقیت انجام شد" });
        })
        .WithTags("AdminJobs")
        .RequireAuthorization("Admin");
    }
}