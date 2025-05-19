namespace UserManagement.API.Endpoints.Auth.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.IdentityId)
                .NotEmpty().WithMessage("شناسه هویتی نامعتبر است.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("رمز عبور فعلی الزامی است.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("رمز عبور جدید الزامی است.")
                .MinimumLength(6).WithMessage("رمز عبور جدید باید حداقل ۶ کاراکتر باشد.")
                .NotEqual(x => x.CurrentPassword).WithMessage("رمز عبور جدید نباید با رمز فعلی یکسان باشد.");
        }
    }
}
