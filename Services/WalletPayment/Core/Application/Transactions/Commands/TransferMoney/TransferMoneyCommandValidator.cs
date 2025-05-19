using FluentValidation;

namespace WalletPayment.Application.Transactions.Commands.TransferMoney;

public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.SourceUserId)
            .NotEmpty().WithMessage("شناسه کاربر مبدأ نمی‌تواند خالی باشد");

        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("شناسه کاربر مقصد نمی‌تواند خالی باشد")
            .NotEqual(x => x.SourceUserId).WithMessage("کاربر مبدأ و مقصد نمی‌توانند یکسان باشند");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ انتقال باید بزرگتر از صفر باشد");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد");
    }
}