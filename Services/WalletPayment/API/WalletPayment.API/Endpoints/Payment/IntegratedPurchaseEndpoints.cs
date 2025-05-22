using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Payment.Commands;
using WalletPayment.Application.Payment.Commands.CreateIntegratedPurchase;
using WalletPayment.Application.Transactions.Commands.IntegratedPurchase;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Payment;

public class IntegratedPurchaseEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Endpoint برای آغاز فرآیند خرید یکپارچه - ایجاد درخواست پرداخت و هدایت به درگاه
        app.MapPost("/payments/integrated", async (
            [FromBody] CreateIntegratedPurchaseRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new Application.Payment.Commands.CreateIntegratedPurchase.CreateIntegratedPurchaseCommand
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                GatewayType = request.GatewayType,
                CallbackUrl = request.CallbackUrl,
                OrderId = request.OrderId,
                Metadata = request.Metadata
            };

            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccessful)
            {
                return Results.Ok(new
                {
                    success = true,
                    paymentUrl = result.PaymentUrl,
                    authority = result.Authority,
                    currentBalance = result.CurrentBalance,
                    amountFromWallet = result.AmountFromWallet,
                    amountToPay = result.AmountToPay
                });
            }
            else
            {
                return Results.BadRequest(new
                {
                    success = false,
                    errorMessage = result.ErrorMessage
                });
            }
        })
        .WithName("CreateIntegratedPurchase")
        .WithDescription("شروع فرآیند خرید یکپارچه با شارژ خودکار کیف پول و هدایت به درگاه")
        .Produces<ExecuteIntegratedPurchaseResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Payments")
        .RequireAuthorization();

        // Endpoint برای اجرای مستقیم خرید یکپارچه (بدون درگاه پرداخت)
        app.MapPost("/payments/integrated/execute", async (
            [FromBody] ExecuteIntegratedPurchaseRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new IntegratedPurchaseCommand(
                request.UserId,
                request.Amount,
                request.Currency,
                request.OrderId,
                request.Description,
                request.PaymentReferenceId,
                request.AutoConvertCurrency);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ExecuteIntegratedPurchase")
.WithDescription("اجرای مستقیم خرید یکپارچه بدون هدایت به درگاه (پس از پرداخت موفق)")
.Produces<ExecuteIntegratedPurchaseResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithTags("Payments")
        .RequireAuthorization();
    }
}

// مدل درخواست برای ایجاد خرید یکپارچه
public class CreateIntegratedPurchaseRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.IRR;
    public string Description { get; set; } = string.Empty;
    public PaymentGatewayType GatewayType { get; set; } = PaymentGatewayType.ZarinPal;
    public string CallbackUrl { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

// مدل درخواست برای اجرای مستقیم خرید یکپارچه
public class ExecuteIntegratedPurchaseRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; } = CurrencyCode.IRR;
    public string OrderId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PaymentReferenceId { get; set; } = string.Empty;
    public bool AutoConvertCurrency { get; set; } = true;
}