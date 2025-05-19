using BuildingBlocks.Contracts;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Auth.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService
) : ICommandHandler<RegisterUserCommand>
{
    public async Task<Unit> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var mobileExists = await uow.Users.MobileExistsAsync(command.Mobile);
        if (mobileExists)
            throw new BadRequestException("شماره موبایل تکراری است",
                $"کاربری با شماره موبایل {command.Mobile} قبلاً ثبت شده است");


        var identity = new MasterIdentity
        {
            Id = Guid.NewGuid(),
            Mobile = command.Mobile,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password),
            CreatedAt = DateTime.UtcNow
        };


        var user = new User
        {
            IdentityId = identity.Id,
            Name = string.Empty,
            Family = string.Empty,
            NationalCode = string.Empty,
            Gender = null,
            BirthDate = default,
            IsActive = false  // کاربر تا زمان تأیید OTP غیرفعال است
        };

        await uow.Users.AddIdentityAsync(identity);
        await uow.Users.AddAsync(user);

        try
        {
            await otpService.SendOtpAsync(command.Mobile);
            await uow.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InternalServerException("خطا در ثبت کاربر", ex.Message);
        }

        return Unit.Value;
    }
}
