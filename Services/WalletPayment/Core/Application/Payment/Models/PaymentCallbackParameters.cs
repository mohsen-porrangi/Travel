using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Payment.Models;

public class PaymentCallbackParameters
{
    public string Authority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public CurrencyCode Currency { get; set; } = CurrencyCode.IRR;
    public bool IsIntegrated { get; set; } = false;
}