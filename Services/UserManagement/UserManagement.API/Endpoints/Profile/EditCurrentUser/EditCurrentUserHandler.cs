using BuildingBlocks.Contracts;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser
{
    internal sealed class EditCurrentUserCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork)
      : ICommandHandler<EditCurrentUserCommand>
    {
        public async Task<Unit> Handle(EditCurrentUserCommand command, CancellationToken cancellationToken)
        {
            var user = await repository.FirstOrDefaultAsync(x => x.IdentityId == command.IdentityId, track: true)
                       ?? throw new InvalidOperationException("کاربر یافت نشد");

            user.UpdateProfile(
                command.Name,
                command.Family,
                command.NationalCode,
                command.Gender,
                command.BirthDate
            );

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
