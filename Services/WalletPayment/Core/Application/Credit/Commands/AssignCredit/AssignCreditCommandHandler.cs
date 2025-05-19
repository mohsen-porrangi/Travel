using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using WalletPayment.Application.Common.Contracts;

namespace WalletPayment.Application.Credit.Commands.AssignCredit;

public class AssignCreditCommandHandler(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignCreditCommand, AssignCreditResponse>
{
    public async Task<AssignCreditResponse> Handle(AssignCreditCommand request, CancellationToken cancellationToken)
    {
        // دریافت کیف پول کاربر
        var wallet = await walletRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (wallet == null)
            throw new NotFoundException("کیف پول برای کاربر مورد نظر یافت نشد", request.UserId);

        if (!wallet.IsActive)
            throw new BadRequestException("کیف پول غیرفعال است و امکان تخصیص اعتبار وجود ندارد");

        // بررسی وجود اعتبار قبلی
        if (wallet.CreditBalance > 0 && wallet.CreditDueDate.HasValue)
            throw new BadRequestException("کاربر دارای اعتبار فعال است. ابتدا اعتبار فعلی را تسویه کنید");

        // تخصیص اعتبار
        wallet.AssignCredit(request.Amount, request.DueDate, request.Description);

        // ذخیره تغییرات
        walletRepository.Update(wallet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // بازگشت نتیجه
        return new AssignCreditResponse(
            wallet.Id,
            wallet.CreditLimit,
            wallet.CreditBalance,
            wallet.CreditDueDate.Value
        );
    }
}