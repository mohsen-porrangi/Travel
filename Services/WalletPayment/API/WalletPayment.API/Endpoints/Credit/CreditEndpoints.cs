// مسیر: Services/WalletPayment/API/WalletPayment.API/Endpoints/Credit/CreditEndpoints.cs

using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.API.Models.Credit;
using WalletPayment.API.Services;
using WalletPayment.Application.Credit.Commands.AssignCredit;
using WalletPayment.Application.Credit.Commands.SettleCredit;
using WalletPayment.Application.Credit.Queries.GetCreditStatus;
using WalletPayment.Application.Transactions.Commands.CreditPurchase;
using WalletPayment.Domain.Entities.Enums;
using BuildingBlocks.Contracts;

namespace WalletPayment.API.Endpoints.Credit;

public class CreditEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // دریافت وضعیت اعتبار کاربر جاری
        app.MapGet("/wallets/credit", async (
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var result = await sender.Send(new GetCreditStatusQuery(userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetCreditStatus")
.WithDescription("دریافت اطلاعات اعتبار فعلی، حد اعتباری و تاریخ سررسید کاربر")
.Produces<CreditStatusResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status404NotFound)
.WithTags("Credit")
        .RequireAuthorization();

        // تخصیص اعتبار به کاربر
        app.MapPost("/wallets/credit/assign", async (
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] AssignCreditRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var command = new AssignCreditCommand(
                userId,
                request.Amount,
                request.DueDate,
                request.Description);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("AssignCredit")
.WithDescription("تخصیص اعتبار جدید به کاربر (فقط برای کاربران B2B)")
.Produces<AssignCreditResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status403Forbidden)
.WithTags("Credit")
        .RequireAuthorization();

        // خرید اعتباری
        app.MapPost("/wallets/credit/purchase", async (
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] CreditPurchaseRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var command = new CreditPurchaseCommand(
                userId,
                request.Amount,
                request.Currency,
                request.OrderId,
                request.Description);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("CreditPurchase")
.WithDescription("انجام خرید با استفاده از اعتبار موجود")
.Produces<CreditPurchaseResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithTags("Credit")
        .RequireAuthorization();

        // تسویه اعتبار
        app.MapPost("/wallets/credit/settle", async (
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] SettleCreditRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();

            var command = new SettleCreditCommand(
                userId,
                request.PaymentReferenceId);

            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("SettleCredit")
.WithDescription("تسویه و پرداخت اعتبار مصرف شده")
.Produces<SettleCreditResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.ProducesProblem(StatusCodes.Status400BadRequest)
.WithTags("Credit")
        .RequireAuthorization();
    }
}