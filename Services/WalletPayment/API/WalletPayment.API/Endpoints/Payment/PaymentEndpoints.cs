using BuildingBlocks.Contracts;
using Carter;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.API.Models.Payment;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;

public class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // 1. ایجاد درخواست پرداخت
        app.MapPost("api/payments", async (
            [FromBody] CreatePaymentRequest request,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var result = await paymentService.CreatePaymentRequestAsync(
                userId, 
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
        .WithName("CreatePayment")
        .WithDescription("ایجاد درخواست پرداخت جدید و دریافت لینک هدایت به درگاه")
        .Produces<PaymentRequestResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Payments")
        .RequireAuthorization();

        // 2. دریافت جزئیات پرداخت
        app.MapGet("api/payments/{paymentId:guid}", async (
            Guid paymentId,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IPaymentService paymentService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

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
        .WithName("GetPaymentDetails")
        .WithDescription("دریافت اطلاعات کامل یک پرداخت خاص")
        .Produces<PaymentDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithTags("Payments")
        .RequireAuthorization();

        // 3. دریافت تاریخچه پرداخت‌های کاربر جاری
        app.MapGet("api/payments/history", async (
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IPaymentService paymentService,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,           
            CancellationToken cancellationToken = default) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var paymentHistory = await paymentService.GetPaymentHistoryAsync(
                userId, pageNumber, pageSize, cancellationToken);
            return Results.Ok(paymentHistory);
        })
        .WithName("GetPaymentHistory")
        .WithDescription("دریافت تاریخچه تمام پرداخت‌های انجام شده توسط کاربر با صفحه‌بندی")
        .Produces<PaymentHistoryResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .WithTags("Payments")
        .RequireAuthorization();
    }
}