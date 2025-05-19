namespace WalletPayment.Domain.Entities.Enums;

public enum TransactionType
{
    Deposit = 1,       // شارژ مستقیم
    Withdrawal = 2,    // برداشت
    Purchase = 3,      // خرید
    Refund = 4,        // استرداد
    Transfer = 5,      // انتقال بین کاربران
    Fee = 6,           // کارمزد
    CreditSettlement = 7 // تسویه اعتبار
}