using BuildingBlocks.Contracts;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.API.Endpoints.Refund;

public class RefundEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // 1. اندپوینت بررسی قابلیت استرداد هر نوع شناسه
        app.MapGet("/refunds/check", async (
            [FromQuery] Guid? transactionId,
            [FromQuery] Guid? paymentId,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IRefundService refundService,            
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            // بررسی اعتبار پارامترها
            if (transactionId == null && paymentId == null)
                return Results.BadRequest("باید حداقل یکی از شناسه تراکنش یا شناسه پرداخت ارائه شود");

            // بررسی قابلیت استرداد
            var result = await refundService.CheckRefundabilityAsync(
                userId,
                transactionId,
                paymentId,
                cancellationToken);

            return Results.Ok(result);
        })
        .WithName("CheckRefundability")
.WithDescription("بررسی قابلیت استرداد تراکنش یا پرداخت و مشاهده جزئیات استرداد")
.Produces<RefundabilityResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithTags("Refunds")
        .RequireAuthorization();

        // 2. اندپوینت انجام استرداد
        app.MapPost("/refunds", async (
            [FromBody] CreateRefundRequest request,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IRefundService refundService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            // بررسی اعتبار پارامترها
            if (request.TransactionId == null && request.PaymentId == null)
                return Results.BadRequest("باید حداقل یکی از شناسه تراکنش یا شناسه پرداخت ارائه شود");

            // انجام استرداد
            var result = await refundService.ProcessRefundAsync(
                userId,
                request.TransactionId,
                request.PaymentId,
                request.Amount,
                request.Reason,
                request.IsAdminApproved,
                cancellationToken);

            return Results.Ok(result);
        })
        .WithName("CreateRefund")
.WithDescription("انجام عملیات استرداد کامل یا جزئی تراکنش/پرداخت")
.Produces<RefundResult>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithTags("Refunds")
        .RequireAuthorization();
    }
}