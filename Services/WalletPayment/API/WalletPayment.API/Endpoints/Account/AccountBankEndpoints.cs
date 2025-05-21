using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WalletPayment.Application.Accounts.Commands.CreateBankAccount;
using WalletPayment.Application.Accounts.Commands.DeleteBankAccount;
using WalletPayment.Application.Accounts.Queries.GetUserBankAccounts;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.API.Endpoints.Account;

public class AccountBankEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // 1. دریافت لیست حساب‌های بانکی کاربر
        app.MapGet("/wallets/bank-accounts", async (
            [FromServices] ICurrentUserService currentUserService,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetUserBankAccountsQuery(currentUserService.GetCurrentUserId()), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("BankAccounts")
        .RequireAuthorization();

        // 2. افزودن حساب بانکی جدید
        app.MapPost("/wallets/bank-accounts", async (
            [FromServices] ICurrentUserService currentUserService,            
            [FromBody] CreateBankAccountCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            //if (command.UserId != currentUserService.GetCurrentUserId())
            //{
            //    return Results.BadRequest("شناسه کاربر در مسیر و بدنه درخواست باید یکسان باشد");
            //}

            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/wallets/{currentUserService.GetCurrentUserId()}/bank-accounts/{result.BankAccountId}", result);
        })
        .WithTags("BankAccounts")
        .RequireAuthorization();

        // 3. حذف حساب بانکی
        app.MapDelete("/wallets/bank-accounts/{accountId:guid}", async (
            [FromServices] ICurrentUserService currentUserService,                        
            Guid accountId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteBankAccountCommand(currentUserService.GetCurrentUserId(), accountId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .WithTags("BankAccounts")
        .RequireAuthorization();
    }
}