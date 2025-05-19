using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Common.Contracts;

public interface ICurrencyExchangeService
{
    /// <summary>
    /// دریافت نرخ تبدیل بین دو ارز
    /// </summary>
    Task<decimal> GetExchangeRateAsync(CurrencyCode sourceCurrency, CurrencyCode targetCurrency);

    /// <summary>
    /// محاسبه مبلغ نهایی پس از تبدیل
    /// </summary>
    Task<(decimal TargetAmount, decimal Fee)> CalculateConversionAsync(
        decimal sourceAmount,
        CurrencyCode sourceCurrency,
        CurrencyCode targetCurrency);

    /// <summary>
    /// دریافت لیست همه نرخ‌های ارزی موجود
    /// </summary>
    Task<IReadOnlyDictionary<(CurrencyCode Source, CurrencyCode Target), decimal>> GetAllExchangeRatesAsync();
}