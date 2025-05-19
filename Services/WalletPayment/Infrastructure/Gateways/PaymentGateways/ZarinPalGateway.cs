using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Infrastructure.ExternalServices.PaymentGateway;

public class ZarinPalGateway : IPaymentGateway
{
    private readonly string _merchantId;
    private readonly string _apiBaseUrl;
    private readonly string _paymentBaseUrl;
    private readonly bool _isTestMode;
    private readonly ILogger<ZarinPalGateway> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ZarinPalGateway(
        IConfiguration configuration,
        ILogger<ZarinPalGateway> logger,
        IHttpClientFactory httpClientFactory)
    {
        _merchantId = configuration["Payment:ZarinPal:MerchantId"] ??
                      throw new ArgumentNullException("Payment:ZarinPal:MerchantId configuration is missing");
        _isTestMode = bool.Parse(configuration["Payment:ZarinPal:IsTestMode"] ?? "false");

        // انتخاب آدرس‌ها بر اساس محیط تست یا واقعی
        if (_isTestMode)
        {
            _apiBaseUrl = "https://sandbox.zarinpal.com/pg/rest/WebGate/";
            _paymentBaseUrl = "https://sandbox.zarinpal.com/pg/StartPay/";
        }
        else
        {
            _apiBaseUrl = "https://www.zarinpal.com/pg/rest/WebGate/";
            _paymentBaseUrl = "https://www.zarinpal.com/pg/StartPay/";
        }

        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public PaymentGatewayType GatewayType => PaymentGatewayType.ZarinPal;

    public async Task<PaymentRequestResult> CreatePaymentRequestAsync(
        PaymentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ZarinPal");

            // تبدیل مبلغ به تومان (ZarinPal با تومان کار می‌کند)
            var amountInToman = ConvertAmountToToman(request.Amount, request.Currency);

            // ساخت درخواست
            var zarinRequest = new
            {
                MerchantID = _merchantId,
                Amount = amountInToman,
                Description = request.Description,
                CallbackURL = request.CallbackUrl,
                Mobile = request.MobileNumber,
                Email = request.Email
            };

            // ارسال درخواست به ZarinPal
            var response = await httpClient.PostAsJsonAsync(
                $"{_apiBaseUrl}PaymentRequest.json",
                zarinRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "خطا در ارتباط با درگاه ZarinPal. کد وضعیت: {StatusCode}",
                    response.StatusCode);

                return PaymentRequestResult.Failure(
                    "خطا در ارتباط با درگاه پرداخت",
                    PaymentErrorCode.ConnectionFailed);
            }

            // دریافت پاسخ
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zarinResponse = JsonSerializer.Deserialize<ZarinPalPaymentRequestResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zarinResponse?.Status != 100)
            {
                _logger.LogError(
                    "خطا در ایجاد درخواست پرداخت ZarinPal. کد وضعیت: {Status}, پیام: {Message}",
                    zarinResponse?.Status,
                    GetZarinPalErrorMessage(zarinResponse?.Status ?? -1));

                return PaymentRequestResult.Failure(
                    GetZarinPalErrorMessage(zarinResponse?.Status ?? -1),
                    MapZarinPalErrorCode(zarinResponse?.Status ?? -1));
            }

            // ساخت آدرس پرداخت
            var paymentUrl = $"{_paymentBaseUrl}{zarinResponse.Authority}";

            _logger.LogInformation(
                "درخواست پرداخت ZarinPal با موفقیت ایجاد شد. شناسه: {Authority}, مبلغ: {Amount} تومان",
                zarinResponse.Authority,
                amountInToman);

            return PaymentRequestResult.Success(zarinResponse.Authority, paymentUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ایجاد درخواست پرداخت ZarinPal");
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
            if (request.Status != "OK" && request.Status != "success")
            {
                _logger.LogWarning(
                    "پرداخت ZarinPal توسط کاربر لغو شد یا ناموفق بود. شناسه: {Authority}, وضعیت: {Status}",
                    request.Authority,
                    request.Status);

                return PaymentVerificationResult.Failure(
                    "پرداخت توسط کاربر لغو شد یا ناموفق بود",
                    PaymentErrorCode.CanceledByUser);
            }

            var httpClient = _httpClientFactory.CreateClient("ZarinPal");

            // تبدیل مبلغ به تومان
            var amountInToman = ConvertAmountToToman(request.Amount, WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR);

            // ساخت درخواست تأیید
            var verifyRequest = new
            {
                MerchantID = _merchantId,
                Authority = request.Authority,
                Amount = amountInToman
            };

            // ارسال درخواست تأیید به ZarinPal
            var response = await httpClient.PostAsJsonAsync(
                $"{_apiBaseUrl}PaymentVerification.json",
                verifyRequest,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "خطا در ارتباط با درگاه ZarinPal برای تأیید پرداخت. کد وضعیت: {StatusCode}",
                    response.StatusCode);

                return PaymentVerificationResult.Failure(
                    "خطا در ارتباط با درگاه پرداخت برای تأیید تراکنش",
                    PaymentErrorCode.ConnectionFailed);
            }

