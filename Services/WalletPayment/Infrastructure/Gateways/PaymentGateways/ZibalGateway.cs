using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.ExternalServices.PaymentGateway;

public class ZibalGateway : IPaymentGateway
{
    private readonly string _merchantId;
    private readonly string _apiBaseUrl;
    private readonly string _paymentBaseUrl;
    private readonly ILogger<ZibalGateway> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ZibalGateway(
        IConfiguration configuration,
        ILogger<ZibalGateway> logger,
        IHttpClientFactory httpClientFactory)
    {
        _merchantId = configuration["Payment:Zibal:MerchantId"] ??
                      throw new ArgumentNullException("Payment:Zibal:MerchantId configuration is missing");
        _apiBaseUrl = configuration["Payment:Zibal:ApiUrl"] ?? "https://gateway.zibal.ir/v1/";
        _paymentBaseUrl = configuration["Payment:Zibal:PaymentUrl"] ?? "https://gateway.zibal.ir/start/";

        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public PaymentGatewayType GatewayType => PaymentGatewayType.Zibal;

    public async Task<PaymentRequestResult> CreatePaymentRequestAsync(
        PaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("Zibal");

            // تبدیل مبلغ به ریال (Zibal با ریال کار می‌کند)
            var amountInRial = ConvertAmountToRial(request.Amount, request.Currency);

            // ساخت درخواست
            var zibalRequest = new
            {
                merchant = _merchantId,
                amount = amountInRial,
                callbackUrl = request.CallbackUrl,
                description = request.Description,
                mobile = request.MobileNumber,
                orderId = Guid.NewGuid().ToString("N"), // شناسه سفارش منحصر به فرد
                allowedCards = new string[] { } // محدودیت کارت‌های بانکی (اختیاری)
            };

            // ارسال درخواست به Zibal
            var response = await httpClient.PostAsJsonAsync(
                $"{_apiBaseUrl}request",
                zibalRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "خطا در ارتباط با درگاه Zibal. کد وضعیت: {StatusCode}",
                    response.StatusCode);

                return PaymentRequestResult.Failure(
                    "خطا در ارتباط با درگاه پرداخت",
                    PaymentErrorCode.ConnectionFailed);
            }

            // دریافت پاسخ
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zibalResponse = JsonSerializer.Deserialize<ZibalPaymentRequestResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zibalResponse?.Result != 100)
            {
                _logger.LogError(
                    "خطا در ایجاد درخواست پرداخت Zibal. کد وضعیت: {Result}, پیام: {Message}",
                    zibalResponse?.Result,
                    GetZibalErrorMessage(zibalResponse?.Result ?? -1));

                return PaymentRequestResult.Failure(
                    GetZibalErrorMessage(zibalResponse?.Result ?? -1),
                    MapZibalErrorCode(zibalResponse?.Result ?? -1));
            }

            // ساخت آدرس پرداخت
            var paymentUrl = $"{_paymentBaseUrl}{zibalResponse.TrackId}";

            _logger.LogInformation(
                "درخواست پرداخت Zibal با موفقیت ایجاد شد. شناسه: {TrackId}, مبلغ: {Amount} ریال",
                zibalResponse.TrackId,
                amountInRial);

