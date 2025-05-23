using BuildingBlocks.Contracts.Services;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Domain.Entities.Enums;


namespace Application.Wallet.Commands.CreateWallet;
public class CreateWalletCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork,
    IUserManagementService userService)
    : ICommandHandler<CreateWalletCommand, CreateWalletResponse>
    {
    public async Task<CreateWalletResponse> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        //// بررسی وجود کاربر
        //bool userExists = await userService.UserExistsAsync(request.UserId);
        //if (!userExists)
        //    throw new NotFoundException("کاربر مورد نظر یافت نشد", request.UserId);

        // بررسی عدم وجود کیف پول قبلی
        bool walletExists = await walletRepository.ExistsByUserIdAsync(request.UserId, cancellationToken);
        if (walletExists)
            throw new BadRequestException("کاربر مورد نظر قبلاً دارای کیف پول است", $"UserId: {request.UserId}");

        // ایجاد کیف پول جدید
        var wallet = new WalletPayment.Domain.Entities.Wallet.Wallet(request.UserId);

        // ایجاد حساب پیش‌فرض با ارز ریال        
      //  const string DefaultCurrency = "IRR";
        var defaultCurrencyAccount = wallet.CreateCurrencyAccount(CurrencyCode.IRR);

        // ذخیره در دیتابیس
        await walletRepository.AddAsync(wallet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // بازگرداندن اطلاعات
        return new CreateWalletResponse(wallet.Id, defaultCurrencyAccount.Id);
    }
}
