using Application.Transactions.Commands.ConvertCurrency;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WalletPayment.Application.Currencies.Queries.GetConversionPreview;
using WalletPayment.Application.Currencies.Queries.GetExchangeRates;

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
       .WithName("GetExchangeRates")
       .WithMetadata(new SwaggerOperationAttribute(
            summary: "دریافت نرخ‌های تبدیل ارز",
            description: "این endpoint لیست تمام نرخ‌های تبدیل بین ارزهای مختلف را برمی‌گرداند"
        ))
        .AllowAnonymous();

        app.MapPost("/wallets/convert-currency/preview", async (
            ISender sender,
            CancellationToken cancellationToken,
            [FromBody] ConversionPreviewRequest request
        ) =>
        {
            var query = new GetConversionPreviewQuery
            {
                SourceAmount = request.SourceAmount,
                SourceCurrency = request.SourceCurrency,
                TargetCurrency = request.TargetCurrency
            };

            var previewResult = await sender.Send(query, cancellationToken);
            return Results.Ok(previewResult);
        })
        .WithTags("Currency")
        .WithName("PreviewCurrencyConversion")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "پیش‌نمایش تبدیل ارز",
            description: "نتایج تبدیل ارز را قبل از انجام عملیات واقعی نمایش می‌دهد"
        ))
        .RequireAuthorization();
        // endpoint اصلی برای تبدیل ارز
        app.MapPost("/wallets/convert-currency", async (
            ISender sender,
            CancellationToken cancellationToken,
            [FromBody] ConvertCurrencyCommand command
        ) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Currency")
        .WithName("ConvertCurrency")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "تبدیل ارز در کیف پول",
            description: "عملیات تبدیل ارز بین حساب‌های مختلف کیف پول"
        ))
        .RequireAuthorization();
    }
}