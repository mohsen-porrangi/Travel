using FluentValidation;
using WalletPayment.Domain.Entities.Enums;

namespace WalletPayment.Application.Transactions.Commands.ProcessTransaction;

public class ProcessWalletTransactionCommandValidator : AbstractValidator<ProcessWalletTransactionCommand>
{
    public ProcessWalletTransactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ تراکنش باید بزرگتر از صفر باشد");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("جهت تراکنش باید In یا Out باشد");

        RuleFor(x => x.ReferenceId)
            .NotEmpty().When(x => x.Direction == TransactionDirection.In)
            .WithMessage("شناسه مرجع برای واریز الزامی است");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیش از 500 کاراکتر باشد");
    }
}