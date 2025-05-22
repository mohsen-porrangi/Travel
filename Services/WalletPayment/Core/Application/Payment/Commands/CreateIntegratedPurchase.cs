using BuildingBlocks.CQRS;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Payment.Commands.CreateIntegratedPurchase;

public record CreateIntegratedPurchaseCommand : ICommand<CreateIntegratedPurchaseResponse>
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; }
    public string Description { get; init; }
    public PaymentGatewayType GatewayType { get; init; }
    public string CallbackUrl { get; init; }
    public string OrderId { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

public record CreateIntegratedPurchaseResponse
{
    public bool IsSuccessful { get; init; }
    public string? PaymentUrl { get; init; }
    public string? Authority { get; init; }
    public decimal CurrentBalance { get; init; }
    public decimal AmountFromWallet { get; init; }
    public decimal AmountToPay { get; init; }
    public decimal TotalAmount { get; init; }
    public bool UseWalletBalance { get; init; }
    public string? ErrorMessage { get; init; }
}