// فایل: Services/UserManagement/UserManagement.API/Endpoints/Users/UserStatus/ChangeUserStatusHandler.cs

// فایل: Services/UserManagement/UserManagement.API/Endpoints/Users/UserStatus/ChangeUserStatusHandler.cs
namespace UserManagement.API.Endpoints.Users.UserStatus;

internal sealed class ChangeUserStatusCommandHandler(
    IUserRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<ChangeUserStatusCommand>
{
    public async Task<Unit> Handle(ChangeUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new InvalidOperationException("کاربر یافت نشد");

        if (command.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}