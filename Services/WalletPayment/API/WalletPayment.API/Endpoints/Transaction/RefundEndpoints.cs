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
        .WithTags("Refunds")
        .WithName("CheckRefundability")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "بررسی امکان استرداد",
            description: "بررسی می‌کند که آیا تراکنش یا پرداخت مورد نظر قابل استرداد است و جزئیات آن را برمی‌گرداند"
        ))
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
        .WithTags("Refunds")
        .WithName("CreateRefund")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "استرداد وجه",
            description: "عملیات استرداد وجه تراکنش یا پرداخت را انجام می‌دهد"
        ))
        .RequireAuthorization();
    }
}