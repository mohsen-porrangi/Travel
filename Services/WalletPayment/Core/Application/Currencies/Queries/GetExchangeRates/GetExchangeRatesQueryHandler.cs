using BuildingBlocks.CQRS;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Currencies.Queries.GetExchangeRates;

public class GetExchangeRatesQueryHandler(ICurrencyExchangeService currencyExchangeService)
    : IQueryHandler<GetExchangeRatesQuery, ExchangeRatesResponse>
{
    public async Task<ExchangeRatesResponse> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        // دریافت تمام نرخ‌های تبدیل ارز
        var allRates = await currencyExchangeService.GetAllExchangeRatesAsync();

        // تبدیل به DTO
        var ratesDto = allRates.Select(rate => new ExchangeRateDto
        {
            SourceCurrency = rate.Key.Source,
            TargetCurrency = rate.Key.Target,
            Rate = rate.Value,
            EstimatedFeeRate = 0.01m // نرخ کارمزد ثابت (1%)
        }).ToList();

        return new ExchangeRatesResponse
        {
            Rates = ratesDto,
            Timestamp = DateTime.UtcNow
        };
    }
}