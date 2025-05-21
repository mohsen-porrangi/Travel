using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Models.Credit;

public record AssignCreditRequest(
    decimal Amount,
    DateTime DueDate,
    string Description = "");

public record CreditPurchaseRequest(
    decimal Amount,
    CurrencyCode Currency = CurrencyCode.IRR,
    string OrderId = "",
    string Description = "");

public record SettleCreditRequest(
    string PaymentReferenceId);