            return PaymentRequestResult.Success(zibalResponse.TrackId.ToString(), paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ایجاد درخواست پرداخت Zibal");
            return PaymentRequestResult.Failure(
                "خطا در ایجاد درخواست پرداخت",
                PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(
        PaymentVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // بررسی وضعیت بازگشتی از درگاه
            if (request.Status != "1")
            {
                _logger.LogWarning(
                    "پرداخت Zibal توسط کاربر لغو شد یا ناموفق بود. شناسه: {Authority}, وضعیت: {Status}",
                    request.Authority,
                    request.Status);

                return PaymentVerificationResult.Failure(
                    "پرداخت توسط کاربر لغو شد یا ناموفق بود",
                    PaymentErrorCode.CanceledByUser);
            }

            var httpClient = _httpClientFactory.CreateClient("Zibal");

            // ساخت درخواست تأیید
            var verifyRequest = new
            {
                merchant = _merchantId,
                trackId = request.Authority
            };

            // ارسال درخواست تأیید به Zibal
            var response = await httpClient.PostAsJsonAsync(
                $"{_apiBaseUrl}verify",
                verifyRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "خطا در ارتباط با درگاه Zibal برای تأیید پرداخت. کد وضعیت: {StatusCode}",
                    response.StatusCode);

                return PaymentVerificationResult.Failure(
                    "خطا در ارتباط با درگاه پرداخت برای تأیید تراکنش",
                    PaymentErrorCode.ConnectionFailed);
            }

            // دریافت پاسخ
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zibalResponse = JsonSerializer.Deserialize<ZibalVerificationResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zibalResponse?.Result != 100)
            {
                _logger.LogError(
                    "خطا در تأیید پرداخت Zibal. کد وضعیت: {Result}, پیام: {Message}",
                    zibalResponse?.Result,
                    GetZibalErrorMessage(zibalResponse?.Result ?? -1));

                return PaymentVerificationResult.Failure(
                    GetZibalErrorMessage(zibalResponse?.Result ?? -1),
                    MapZibalErrorCode(zibalResponse?.Result ?? -1));
            }

            _logger.LogInformation(
                "تأیید پرداخت Zibal با موفقیت انجام شد. شناسه ارجاع: {RefNumber}, مبلغ: {Amount} ریال",
                zibalResponse.RefNumber,
                zibalResponse.Amount);

            // تبدیل ریال به واحد کیف پول
            decimal amount = ConvertRialToAmount(zibalResponse.Amount, WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR);

            return PaymentVerificationResult.Success(zibalResponse.RefNumber, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تأیید پرداخت Zibal");
            return PaymentVerificationResult.Failure(
                "خطا در تأیید پرداخت",
                PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentRefundResult> RefundPaymentAsync(
        PaymentRefundRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("Zibal");

            // ساخت درخواست استرداد
            var refundRequest = new
            {
                merchant = _merchantId,
                refNumber = request.ReferenceId,
                reason = request.Description,
                amount = ConvertAmountToRial(request.Amount, WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR)
            };

            // ارسال درخواست استرداد به Zibal
            var response = await httpClient.PostAsJsonAsync(
                $"{_apiBaseUrl}refund",
                refundRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "خطا در ارتباط با درگاه Zibal برای استرداد وجه. کد وضعیت: {StatusCode}",
                    response.StatusCode);

                return PaymentRefundResult.Failure(
                    "خطا در ارتباط با درگاه پرداخت برای استرداد وجه",
                    PaymentErrorCode.ConnectionFailed);
            }

            // دریافت پاسخ
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zibalResponse = JsonSerializer.Deserialize<ZibalRefundResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zibalResponse?.Result != 100)
            {
                _logger.LogError(
                    "خطا در استرداد وجه Zibal. کد وضعیت: {Result}, پیام: {Message}",
                    zibalResponse?.Result,
                    GetZibalErrorMessage(zibalResponse?.Result ?? -1));

                return PaymentRefundResult.Failure(
                    GetZibalErrorMessage(zibalResponse?.Result ?? -1),
                    MapZibalErrorCode(zibalResponse?.Result ?? -1));
            }

            _logger.LogInformation(
                "استرداد وجه Zibal با موفقیت انجام شد. شناسه استرداد: {RefundTrackId}",
                zibalResponse.RefundTrackId);

            return PaymentRefundResult.Success(zibalResponse.RefundTrackId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در استرداد وجه Zibal");
            return PaymentRefundResult.Failure(
                "خطا در استرداد وجه",
                PaymentErrorCode.Unknown);
        }
    }

    #region Helper Methods

    private decimal ConvertAmountToRial(decimal amount, WalletPayment.Domain.Entities.Enums.CurrencyCode currency)
    {
        // تبدیل مبلغ به ریال بر اساس نوع ارز
        return currency switch
        {
            WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR => amount, // ریال به ریال
            _ => throw new ArgumentException($"تبدیل ارز {currency} به ریال پشتیبانی نمی‌شود")
        };
    }

    private decimal ConvertRialToAmount(decimal rial, WalletPayment.Domain.Entities.Enums.CurrencyCode currency)
    {
        // تبدیل ریال به واحد کیف پول
        return currency switch
        {
            WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR => rial, // ریال به ریال
            _ => throw new ArgumentException($"تبدیل ریال به ارز {currency} پشتیبانی نمی‌شود")
        };
    }

    private string GetZibalErrorMessage(int result) => result switch
    {
        -1 => "مرچنت کد صحیح نیست",
        -2 => "مبلغ کمتر از حداقل مبلغ مجاز است",
        -3 => "مقدار مبلغ مجاز نیست",
        -4 => "درگاه فعال نیست",
        -11 => "درخواست مورد نظر یافت نشد",
        -12 => "تراکنش قبلاً تأیید شده است",
        -21 => "هیچ نوع عملیات مالی برای این تراکنش یافت نشد",
        -22 => "تراکنش ناموفق بود",
        -33 => "رقم تراکنش با رقم پرداخت شده مطابقت ندارد",
        -40 => "اجازه دسترسی به متد مربوطه وجود ندارد",
        -41 => "مشکل در اعتبارسنجی اطلاعات",
        -54 => "درخواست مورد نظر آرشیو شده است",
        101 => "تراکنش با موفقیت انجام شده است",
        _ => $"خطای ناشناخته با کد {result}"
    };

    private PaymentErrorCode MapZibalErrorCode(int result) => result switch
    {
        -1 => PaymentErrorCode.MerchantNotFound,
        -2 => PaymentErrorCode.InvalidAmount,
        -3 => PaymentErrorCode.InvalidAmount,
        -4 => PaymentErrorCode.GatewayError,
        -11 => PaymentErrorCode.AuthorityNotFound,
        -12 => PaymentErrorCode.DuplicateTransaction,
        -21 => PaymentErrorCode.AuthorityNotFound,
        -22 => PaymentErrorCode.GatewayError,
        -33 => PaymentErrorCode.InvalidAmount,
        -40 => PaymentErrorCode.GatewayError,
        -41 => PaymentErrorCode.GatewayError,
        -54 => PaymentErrorCode.ExpiredTransaction,
        101 => PaymentErrorCode.DuplicateTransaction,
        _ => PaymentErrorCode.GatewayError
    };

    #endregion
}

// کلاس‌های موردنیاز برای پاسخ‌های Zibal
internal class ZibalPaymentRequestResponse
{
    public int Result { get; set; }
    public long TrackId { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class ZibalVerificationResponse
{
    public int Result { get; set; }
    public string RefNumber { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardHashPan { get; set; } = string.Empty;
    public string PaidAt { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}

internal class ZibalRefundResponse
{
    public int Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public long RefundTrackId { get; set; }
}