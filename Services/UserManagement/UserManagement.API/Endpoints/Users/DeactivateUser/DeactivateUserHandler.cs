using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Users.DeactivateUser
{
    internal sealed class DeactivateUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateUserCommand>
    {
        public async Task<Unit> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await repository.GetByIdAsync(command.Id, cancellationToken)
                       ?? throw new InvalidOperationException("کاربر یافت نشد");

            user.Deactivate();

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}