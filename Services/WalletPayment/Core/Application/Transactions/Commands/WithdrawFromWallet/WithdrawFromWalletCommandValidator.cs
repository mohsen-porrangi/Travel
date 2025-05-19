using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Transactions.Commands.WithdrawFromWallet;

public class WithdrawFromWalletCommandValidator : AbstractValidator<WithdrawFromWalletCommand>
{
    public WithdrawFromWalletCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ برداشت باید بزرگتر از صفر باشد");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد");
    }
}