using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Credit.Commands.AssignCredit;
using WalletPayment.Application.Credit.Commands.SettleCredit;
using WalletPayment.Application.Credit.Queries.GetCreditStatus;
using WalletPayment.Application.Transactions.Commands.CreditPurchase;

namespace WalletPayment.API.Endpoints.Credit;

public class CreditEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/wallets/{userId:guid}/credit", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCreditStatusQuery(userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Credit")
        .RequireAuthorization();

        app.MapPost("/wallets/credit/assign", async (
            [FromBody] AssignCreditCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Credit")
        .RequireAuthorization();

        app.MapPost("/wallets/credit/purchase", async (
            [FromBody] CreditPurchaseCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Credit")
        .RequireAuthorization();

        app.MapPost("/wallets/credit/settle", async (
            [FromBody] SettleCreditCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Credit")
        .RequireAuthorization();
    }
}