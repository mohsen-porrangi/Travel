using Application.Common.Contracts;
using Carter;
using Domain.Entities.Account;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Accounts.Queries.GetAccountBalanceHistory;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Account;

public class AccountSnapshotEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
  

        app.MapGet("/accounts/{accountId:guid}/snapshots", async (
            Guid accountId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] SnapshotType? type,
            IAccountSnapshotService snapshotService,
            CancellationToken cancellationToken) =>
        {
            var snapshots = await snapshotService.GetAccountSnapshotsAsync(
                accountId, startDate, endDate, type, cancellationToken);
            return Results.Ok(snapshots);
        })
        .WithTags("AccountSnapshots")
        .RequireAuthorization();


        app.MapGet("/accounts/{accountId:guid}/balance-history", async (
            Guid accountId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] SnapshotType? type,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAccountBalanceHistoryQuery(accountId, startDate, endDate, type);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
            .WithTags("AccountSnapshots")
            .RequireAuthorization();
    }
}