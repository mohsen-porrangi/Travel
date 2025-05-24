using BuildingBlocks.Contracts;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.API.Endpoints.Transaction.Internal;

public class RefundInternalEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // 1. اندپوینت بررسی قابلیت استرداد هر نوع شناسه
        app.MapGet("api/internal/refunds/check", async (
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
    }
}