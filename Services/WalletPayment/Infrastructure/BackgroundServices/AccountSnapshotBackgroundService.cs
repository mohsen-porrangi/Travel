using Application.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.BackgroundServices;

public class AccountSnapshotBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountSnapshotBackgroundService> _logger;
    private readonly TimeSpan _dailyInterval = TimeSpan.FromHours(24);

    public AccountSnapshotBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AccountSnapshotBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("سرویس ایجاد خودکار اسنپ‌شات حساب‌ها آغاز به کار کرد");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRunTime = CalculateNextRunTime(now);
            var delay = nextRunTime - now;

            _logger.LogInformation(
                "اسنپ‌شات بعدی در {NextRunTime} ایجاد خواهد شد (تأخیر: {Delay})",
                nextRunTime, delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CreateDailySnapshotsAsync(stoppingToken);

                    // بررسی برای ایجاد اسنپ‌شات هفتگی و ماهانه
                    await CreatePeriodicSnapshotsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "خطا در ایجاد اسنپ‌شات‌ها");
                }
            }
        }
    }

    private async Task CreateDailySnapshotsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("شروع ایجاد اسنپ‌شات روزانه: {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var snapshotService = scope.ServiceProvider.GetRequiredService<IAccountSnapshotService>();

        await snapshotService.CreateSnapshotsForAllAccountsAsync(SnapshotType.Daily, cancellationToken);

        _logger.LogInformation("ایجاد اسنپ‌شات روزانه با موفقیت انجام شد");
    }

    private async Task CreatePeriodicSnapshotsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        using var scope = _scopeFactory.CreateScope();
        var snapshotService = scope.ServiceProvider.GetRequiredService<IAccountSnapshotService>();

        // اسنپ‌شات هفتگی (یکشنبه)
        if (now.DayOfWeek == DayOfWeek.Sunday)
        {
            _logger.LogInformation("شروع ایجاد اسنپ‌شات هفتگی: {Time}", now);
            await snapshotService.CreateSnapshotsForAllAccountsAsync(SnapshotType.Weekly, cancellationToken);
            _logger.LogInformation("ایجاد اسنپ‌شات هفتگی با موفقیت انجام شد");
        }

        // اسنپ‌شات ماهانه (اول ماه)
        if (now.Day == 1)
        {
            _logger.LogInformation("شروع ایجاد اسنپ‌شات ماهانه: {Time}", now);
            await snapshotService.CreateSnapshotsForAllAccountsAsync(SnapshotType.Monthly, cancellationToken);
            _logger.LogInformation("ایجاد اسنپ‌شات ماهانه با موفقیت انجام شد");
        }
    }

    private DateTime CalculateNextRunTime(DateTime now)
    {
        // تنظیم برای اجرا در ساعت 1 بامداد (UTC)
        var nextRun = new DateTime(now.Year, now.Month, now.Day, 1, 0, 0, DateTimeKind.Utc);

        // اگر زمان فعلی بعد از 1 بامداد است، برو به روز بعد
        if (now.TimeOfDay >= new TimeSpan(1, 0, 0))
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun;
    }
}