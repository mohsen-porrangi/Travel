using Application.Transactions.Commands.ConvertCurrency;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WalletPayment.Application.Currencies.Queries.GetConversionPreview;
using WalletPayment.Application.Currencies.Queries.GetExchangeRates;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.API.Endpoints.Currency;

public class CurrencyEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/currency/rates", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetExchangeRatesQuery(), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Currency")
        .AllowAnonymous();

        app.MapGet("/currency/preview", async (
            [FromQuery] decimal sourceAmount,
            [FromQuery] CurrencyCode sourceCurrency,
            [FromQuery] CurrencyCode targetCurrency,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetConversionPreviewQuery
            {
                SourceAmount = sourceAmount,
                SourceCurrency = sourceCurrency,
                TargetCurrency = targetCurrency
            };

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Currency")
        .AllowAnonymous();

        app.MapPost("/wallets/convert-currency", async (
            [FromBody] ConvertCurrencyCommand command,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Currency")
        .RequireAuthorization();
    }
}