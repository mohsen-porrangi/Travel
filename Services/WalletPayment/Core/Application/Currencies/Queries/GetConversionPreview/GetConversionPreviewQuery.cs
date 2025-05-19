using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Currencies.Queries.GetConversionPreview;

public record GetConversionPreviewQuery : IQuery<ConversionPreviewResponse>
{
    public decimal SourceAmount { get; init; }
    public CurrencyCode SourceCurrency { get; init; }
    public CurrencyCode TargetCurrency { get; init; }
}

public record ConversionPreviewResponse
{
    public decimal SourceAmount { get; init; }
    public decimal TargetAmount { get; init; }
    public CurrencyCode SourceCurrency { get; init; }
    public CurrencyCode TargetCurrency { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal FeeAmount { get; init; }
    public decimal FeeRate { get; init; }
    public DateTime PreviewValidUntil { get; init; }
}