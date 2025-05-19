using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.ExternalServices.PaymentGateway;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory; // تغییر به ILoggerFactory
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentGatewayFactory(
        IConfiguration configuration,
        ILoggerFactory loggerFactory, // تغییر به ILoggerFactory
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _httpClientFactory = httpClientFactory;
    }

    public IPaymentGateway CreateGateway(PaymentGatewayType gatewayType)
    {
        return gatewayType switch
        {
            PaymentGatewayType.ZarinPal => new ZarinPalGateway(
                _configuration,
                _loggerFactory.CreateLogger<ZarinPalGateway>(), // استفاده از CreateLogger روی LoggerFactory
                _httpClientFactory),

            PaymentGatewayType.Zibal => new ZibalGateway(
                _configuration,
                _loggerFactory.CreateLogger<ZibalGateway>(), // استفاده از CreateLogger روی LoggerFactory
                _httpClientFactory),

            PaymentGatewayType.Sandbox => new SandboxGateway(
                _configuration,
                _loggerFactory.CreateLogger<SandboxGateway>()), // استفاده از CreateLogger روی LoggerFactory

            _ => throw new ArgumentException($"درگاه پرداخت نامعتبر: {gatewayType}")
        };
    }
}

public interface IPaymentGatewayFactory
{
    IPaymentGateway CreateGateway(PaymentGatewayType gatewayType);
}