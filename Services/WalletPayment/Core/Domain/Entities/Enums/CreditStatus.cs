namespace WalletPayment.Domain.Entities.Enums;

public enum CreditStatus
{
    Active = 1,      // فعال
    Settled = 2,     // تسویه شده
    Overdue = 3,     // سررسید گذشته
    Suspended = 4    // معلق
}