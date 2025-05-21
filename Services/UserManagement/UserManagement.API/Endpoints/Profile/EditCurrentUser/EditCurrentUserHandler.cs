// فایل: Services/UserManagement/UserManagement.API/Endpoints/Profile/EditCurrentUser/EditCurrentUserHandler.cs
using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Profile.EditCurrentUser
{
    internal sealed class EditCurrentUserCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork
    ) : ICommandHandler<EditCurrentUserCommand>
    {
        public async Task<Unit> Handle(EditCurrentUserCommand command, CancellationToken cancellationToken)
        {
            var user = await repository.FirstOrDefaultAsync(x => x.IdentityId == command.IdentityId, track: true)
                ?? throw new InvalidOperationException("کاربر یافت نشد");

            // بروزرسانی اطلاعات پروفایل
            user.UpdateProfile(
                command.Name,
                command.Family,
                command.NationalCode,
                command.Gender,
                command.BirthDate
            );

            // اگر فیلدهای تغییر رمز ارسال شده باشند، رمز عبور را تغییر بده
            if (!string.IsNullOrEmpty(command.CurrentPassword) && !string.IsNullOrEmpty(command.NewPassword))
            {
                var identity = await repository.GetIdentityByIdAsync(command.IdentityId)
                    ?? throw new InvalidOperationException("اطلاعات هویتی کاربر یافت نشد");

                var isValid = BCrypt.Net.BCrypt.Verify(command.CurrentPassword, identity.PasswordHash);
                if (!isValid)
                    throw new UnauthorizedDomainException("رمز فعلی نادرست است");

                identity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
                await repository.UpdateIdentityAsync(identity);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}