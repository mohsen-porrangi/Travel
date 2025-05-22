using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Credit;
using WalletPayment.Domain.Entities.Enums;
using WalletPayment.Domain.Events;
using BuildingBlocks.Messaging.Contracts;

namespace WalletPayment.Infrastructure.BackgroundServices;

public class CreditDueDateCheckingService// : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CreditDueDateCheckingService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // بررسی هر 6 ساعت

    public CreditDueDateCheckingService(
        IServiceScopeFactory scopeFactory,
        ILogger<CreditDueDateCheckingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

   // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
   // {
   //     _logger.LogInformation("سرویس بررسی سررسید اعتبارها آغاز به کار کرد");

   //     while (!stoppingToken.IsCancellationRequested)
   //     {
   //         try
   //         {
   //             //await CheckCreditDueDatesAsync(stoppingToken);
   //         }
   //         catch (Exception ex)
   //         {
   //             _logger.LogError(ex, "خطا در بررسی سررسید اعتبارها");
   //         }

   //// TODO         await Task.Delay(_checkInterval, stoppingToken);
   //     }
   // }

   // private async Task CheckCreditDueDatesAsync(CancellationToken cancellationToken)
   // {
   //     try
   //     {
   //         _logger.LogInformation("شروع بررسی سررسید اعتبارها: {Time}", DateTime.UtcNow);

   //         using var scope = _scopeFactory.CreateScope();
   //         var dbContext = scope.ServiceProvider.GetRequiredService<IWalletDbContext>();
   //         var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
   //         var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

   //         // یافتن کیف پول‌هایی که دارای اعتبار فعال هستند و سررسید آنها نزدیک یا گذشته است
   //         var now = DateTime.UtcNow;
   //         var threeDaysLater = now.AddDays(3);

   //         // کیف پول‌های با اعتبار نزدیک به سررسید
   //         var nearDueWallets = await dbContext.Wallets
   //             .Where(w => w.IsActive &&
   //                         !w.IsDeleted &&
   //                         w.CreditLimit > 0 &&
   //                         w.CreditBalance > 0 &&
   //                         w.CreditDueDate.HasValue &&
   //                         w.CreditDueDate.Value <= threeDaysLater &&
   //                         w.CreditDueDate.Value > now)
   //             .ToListAsync(/*cancellationToken*/);

   //         // کیف پول‌های با اعتبار سررسید گذشته
   //         var overDueWallets = await dbContext.Wallets
   //             .Where(w => w.IsActive &&
   //                         !w.IsDeleted &&
   //                         w.CreditLimit > 0 &&
   //                         w.CreditBalance > 0 &&
   //                         w.CreditDueDate.HasValue &&
   //                         w.CreditDueDate.Value <= now)
   //             .ToListAsync(cancellationToken);

   //         _logger.LogInformation(
   //             "تعداد {NearDueCount} اعتبار نزدیک به سررسید و {OverDueCount} اعتبار سررسید گذشته یافت شد",
   //             nearDueWallets.Count, overDueWallets.Count);

   //         // ارسال اعلان برای اعتبارهای نزدیک به سررسید
   //         foreach (var wallet in nearDueWallets)
   //         {
   //             var daysRemaining = (wallet.CreditDueDate.Value - now).Days;
   //             var message = $"اعتبار شما به مبلغ {wallet.CreditBalance} تا {daysRemaining} روز دیگر سررسید می‌شود. لطفاً نسبت به تسویه آن اقدام نمایید.";

   //             await notificationService.SendNotificationAsync(
   //                 wallet.UserId,
   //                 "هشدار سررسید اعتبار",
   //                 message,
   //                 NotificationType.CreditDueReminder,
   //                 cancellationToken);

   //             _logger.LogInformation(
   //                 "اعلان سررسید اعتبار برای کاربر {UserId} ارسال شد. مبلغ: {Amount}, سررسید: {DueDate}",
   //                 wallet.UserId, wallet.CreditBalance, wallet.CreditDueDate);
   //         }

   //         // بروزرسانی و ارسال اعلان برای اعتبارهای سررسید گذشته
   //         foreach (var wallet in overDueWallets)
   //         {
   //             try
   //             {
   //                 // بررسی وضعیت سررسید
   //                 wallet.CheckCreditDueDate();

   //                 // ارسال اعلان سررسید گذشته به کاربر
   //                 var message = $"اعتبار شما به مبلغ {wallet.CreditBalance} سررسید شده است. لطفاً در اسرع وقت نسبت به تسویه آن اقدام نمایید.";

   //                 await notificationService.SendNotificationAsync(
   //                     wallet.UserId,
   //                     "اعتبار سررسید شده",
   //                     message,
   //                     NotificationType.CreditOverdue,
   //                     cancellationToken);

   //                 // انتشار رویداد سررسید اعتبار
   //                 var overDueEvent = new CreditOverdueEvent(
   //                     wallet.Id, wallet.UserId, wallet.CreditBalance, wallet.CreditDueDate.Value);

   //                 await messageBus.PublishAsync(overDueEvent, cancellationToken);

   //                 _logger.LogInformation(
   //                     "اعتبار کاربر {UserId} سررسید شده. مبلغ: {Amount}, سررسید: {DueDate}",
   //                     wallet.UserId, wallet.CreditBalance, wallet.CreditDueDate);
   //             }
   //             catch (Exception ex)
   //             {
   //                 _logger.LogError(ex,
   //                     "خطا در پردازش اعتبار سررسید شده برای کیف پول {WalletId}, کاربر {UserId}",
   //                     wallet.Id, wallet.UserId);
   //             }
   //         }

   //         // ذخیره تغییرات
   //         await dbContext.SaveChangesAsync(cancellationToken);
   //         _logger.LogInformation("بررسی سررسید اعتبارها با موفقیت انجام شد");
   //     }
   //     catch (OperationCanceledException ex) {
   //         _logger.LogWarning("Query cancelled due to token cancellation.");
   //     }
   //     catch (Exception ex)
   //     {
   //         _logger.LogError(ex, "❗ Exception type: {ExceptionType}", ex.GetType().Name);
   //     }
   // }
}