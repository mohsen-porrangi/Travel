using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletPayment.Application.Transactions.Commands.RefundTransaction;
using WalletPayment.Application.Transactions.Queries.GetRefundableTransaction;

namespace WalletPayment.API.Endpoints.Transaction;
public class RefundEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // endpoint برای بررسی امکان استرداد یک تراکنش
        app.MapGet("/transactions/{transactionId:guid}/refundability", async (
            Guid transactionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRefundableTransactionQuery(transactionId);
            var refundInfo = await sender.Send(query, cancellationToken);
            return Results.Ok(refundInfo);
        })
        .WithTags("Refunds")
        .WithName("CheckRefundability")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "بررسی امکان استرداد تراکنش",
            description: "بررسی می‌کند که آیا تراکنش مورد نظر قابل استرداد است و جزئیات آن را برمی‌گرداند"
        ))
        .RequireAuthorization();

        // endpoint برای انجام عملیات استرداد
        app.MapPost("/transactions/{transactionId:guid}/refund", async (
            Guid transactionId,
            [FromBody] RefundTransactionRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RefundTransactionCommand
            {
                OriginalTransactionId = transactionId,
                Amount = request.Amount,
                Reason = request.Reason,
                IsAdminApproved = request.IsAdminApproved
            };

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Refunds")
        .WithName("RefundTransaction")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "استرداد وجه تراکنش",
            description: "عملیات استرداد وجه تراکنش را انجام می‌دهد"
        ))
        .RequireAuthorization();
    }
}

// مدل درخواست برای استرداد تراکنش
//TODO find good place for this class
public class RefundTransactionRequest
{
    public decimal? Amount { get; init; } // null برای استرداد کامل
    public string Reason { get; init; } = "استرداد وجه";
    public bool IsAdminApproved { get; init; } = false; // آیا توسط ادمین تأیید شده است
}