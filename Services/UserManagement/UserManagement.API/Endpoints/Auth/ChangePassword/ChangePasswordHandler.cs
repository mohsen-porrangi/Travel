using BuildingBlocks.Contracts;
using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Auth.ChangePassword;

internal sealed class ChangePasswordCommandHandler(IUnitOfWork uow)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var identity = await uow.Users.GetIdentityByIdAsync(command.IdentityId);
        if (identity is null)
            throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه هویت {command.IdentityId} یافت نشد");

        var isValid = BCrypt.Net.BCrypt.Verify(command.CurrentPassword, identity.PasswordHash);
        if (!isValid)
            throw new UnauthorizedDomainException("رمز فعلی نادرست است");

        identity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);

        await uow.Users.UpdateIdentityAsync(identity);
        await uow.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
