using BuildingBlocks.CQRS;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Currencies.Queries.GetConversionPreview;

public class GetConversionPreviewQueryHandler(ICurrencyExchangeService currencyExchangeService)
    : IQueryHandler<GetConversionPreviewQuery, ConversionPreviewResponse>
{
    public async Task<ConversionPreviewResponse> Handle(GetConversionPreviewQuery request, CancellationToken cancellationToken)
    {
        // دریافت نرخ تبدیل
        var exchangeRate = await currencyExchangeService.GetExchangeRateAsync(
            request.SourceCurrency, request.TargetCurrency);

        // محاسبه مبلغ نهایی و کارمزد
        var (targetAmount, feeAmount) = await currencyExchangeService.CalculateConversionAsync(
            request.SourceAmount, request.SourceCurrency, request.TargetCurrency);

        // محاسبه نرخ کارمزد
        var feeRate = 0.01m; // 1%

        // زمان اعتبار پیش‌نمایش (معمولاً 15 دقیقه)
        var previewValidUntil = DateTime.UtcNow.AddMinutes(15);

        return new ConversionPreviewResponse
        {
            SourceAmount = request.SourceAmount,
            TargetAmount = targetAmount,
            SourceCurrency = request.SourceCurrency,
            TargetCurrency = request.TargetCurrency,
            ExchangeRate = exchangeRate,
            FeeAmount = feeAmount,
            FeeRate = feeRate,
            PreviewValidUntil = previewValidUntil
        };
    }
}