            // دریافت پاسخ
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var zarinResponse = JsonSerializer.Deserialize<ZarinPalVerificationResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (zarinResponse?.Status != 100)
            {
                _logger.LogError(
                    "خطا در تأیید پرداخت ZarinPal. کد وضعیت: {Status}, پیام: {Message}",
                    zarinResponse?.Status,
                    GetZarinPalErrorMessage(zarinResponse?.Status ?? -1));

                return PaymentVerificationResult.Failure(
                    GetZarinPalErrorMessage(zarinResponse?.Status ?? -1),
                    MapZarinPalErrorCode(zarinResponse?.Status ?? -1));
            }

            _logger.LogInformation(
                "تأیید پرداخت ZarinPal با موفقیت انجام شد. شناسه ارجاع: {RefID}, مبلغ: {Amount} تومان",
                zarinResponse.RefID,
                amountInToman);

            return PaymentVerificationResult.Success(zarinResponse.RefID.ToString(), request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تأیید پرداخت ZarinPal");
            return PaymentVerificationResult.Failure(
                "خطا در تأیید پرداخت",
                PaymentErrorCode.Unknown);
        }
    }

    public async Task<PaymentRefundResult> RefundPaymentAsync(
        PaymentRefundRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // ZarinPal API رسمی برای استرداد ندارد و نیاز به ارتباط با پشتیبانی دارد
        _logger.LogWarning(
            "درخواست استرداد وجه برای پرداخت ZarinPal. شناسه ارجاع: {ReferenceId}, مبلغ: {Amount}",
            request.ReferenceId,
            request.Amount);

        return PaymentRefundResult.Failure(
            "استرداد خودکار برای ZarinPal پشتیبانی نمی‌شود. لطفاً با پشتیبانی تماس بگیرید",
            PaymentErrorCode.RefundNotAllowed);
    }

    #region Helper Methods

    private decimal ConvertAmountToToman(decimal amount, WalletPayment.Domain.Entities.Enums.CurrencyCode currency)
    {
        // تبدیل مبلغ به تومان بر اساس نوع ارز
        return currency switch
        {
            WalletPayment.Domain.Entities.Enums.CurrencyCode.IRR => Math.Round(amount / 10), // ریال به تومان
            _ => throw new ArgumentException($"تبدیل ارز {currency} به تومان پشتیبانی نمی‌شود")
        };
    }

    private string GetZarinPalErrorMessage(int status) => status switch
    {
        -1 => "اطلاعات ارسال شده ناقص است",
        -2 => "IP یا مرچنت کد صحیح نیست",
        -3 => "با توجه به محدودیت‌های شاپرک، امکان پرداخت با رقم درخواست شده میسر نیست",
        -4 => "سطح تأیید پذیرنده پایین‌تر از سطح نقره‌ای است",
        -11 => "درخواست مورد نظر یافت نشد",
        -12 => "امکان ویرایش درخواست میسر نیست",
        -21 => "هیچ نوع عملیات مالی برای این تراکنش یافت نشد",
        -22 => "تراکنش ناموفق بود",
        -33 => "رقم تراکنش با رقم پرداخت شده مطابقت ندارد",
        -34 => "سقف تقسیم تراکنش از لحاظ تعداد یا رقم عبور شده است",
        -40 => "اجازه دسترسی به متد مربوطه وجود ندارد",
        -41 => "اطلاعات ارسال شده مربوط به AdditionalData غیرمعتبر است",
        -42 => "مدت زمان معتبر طول عمر شناسه پرداخت باید بین 30 دقیقه تا 45 روز باشد",
        -54 => "درخواست مورد نظر آرشیو شده است",
        101 => "عملیات پرداخت موفق بوده و قبلاً انجام شده است",
        _ => $"خطای ناشناخته با کد {status}"
    };

    private PaymentErrorCode MapZarinPalErrorCode(int status) => status switch
    {
        -1 => PaymentErrorCode.InvalidAmount,
        -2 => PaymentErrorCode.MerchantNotFound,
        -3 => PaymentErrorCode.InvalidAmount,
        -11 => PaymentErrorCode.AuthorityNotFound,
        -21 => PaymentErrorCode.AuthorityNotFound,
        -22 => PaymentErrorCode.GatewayError,
        -33 => PaymentErrorCode.InvalidAmount,
        -34 => PaymentErrorCode.DuplicateTransaction,
        101 => PaymentErrorCode.DuplicateTransaction,
        _ => PaymentErrorCode.GatewayError
    };

    #endregion
}

// کلاس‌های موردنیاز برای پاسخ‌های ZarinPal
internal class ZarinPalPaymentRequestResponse
{
    public int Status { get; set; }
    public string Authority { get; set; } = string.Empty;
}

internal class ZarinPalVerificationResponse
{
    public int Status { get; set; }
    public long RefID { get; set; }
}