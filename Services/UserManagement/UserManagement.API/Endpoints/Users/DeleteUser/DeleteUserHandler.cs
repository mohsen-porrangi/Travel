using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Users.DeleteUser
{
    internal sealed class DeleteUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteUserCommand>
    {
        public async Task<Unit> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
        {
            await repository.DeleteAsync(command.Id, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
