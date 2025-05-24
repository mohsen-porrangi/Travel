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
        app.MapGet("api/currency/rates", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetExchangeRatesQuery(), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetExchangeRates")
        .WithDescription("دریافت آخرین نرخ‌های تبدیل بین تمام ارزهای پشتیبانی شده")
        .Produces<ExchangeRatesResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags("Currency")
        .AllowAnonymous();

        app.MapPost("api/wallets/convert-currency/preview", async (
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
        .WithName("PreviewCurrencyConversion")
        .WithDescription("محاسبه و نمایش نتیجه تبدیل ارز قبل از انجام عملیات")
        .Produces<ConversionPreviewResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithTags("Currency")
        .RequireAuthorization();
        // endpoint اصلی برای تبدیل ارز
        app.MapPost("api/wallets/convert-currency", async (
            ISender sender,
            CancellationToken cancellationToken,
            [FromBody] ConvertCurrencyCommand command
        ) =>
        {
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ConvertCurrency")
        .WithDescription("تبدیل ارز بین حساب‌های مختلف در کیف پول")
        .Produces<ConvertCurrencyResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags("Currency")
        .RequireAuthorization();
    }
}