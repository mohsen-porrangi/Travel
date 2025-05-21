using Microsoft.AspNetCore.Http;
using WalletPayment.Application.Payment.Models;

namespace WalletPayment.Application.Payment.Services;

/// <summary>
/// سرویس پردازش کالبک‌های پرداخت
/// </summary>
public interface IPaymentCallbackProcessor
{
    /// <summary>
    /// استخراج پارامترهای کالبک از کوئری استرینگ درخواست
    /// </summary>
    /// <param name="query">کوئری استرینگ درخواست</param>
    /// <param name="gatewayType">نوع درگاه پرداخت (اختیاری)</param>
    /// <returns>مدل پارامترهای کالبک</returns>
    PaymentCallbackParameters ExtractParameters(IQueryCollection query, string? gatewayType);
}