using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace Application.Transactions.Commands.ConvertCurrency;

public record ConvertCurrencyCommand : ICommand<ConvertCurrencyResponse>
{
    public Guid UserId { get; init; }
    public decimal SourceAmount { get; init; }
    public CurrencyCode SourceCurrency { get; init; }
    public CurrencyCode TargetCurrency { get; init; }
}

public record ConvertCurrencyResponse
{
    public Guid ConversionId { get; init; }
    public Guid SourceTransactionId { get; init; }
    public Guid TargetTransactionId { get; init; }
    public decimal SourceAmount { get; init; }
    public decimal TargetAmount { get; init; }
    public CurrencyCode SourceCurrency { get; init; }
    public CurrencyCode TargetCurrency { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal FeeAmount { get; init; }
    public DateTime ConversionDate { get; init; }
}