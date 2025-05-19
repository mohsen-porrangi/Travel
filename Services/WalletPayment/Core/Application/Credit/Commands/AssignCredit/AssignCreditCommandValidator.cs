using FluentValidation;

namespace WalletPayment.Application.Credit.Commands.AssignCredit;

public class AssignCreditCommandValidator : AbstractValidator<AssignCreditCommand>
{
    public AssignCreditCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ اعتبار باید بزرگتر از صفر باشد");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("تاریخ سررسید باید در آینده باشد");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد");
    }
}