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
        // تنها endpoint برای تاریخچه موجودی حساب
        app.MapGet("/accounts/{accountId:guid}/balance-history", async (
            Guid accountId,
            ISender sender,
            CancellationToken cancellationToken,
            [FromQuery] DateTime? startDate,  // اختیاری شده
            [FromQuery] DateTime? endDate,    // اختیاری شده
            [FromQuery] SnapshotType? type,
            [FromQuery] bool detailed = true // پارامتر جدید برای کنترل سطح جزئیات
            ) =>
        {
            // اگر تاریخ‌ها تعیین نشده باشند، مقادیر پیش‌فرض تنظیم می‌شوند
            startDate ??= DateTime.UtcNow.AddMonths(-1);
            endDate ??= DateTime.UtcNow;

            var query = new GetAccountBalanceHistoryQuery(accountId, startDate.Value, endDate.Value, type, detailed);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAccountBalanceHistory")
.WithDescription("دریافت تاریخچه و تغییرات موجودی حساب ارزی در بازه زمانی مشخص")
.Produces<AccountBalanceHistoryResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithTags("AccountHistory")
        .RequireAuthorization();
    }
}