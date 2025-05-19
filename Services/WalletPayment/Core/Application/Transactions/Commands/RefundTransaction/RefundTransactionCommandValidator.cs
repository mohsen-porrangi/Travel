using FluentValidation;

namespace WalletPayment.Application.Transactions.Commands.RefundTransaction;

public class RefundTransactionCommandValidator : AbstractValidator<RefundTransactionCommand>
{
    public RefundTransactionCommandValidator()
    {
        RuleFor(x => x.OriginalTransactionId)
            .NotEmpty().WithMessage("شناسه تراکنش اصلی نمی‌تواند خالی باشد");

        RuleFor(x => x.Amount)
            .GreaterThan(0).When(x => x.Amount.HasValue)
            .WithMessage("مبلغ استرداد باید بزرگتر از صفر باشد");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("دلیل استرداد الزامی است")
            .MaximumLength(500).WithMessage("دلیل استرداد نمی‌تواند بیش از 500 کاراکتر باشد");
    }
}