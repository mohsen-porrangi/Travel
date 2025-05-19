namespace WalletPayment.Domain.Entities.Enums;

/// <summary>
/// کدهای خطای درگاه پرداخت
/// </summary>
public enum PaymentErrorCode
{
    Unknown = 0,
    InvalidAmount = 1,
    InvalidCurrency = 2,
    DuplicateTransaction = 3,
    AuthorityNotFound = 4,
    ExpiredTransaction = 5,
    InvalidIpAddress = 6,
    MerchantNotFound = 7,
    ConnectionFailed = 8,
    GatewayError = 9,
    RefundNotAllowed = 10,
    CanceledByUser = 11
}