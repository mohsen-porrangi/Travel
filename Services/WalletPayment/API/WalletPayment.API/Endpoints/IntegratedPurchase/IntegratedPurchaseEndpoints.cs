using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Transactions.Commands.IntegratedPurchase;

namespace WalletPayment.API.Endpoints.IntegratedPurchase;

public class IntegratedPurchaseEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/integrated-purchase", async (
            [FromBody] IntegratedPurchaseCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("IntegratedPurchase")
        .RequireAuthorization();
    }
}