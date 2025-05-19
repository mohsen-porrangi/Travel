namespace WalletPayment.Domain.Entities.Enums;

public enum TransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4,
    Canceled = 5
}