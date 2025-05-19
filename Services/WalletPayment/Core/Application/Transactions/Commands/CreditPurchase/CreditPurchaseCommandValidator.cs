using FluentValidation;

namespace WalletPayment.Application.Transactions.Commands.CreditPurchase;

public class CreditPurchaseCommandValidator : AbstractValidator<CreditPurchaseCommand>
{
    public CreditPurchaseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ خرید باید بزرگتر از صفر باشد");

        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("شناسه سفارش الزامی است");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد");
    }
}