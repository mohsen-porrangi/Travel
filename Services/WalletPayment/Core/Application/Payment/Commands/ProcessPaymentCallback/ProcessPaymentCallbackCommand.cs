using BuildingBlocks.CQRS;
using WalletPayment.Application.Payment.Models;

namespace WalletPayment.Application.Payment.Commands.ProcessPaymentCallback;

public record ProcessPaymentCallbackCommand(PaymentCallbackParameters Parameters) : ICommand<PaymentCallbackResult>;

public class PaymentCallbackResult
{
    public bool IsSuccessful { get; set; }
    public string RedirectUrl { get; set; } = string.Empty;
    public Guid? TransactionId { get; set; }
    public string? ReferenceId { get; set; }
    public string? ErrorMessage { get; set; }
}