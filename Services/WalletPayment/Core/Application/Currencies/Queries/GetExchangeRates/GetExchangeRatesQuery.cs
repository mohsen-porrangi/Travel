using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Currencies.Queries.GetExchangeRates;

public record GetExchangeRatesQuery : IQuery<ExchangeRatesResponse>;

public record ExchangeRatesResponse
{
    public List<ExchangeRateDto> Rates { get; init; } = new();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record ExchangeRateDto
{
    public CurrencyCode SourceCurrency { get; init; }
    public CurrencyCode TargetCurrency { get; init; }
    public decimal Rate { get; init; }
    public decimal EstimatedFeeRate { get; init; }
}