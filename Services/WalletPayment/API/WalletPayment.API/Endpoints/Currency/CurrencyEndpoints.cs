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

        app.MapPost("/wallets/convert-currency", async (
             ISender sender,
            CancellationToken cancellationToken,
            [FromBody] ConvertCurrencyCommand command,
            [FromQuery] bool preview = false
           ) =>
        {
            if (preview)
            {
                // اگر فقط پیش‌نمایش نیاز است
                var previewQuery = new GetConversionPreviewQuery
                {
                    SourceAmount = command.SourceAmount,
                    SourceCurrency = command.SourceCurrency,
                    TargetCurrency = command.TargetCurrency
                };
                var previewResult = await sender.Send(previewQuery, cancellationToken);
                return Results.Ok(previewResult);
            }
            else
            {
                // انجام عملیات واقعی تبدیل
                var result = await sender.Send(command, cancellationToken);
                return Results.Ok(result);
            }
        })
                .WithTags("Currency")
        .WithName("ConvertCurrency")
        .WithMetadata(new SwaggerOperationAttribute(
            summary: "تبدیل ارز در کیف پول",
            description: "این endpoint برای تبدیل ارز بین حساب‌های مختلف کیف پول استفاده می‌شود. با استفاده از پارامتر preview می‌توان نتایج تبدیل را قبل از انجام آن مشاهده کرد."
        ))
        .RequireAuthorization();
    }
}