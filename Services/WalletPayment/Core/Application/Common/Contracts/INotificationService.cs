namespace WalletPayment.Application.Common.Contracts;

public interface INotificationService
{
    Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        CancellationToken cancellationToken = default);

    Task SendEmailNotificationAsync(
        string email,
        string title,
        string message,
        CancellationToken cancellationToken = default);

    Task SendSmsNotificationAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}

public enum NotificationType
{
    General = 1,
    WalletDeposit = 2,
    WalletWithdrawal = 3,
    Transfer = 4,
    CreditAssigned = 5,
    CreditDueReminder = 6,
    CreditOverdue = 7,
    CreditSettled = 8
}