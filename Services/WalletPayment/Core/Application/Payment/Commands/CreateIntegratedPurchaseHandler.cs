using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.Payment.Contracts;

namespace WalletPayment.Application.Payment.Commands.CreateIntegratedPurchase;

public class CreateIntegratedPurchaseCommandHandler :
    ICommandHandler<CreateIntegratedPurchaseCommand, IntegratedPurchaseResponse>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IIntegratedPurchaseService _integratedPurchaseService;
    private readonly ILogger<CreateIntegratedPurchaseCommandHandler> _logger;

    public CreateIntegratedPurchaseCommandHandler(
        IWalletRepository walletRepository,
        IIntegratedPurchaseService integratedPurchaseService,
        ILogger<CreateIntegratedPurchaseCommandHandler> logger)
    {
        _walletRepository = walletRepository;
        _integratedPurchaseService = integratedPurchaseService;
        _logger = logger;
    }

    public async Task<IntegratedPurchaseResponse> Handle(
        CreateIntegratedPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // بررسی موجودی فعلی کیف پول
            var wallet = await _walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (wallet == null)
            {
                return new IntegratedPurchaseResponse
                {
                    IsSuccessful = false,
                    ErrorMessage = "کیف پول کاربر یافت نشد"
                };
            }

            // یافتن حساب با ارز مورد نظر
            var account = wallet.CurrencyAccount.FirstOrDefault(a => a.Currency == request.Currency && a.IsActive);
            decimal currentBalance = account?.Balance ?? 0;

            // محاسبه مبلغی که باید از درگاه پرداخت شود
            decimal amountToPay = request.Amount;
            bool useWalletBalance = false;

            if (currentBalance > 0)
            {
                if (currentBalance >= request.Amount)
                {
                    // موجودی کیف پول برای پرداخت کامل کافی است
                    // در اینجا می‌توان مستقیماً از خود کیف پول برداشت کرد
                    // و نیازی به درگاه پرداخت نیست

                    return new IntegratedPurchaseResponse
                    {
                        IsSuccessful = true,
                        UseWalletBalance = true,
                        CurrentBalance = currentBalance,
                        AmountFromWallet = request.Amount,
                        AmountToPay = 0,
                        TotalAmount = request.Amount,
                        // در این حالت PaymentUrl و Authority خالی می‌ماند چون نیازی به پرداخت از درگاه نیست
                    };
                }
                else
                {
                    // استفاده از موجودی کیف پول و پرداخت مابقی از طریق درگاه
                    amountToPay = request.Amount - currentBalance;
                    useWalletBalance = true;
                }
            }

            // اضافه کردن اطلاعات موجودی و مقدار پرداختی به متادیتا
            var metadata = request.Metadata ?? new Dictionary<string, string>();
            metadata["UseWalletBalance"] = useWalletBalance.ToString();
            metadata["CurrentBalance"] = currentBalance.ToString();
            metadata["AmountFromWallet"] = Math.Min(currentBalance, request.Amount).ToString();
            metadata["AmountFromGateway"] = amountToPay.ToString();
            metadata["TotalAmount"] = request.Amount.ToString();

            // ایجاد درخواست پرداخت یکپارچه - فقط برای مقدار مابه‌التفاوت
            var result = await _integratedPurchaseService.CreateIntegratedPurchaseRequestAsync(
                request.UserId,
                amountToPay, // فقط مبلغ مابه‌التفاوت را پرداخت می‌کنیم
                request.Currency,
                request.Description,
                request.GatewayType,
                request.CallbackUrl,
                request.OrderId,
                cancellationToken);

            return new IntegratedPurchaseResponse
            {
                IsSuccessful = result.IsSuccessful,
                PaymentUrl = result.PaymentUrl,
                Authority = result.Authority,
                CurrentBalance = currentBalance,
                AmountFromWallet = Math.Min(currentBalance, request.Amount),
                AmountToPay = amountToPay,
                TotalAmount = request.Amount,
                UseWalletBalance = useWalletBalance,
                ErrorMessage = result.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در پردازش خرید یکپارچه");
            return new IntegratedPurchaseResponse
            {
                IsSuccessful = false,
                ErrorMessage = $"خطای سیستمی: {ex.Message}"
            };
        }
    }
}