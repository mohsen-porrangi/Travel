using BuildingBlocks.Contracts;
using BuildingBlocks.Contracts.Services;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Transactions.Queries.Common;
using WalletPayment.Application.Transactions.Queries.ExportTransactions;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Transaction;

public class ExportTransactionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/wallets/transactions/export", async (
            [FromServices] ICurrentUserService currentUserService,
            ISender sender,
        //TODO    CancellationToken cancellationToken,  // پارامتر اجباری بدون مقدار پیش‌فرض
            [FromQuery] ExportFormat format = ExportFormat.Csv,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] TransactionDirection? direction = null,
            [FromQuery] Domain.Entities.Enums.TransactionType? type = null,
            [FromQuery] TransactionStatus? status = null,
            [FromQuery] CurrencyCode? currency = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] bool? isCredit = null) =>
        {
            var filter = new TransactionFilter
            {
                StartDate = startDate,
                EndDate = endDate,
                Direction = direction,
                Type = type,
                Status = status,
                Currency = currency,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                IsCredit = isCredit
            };

            var query = new ExportTransactionsQuery
            {
                UserId = currentUserService.GetCurrentUserId(),
                Format = format,
                Filter = filter
            };

            var result = await sender.Send(query/* TODO , cancellationToken*/);

            return Results.File(
                result.FileContents,
                result.ContentType,
                result.FileName);
        })
        .WithName("ExportTransactions")
        .WithDescription("دریافت فایل خروجی تراکنش‌ها در فرمت‌های مختلف (CSV, Excel, PDF)")
        .Produces<FileResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Transactions")
        .RequireAuthorization();
    }
}