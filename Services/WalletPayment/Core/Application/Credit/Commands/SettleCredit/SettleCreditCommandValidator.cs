using FluentValidation;

namespace WalletPayment.Application.Credit.Commands.SettleCredit;

public class SettleCreditCommandValidator : AbstractValidator<SettleCreditCommand>
{
    public SettleCreditCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد");

        RuleFor(x => x.PaymentReferenceId)
            .NotEmpty().WithMessage("شناسه مرجع پرداخت الزامی است");
    }
}