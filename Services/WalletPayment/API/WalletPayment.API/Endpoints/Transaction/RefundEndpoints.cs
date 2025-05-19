using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Transactions.Commands.RefundTransaction;
using WalletPayment.Application.Transactions.Queries.GetRefundableTransaction;

namespace WalletPayment.API.Endpoints.Transaction;

public class RefundEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/transactions/{transactionId:guid}/refund-info", async (
            Guid transactionId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetRefundableTransactionQuery(transactionId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Refunds")
        .RequireAuthorization();

        app.MapPost("/transactions/refund", async (
            [FromBody] RefundTransactionCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Refunds")
        .RequireAuthorization();
    }
}