using BuildingBlocks.Contracts.Services;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IUserManagementService _userManagementService;

    public NotificationService(
        ILogger<NotificationService> logger,
        IUserManagementService userManagementService)
    {
        _logger = logger;
        _userManagementService = userManagementService;
    }

    public async Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // دریافت اطلاعات کاربر
            var user = await _userManagementService.GetUserByIdAsync(userId);

            // ایجاد متغیرهای محتوا با اطلاعات شخصی‌سازی شده
            var contentVariables = new Dictionary<string, string>
            {
                { "UserName", $"{user.FirstName} {user.LastName}" },
                { "CurrentDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm") }
            };

            // جایگزینی متغیرها در پیام
            var finalMessage = ReplaceVariables(message, contentVariables);

            _logger.LogInformation(
                "اعلان ارسال شد: کاربر: {UserId}, نوع: {Type}, عنوان: {Title}",
                userId, type, title);

            // در محیط واقعی، اینجا از سیستم اعلان استفاده می‌شود
            // مثلاً ارسال به FCM، ذخیره در دیتابیس اعلان‌ها و...

            // ارسال ایمیل و پیامک برای اعلان‌های مهم
            if (type == NotificationType.CreditDueReminder || type == NotificationType.CreditOverdue)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendEmailNotificationAsync(user.Email, title, finalMessage, cancellationToken);
                }

                if (!string.IsNullOrEmpty(user.Mobile))
                {
                    await SendSmsNotificationAsync(user.Mobile, finalMessage, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ارسال اعلان به کاربر {UserId}", userId);
        }
    }

    public async Task SendEmailNotificationAsync(
        string email,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("ارسال ایمیل به {Email}: {Title}", email, title);

            // در محیط واقعی، اینجا از سرویس ارسال ایمیل استفاده می‌شود
            // await _emailService.SendAsync(email, title, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ارسال ایمیل به {Email}", email);
        }
    }

    public async Task SendSmsNotificationAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("ارسال پیامک به {PhoneNumber}: {Message}", phoneNumber, message);

            // در محیط واقعی، اینجا از سرویس ارسال پیامک استفاده می‌شود
            // await _smsService.SendAsync(phoneNumber, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ارسال پیامک به {PhoneNumber}", phoneNumber);
        }
    }

    private string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        foreach (var variable in variables)
        {
            template = template.Replace($"{{{variable.Key}}}", variable.Value);
        }

        return template;
    }
}