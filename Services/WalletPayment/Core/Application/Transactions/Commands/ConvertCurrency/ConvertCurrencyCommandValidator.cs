using FluentValidation;

namespace Application.Transactions.Commands.ConvertCurrency;

public class ConvertCurrencyCommandValidator : AbstractValidator<ConvertCurrencyCommand>
{
    public ConvertCurrencyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد");

        RuleFor(x => x.SourceAmount)
            .GreaterThan(0).WithMessage("مبلغ تبدیل باید بزرگتر از صفر باشد");

        RuleFor(x => x.SourceCurrency)
            .NotNull().WithMessage("ارز مبدأ الزامی است")
            .NotEqual(x => x.TargetCurrency).WithMessage("ارز مبدأ و مقصد نمی‌توانند یکسان باشند");

        RuleFor(x => x.TargetCurrency)
            .NotNull().WithMessage("ارز مقصد الزامی است");
    }
}