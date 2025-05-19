using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Auth.Password.ResetPassword;

internal sealed class ResetPasswordCommandHandler(IUnitOfWork uow)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var identity = await uow.Users.GetIdentityByResetTokenAsync(command.ResetToken)
                       ?? throw new InvalidOperationException("توکن نامعتبر است");

        identity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);

        await uow.Users.RemoveResetTokenAsync(identity.Id);
        await uow.Users.UpdateIdentityAsync(identity);
        await uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}