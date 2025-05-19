namespace WalletPayment.Domain.Entities.Enums;

/// <summary>
/// انواع درگاه‌های پرداخت
/// </summary>
public enum PaymentGatewayType
{
    ZarinPal = 1,
    PayIr = 2,
    NextPay = 3,
    Zibal = 4,
    Sandbox = 99  // برای محیط توسعه
}