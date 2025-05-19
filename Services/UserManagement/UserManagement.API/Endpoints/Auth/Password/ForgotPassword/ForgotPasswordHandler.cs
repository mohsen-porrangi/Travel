using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Auth.Password.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService
) : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var identity = await uow.Users.GetIdentityByMobileAsync(command.Mobile)
                       ?? throw new InvalidOperationException("کاربری با این شماره وجود ندارد");

        await otpService.SendOtpAsync(command.Mobile);

        var resetToken = Guid.NewGuid();
        await uow.Users.StorePasswordResetTokenAsync(identity.Id, resetToken);
        await uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}