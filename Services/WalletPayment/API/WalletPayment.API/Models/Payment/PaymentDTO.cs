using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Models.Payment;


// مدل‌های درخواست
public class CreatePaymentRequest
{    
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.IRR;
    public string Description { get; set; } = string.Empty;
    public PaymentGatewayType GatewayType { get; set; } = PaymentGatewayType.ZarinPal;
    public string CallbackUrl { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
    public string? OrderId { get; set; }
    public string? CancelUrl { get; set; }
}

public class RefundPaymentRequest
{
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
}