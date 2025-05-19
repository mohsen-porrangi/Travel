using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.Services;

public class CurrencyExchangeService(
    IConfiguration configuration,
    ILogger<CurrencyExchangeService> logger)
    : ICurrencyExchangeService
{
    // نرخ‌های پایه نسبت به ریال (به عنوان پایه)
    private readonly Dictionary<CurrencyCode, decimal> _baseRates = new()
    {
        { CurrencyCode.IRR, 1m },
        { CurrencyCode.USD, 500000m }, // 1 دلار = 500000 ریال
        { CurrencyCode.EUR, 550000m }, // 1 یورو = 550000 ریال
        { CurrencyCode.GBP, 650000m }, // 1 پوند = 650000 ریال
        { CurrencyCode.AED, 135000m }  // 1 درهم = 135000 ریال
    };

    // نرخ کارمزد تبدیل ارز
    private readonly decimal _conversionFeeRate = 0.01m; // 1%

    public async Task<decimal> GetExchangeRateAsync(CurrencyCode sourceCurrency, CurrencyCode targetCurrency)
    {
        // در دنیای واقعی، این متد می‌تواند از یک API خارجی نرخ‌ها را دریافت کند
        // به عنوان نمونه، از نرخ‌های ثابت استفاده می‌کنیم

        // اگر ارزها یکسان باشند، نرخ تبدیل 1 است
        if (sourceCurrency == targetCurrency)
            return 1m;

        try
        {
            // محاسبه نرخ تبدیل از طریق ریال به عنوان ارز میانی
            var sourceToIRR = _baseRates[sourceCurrency];
            var targetToIRR = _baseRates[targetCurrency];

            // نرخ تبدیل = ارزش ارز مبدأ به ریال / ارزش ارز مقصد به ریال
            var exchangeRate = sourceToIRR / targetToIRR;

            logger.LogInformation(
                "نرخ تبدیل از {SourceCurrency} به {TargetCurrency}: {ExchangeRate}",
                sourceCurrency,
                targetCurrency,
                exchangeRate);

            return exchangeRate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "خطا در محاسبه نرخ تبدیل ارز");
            throw new InvalidOperationException("خطا در محاسبه نرخ تبدیل ارز", ex);
        }

        // در دنیای واقعی، می‌توان یک سیستم caching برای نرخ‌های ارز داشت
        // تا نیاز به فراخوانی مکرر API خارجی نباشد
    }

    public async Task<(decimal TargetAmount, decimal Fee)> CalculateConversionAsync(
        decimal sourceAmount,
        CurrencyCode sourceCurrency,
        CurrencyCode targetCurrency)
    {
        // نرخ تبدیل
        var exchangeRate = await GetExchangeRateAsync(sourceCurrency, targetCurrency);

        // مبلغ پایه تبدیل
        var baseTargetAmount = sourceAmount * exchangeRate;

        // محاسبه کارمزد
        var feeAmount = baseTargetAmount * _conversionFeeRate;

        // مبلغ نهایی پس از کسر کارمزد
        var finalTargetAmount = baseTargetAmount - feeAmount;

        logger.LogInformation(
            "تبدیل {SourceAmount} {SourceCurrency} به {TargetCurrency}: {TargetAmount} (کارمزد: {FeeAmount})",
            sourceAmount,
            sourceCurrency,
            targetCurrency,
            finalTargetAmount,
            feeAmount);

        return (finalTargetAmount, feeAmount);
    }

    public Task<IReadOnlyDictionary<(CurrencyCode Source, CurrencyCode Target), decimal>> GetAllExchangeRatesAsync()
    {
        var result = new Dictionary<(CurrencyCode Source, CurrencyCode Target), decimal>();

        // ایجاد ماتریس نرخ‌های تبدیل برای همه جفت‌های ارزی
        foreach (var sourceCurrency in _baseRates.Keys)
        {
            foreach (var targetCurrency in _baseRates.Keys)
            {
                if (sourceCurrency == targetCurrency)
                    continue;

                var sourceToIRR = _baseRates[sourceCurrency];
                var targetToIRR = _baseRates[targetCurrency];
                var rate = sourceToIRR / targetToIRR;

                result.Add((sourceCurrency, targetCurrency), rate);
            }
        }

        return Task.FromResult<IReadOnlyDictionary<(CurrencyCode Source, CurrencyCode Target), decimal>>(result);
    }
}