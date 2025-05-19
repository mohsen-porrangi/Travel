namespace WalletPayment.Infrastructure.ExternalServices.PaymentGateway;

public class PaymentGatewaySettings
{
    public string DefaultGateway { get; set; } = "ZarinPal";
    public Dictionary<string, GatewayConfig> Gateways { get; set; } = new();
}

public class GatewayConfig
{
    public string MerchantId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public bool IsSandbox { get; set; } = false;
    public Dictionary<string, string> AdditionalSettings { get; set; } = new();
}