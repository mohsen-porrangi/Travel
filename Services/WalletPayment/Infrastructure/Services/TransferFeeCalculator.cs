using Microsoft.Extensions.Configuration;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.Services;

public class TransferFeeCalculator(IConfiguration configuration) : ITransferFeeCalculator
{
    private readonly IConfiguration _configuration = configuration;

    public decimal CalculateTransferFee(decimal amount, CurrencyCode currency)
    {
        // خواندن نرخ کارمزد از تنظیمات
        var feeRateStr = _configuration["Fees:TransferFeeRate"] ?? "0.005"; // پیش‌فرض 0.5%
        if (!decimal.TryParse(feeRateStr, out var feeRate))
            feeRate = 0.005m; // پیش‌فرض در صورت عدم خواندن موفق

        // خواندن حداقل کارمزد از تنظیمات
        var minFeeStr = _configuration["Fees:MinTransferFee"] ?? "1000"; // پیش‌فرض 1000 ریال
        if (!decimal.TryParse(minFeeStr, out var minFee))
            minFee = 1000m;

        // خواندن حداکثر کارمزد از تنظیمات
        var maxFeeStr = _configuration["Fees:MaxTransferFee"] ?? "50000"; // پیش‌فرض 50000 ریال
        if (!decimal.TryParse(maxFeeStr, out var maxFee))
            maxFee = 50000m;

        // محاسبه کارمزد بر اساس مبلغ انتقال
        var calculatedFee = amount * feeRate;

        // اعمال حداقل و حداکثر
        calculatedFee = Math.Max(calculatedFee, minFee);
        calculatedFee = Math.Min(calculatedFee, maxFee);

        // تنظیم کارمزد برای ارزهای مختلف
        if (currency != CurrencyCode.IRR)
        {
            // برای ارزهای خارجی، کارمزد را متناسب با نرخ تبدیل تنظیم می‌کنیم
            var conversionRate = GetConversionRateToIRR(currency);
            calculatedFee = calculatedFee / conversionRate;

            // گرد کردن تا 2 رقم اعشار برای ارزهای خارجی
            calculatedFee = Math.Round(calculatedFee, 2);
        }
        else
        {
            // گرد کردن به عدد صحیح برای ریال
            calculatedFee = Math.Round(calculatedFee, 0);
        }

        return calculatedFee;
    }

    private decimal GetConversionRateToIRR(CurrencyCode currency)
    {
        // در یک سیستم واقعی، این نرخ‌ها می‌تواند از یک سرویس نرخ ارز دریافت شود
        return currency switch
        {
            CurrencyCode.USD => 500000m, // نرخ دلار به ریال
            CurrencyCode.EUR => 550000m, // نرخ یورو به ریال
            CurrencyCode.GBP => 650000m, // نرخ پوند به ریال
            CurrencyCode.AED => 135000m, // نرخ درهم به ریال
            _ => 1m // پیش‌فرض
        };
    }
}