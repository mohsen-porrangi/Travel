using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Payment.Models;

namespace WalletPayment.Application.Payment.Services;
public class PaymentCallbackProcessor : IPaymentCallbackProcessor
{
    private readonly ILogger<PaymentCallbackProcessor> _logger;

    public PaymentCallbackProcessor(ILogger<PaymentCallbackProcessor> logger)
    {
        _logger = logger;
    }

    public PaymentCallbackParameters ExtractParameters(IQueryCollection query, string? gatewayType)
    {
        var parameters = new PaymentCallbackParameters();

        // تشخیص نوع درگاه یا استفاده از پارامتر ورودی
        var gateway = gatewayType?.ToLower() ?? "";

        if (gateway == "zarinpal" || query.ContainsKey("Authority"))
        {
            // ZarinPal format
            parameters.Authority = query["Authority"].ToString();
            parameters.Status = query["Status"].ToString();
            if (Guid.TryParse(query["userId"].ToString(), out var userId))
                parameters.UserId = userId;
            decimal.TryParse(query["Amount"].ToString(), out decimal amount);
            parameters.Amount = amount;
            parameters.OrderId = query["orderId"].ToString();
        }
        else if (gateway == "zibal" || query.ContainsKey("trackId"))
        {
            // Zibal format
            parameters.Authority = query["trackId"].ToString();
            parameters.Status = query["success"].ToString() == "1" ? "OK" : "NOK";
            if (Guid.TryParse(query["userId"].ToString(), out var userId))
                parameters.UserId = userId;
            decimal.TryParse(query["amount"].ToString(), out decimal amount);
            parameters.Amount = amount;
            parameters.OrderId = query["orderId"].ToString();
        }
        else if (gateway == "sandbox" || query.ContainsKey("authority"))
        {
            // Sandbox format (lowercase 'authority')
            parameters.Authority = query["authority"].ToString();
            parameters.Status = query["status"].ToString();
            if (Guid.TryParse(query["userId"].ToString(), out var userId))
                parameters.UserId = userId;
            decimal.TryParse(query["amount"].ToString(), out decimal amount);
            parameters.Amount = amount;
            parameters.OrderId = query["orderId"].ToString();
        }
        else
        {
            _logger.LogWarning("فرمت کالبک ناشناخته: {QueryString}", query.ToString());
            throw new ArgumentException("پارامترهای کالبک نامعتبر است");
        }

        // تعیین نوع پرداخت (یکپارچه یا عادی)
        parameters.IsIntegrated = !string.IsNullOrEmpty(query["integrated"].ToString()) &&
                                 query["integrated"].ToString().ToLower() == "true";

        return parameters;
    }
}