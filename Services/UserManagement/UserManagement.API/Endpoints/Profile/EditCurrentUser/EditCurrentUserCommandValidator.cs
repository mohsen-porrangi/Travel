using BuildingBlocks.Utils;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser;

public class EditCurrentUserCommandValidator : AbstractValidator<EditCurrentUserCommand>
{
    public EditCurrentUserCommandValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("شناسه هویتی نامعتبر است.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام الزامی است.")
            .MaximumLength(50).WithMessage("نام نباید بیش از ۵۰ کاراکتر باشد.");

        RuleFor(x => x.Family)
            .NotEmpty().WithMessage("نام خانوادگی الزامی است.")
            .MaximumLength(50).WithMessage("نام خانوادگی نباید بیش از ۵۰ کاراکتر باشد.");

        RuleFor(x => x.NationalCode)
            .Matches(@"^\d{10}$").WithMessage("کد ملی باید ۱۰ رقم باشد.")
            .Must(ValidationHelpers.IsValidIranianNationalCode)
            .WithMessage("کد ملی نامعتبر است.")
            .When(x => !string.IsNullOrWhiteSpace(x.NationalCode));

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("مقدار جنسیت نامعتبر است.");

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("تاریخ تولد نمی‌تواند در آینده باشد.");
    }
}
