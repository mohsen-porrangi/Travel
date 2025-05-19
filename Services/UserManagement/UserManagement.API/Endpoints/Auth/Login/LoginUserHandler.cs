using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;
using UserManagement.API.Infrastructure.Data.Models;

namespace UserManagement.API.Endpoints.Auth.Login;

internal sealed class LoginUserCommandHandler(
    IUnitOfWork uow,
    ITokenService tokenService,
    IOtpService otpService
) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<string> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        MasterIdentity? identity = null;

        //if (!string.IsNullOrWhiteSpace(request.Email) && !string.IsNullOrWhiteSpace(request.Password))
        //{
        //    identity = await uow.Users.GetIdentityByEmailAsync(request.Email);
        //    if (identity is null || !BCrypt.Net.BCrypt.Verify(request.Password, identity.PasswordHash))
        //        throw new UnauthorizedAccessException("اطلاعات ورود نامعتبر است");
        //}
        if (!string.IsNullOrWhiteSpace(request.Mobile) && !string.IsNullOrWhiteSpace(request.Password))
        {
            identity = await uow.Users.GetIdentityByMobileAsync(request.Mobile);
            if (identity is null || !BCrypt.Net.BCrypt.Verify(request.Password, identity.PasswordHash))
                throw new UnauthorizedDomainException("اطلاعات ورود نامعتبر است");
        }
        else if (!string.IsNullOrWhiteSpace(request.Mobile) && !string.IsNullOrWhiteSpace(request.Otp))
        {
            var isValid = await otpService.ValidateOtpAsync(request.Mobile, request.Otp);
            if (!isValid)
                throw new UnauthorizedDomainException("کد اعتبارسنجی نامعتبر است");

            identity = await uow.Users.GetIdentityByMobileAsync(request.Mobile)
                ?? throw new NotFoundException("کاربر یافت نشد", $"کاربری با شماره موبایل {request.Mobile} یافت نشد");
        }
        else
        {
            throw new BadRequestException("پارامترهای ورود ناقص هستند", "باید شماره موبایل همراه با رمز عبور یا کد OTP ارائه شود");
        }

        var user = await uow.Users.GetUserByIdentityIdAsync(identity.Id)
        ?? throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه هویت {identity.Id} یافت نشد");

        if (!user.IsActive)
            throw new ForbiddenDomainException("حساب کاربری غیرفعال است");

        var permissions = await uow.Users.GetUserPermissionsAsync(user.Id);

        return tokenService.GenerateToken(user, permissions);
    }
}
