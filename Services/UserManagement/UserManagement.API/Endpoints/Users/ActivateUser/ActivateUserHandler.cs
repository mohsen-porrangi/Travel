using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Users.ActivateUser
{
    internal sealed class ActivateUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
      : ICommandHandler<ActivateUserCommand>
    {
        public async Task<Unit> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await repository.GetByIdAsync(command.Id, cancellationToken)
                       ?? throw new InvalidOperationException("کاربر یافت نشد");

            user.Activate();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
