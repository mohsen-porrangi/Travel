using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Auth.RegisterUser;

internal sealed class VerifyRegisterOtpCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService
) : ICommandHandler<VerifyRegisterOtpCommand>
{
    public async Task<Unit> Handle(VerifyRegisterOtpCommand command, CancellationToken cancellationToken)
    {
        var isValid = await otpService.ValidateOtpAsync(command.Mobile, command.Otp);
        if (!isValid)
            throw new UnauthorizedAccessException("کد اعتبارسنجی اشتباه است");

        var identity = await uow.Users.GetIdentityByMobileAsync(command.Mobile)
                       ?? throw new InvalidOperationException("کاربر یافت نشد");

        var user = await uow.Users.GetUserByIdentityIdAsync(identity.Id)
                   ?? throw new InvalidOperationException("کاربر یافت نشد");

        user.MasterIdentity.IsActive = true;

        await uow.Users.UpdateAsync(user);
        await uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
