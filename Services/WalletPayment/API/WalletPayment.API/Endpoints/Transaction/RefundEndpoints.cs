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
        app.MapPost("/transactions/refund", async (
            ISender sender,
            CancellationToken cancellationToken,
            [FromBody] RefundTransactionCommand command,
            [FromQuery][SwaggerParameter(Description = "اگر true باشد، فقط امکان استرداد بررسی می‌شود بدون انجام عملیات استرداد")] bool checkOnly = false
           ) =>
        {
            if (checkOnly)
            {
                // بررسی امکان استرداد بدون انجام عملیات واقعی
                var query = new GetRefundableTransactionQuery(command.OriginalTransactionId);
                var refundInfo = await sender.Send(query, cancellationToken);
                return Results.Ok(refundInfo);
            }
            else
            {
                // انجام عملیات واقعی استرداد
                var result = await sender.Send(command, cancellationToken);
                return Results.Ok(result);
            }
        })
        .WithTags("Refunds")
        .WithName("RefundTransaction")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "استرداد وجه تراکنش",
            description: "این endpoint برای استرداد وجه یک تراکنش استفاده می‌شود. با استفاده از پارامتر checkOnly می‌توان فقط امکان استرداد را بررسی کرد."
        ))
        .RequireAuthorization();
    }
}