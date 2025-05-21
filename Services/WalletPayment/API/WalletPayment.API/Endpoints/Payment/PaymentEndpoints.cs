using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Commands.CreateIntegratedPurchase;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Application.Transactions.Commands.IntegratedPurchase;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Payment;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/create", async (
            [FromBody] CreatePaymentRequest request,
            IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentService.CreatePaymentRequestAsync(
                request.UserId,
                request.Amount,
                request.Currency,
                request.Description,
                request.GatewayType,
                request.CallbackUrl,
                request.Metadata,
                request.OrderId,
                request.CancelUrl,
                cancellationToken);

            if (result.IsSuccessful)
            {
                return Results.Ok(new
                {
                    IsSuccessful = true,
                    Authority = result.Authority,
                    PaymentUrl = result.PaymentUrl
                });
            }
            else
            {
                return Results.BadRequest(new
                {
                    IsSuccessful = false,
                    ErrorMessage = result.ErrorMessage,
                    ErrorCode = result.ErrorCode
                });
            }
        })
        .WithTags("Payments")
        .RequireAuthorization();

        app.MapGet("/payments/{paymentId:guid}", async (
            Guid paymentId,
            Guid userId,
            IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var paymentDetails = await paymentService.GetPaymentDetailsAsync(
                    paymentId, userId, cancellationToken);
                return Results.Ok(paymentDetails);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { Message = "پرداخت مورد نظر یافت نشد" });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithTags("Payments")
        .RequireAuthorization();

        app.MapGet("/users/{userId:guid}/payments", async (
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            IPaymentService paymentService = null,
            CancellationToken cancellationToken = default) =>
        {
            var paymentHistory = await paymentService.GetPaymentHistoryAsync(
                userId, pageNumber, pageSize, cancellationToken);
            return Results.Ok(paymentHistory);
        })
        .WithTags("Payments")
        .RequireAuthorization();

        app.MapPost("/payments/{paymentId:guid}/refund", async (
            Guid paymentId,
            [FromBody] RefundPaymentRequest request,
            IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var result = await paymentService.RefundPaymentAsync(
                paymentId,
                request.UserId,
                request.Reason,
                request.Amount,
                cancellationToken);

            if (result.IsSuccessful)
            {
                return Results.Ok(new
                {
                    IsSuccessful = true,
                    RefundTrackingId = result.RefundTrackingId
                });
            }
            else
            {
                return Results.BadRequest(new
                {
                    IsSuccessful = false,
                    ErrorMessage = result.ErrorMessage,
                    ErrorCode = result.ErrorCode
                });
            }
        })
        .WithTags("Payments")
        .RequireAuthorization();
       
    }

}

// مدل‌های درخواست
public class CreatePaymentRequest
{
    public Guid UserId { get; set; }
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
