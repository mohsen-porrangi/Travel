using BuildingBlocks.Contracts;
using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Auth.RegisterUser;

internal sealed class VerifyRegisterOtpCommandHandler(
    IUnitOfWork uow,
    IOtpService otpService,
    IWalletPaymentService walletService,
    ITemporaryRegistrationService tempRegistrationService,
    ILogger<VerifyRegisterOtpCommandHandler> logger
) : ICommandHandler<VerifyRegisterOtpCommand>
{
    public async Task<Unit> Handle(VerifyRegisterOtpCommand command, CancellationToken cancellationToken)
    {
        // دریافت اطلاعات ثبت‌نام موقت
        var tempData = await tempRegistrationService.GetTemporaryRegistrationAsync(command.Mobile);
        if (tempData == null)
        {
            logger.LogWarning("No temporary registration found for mobile: {Mobile}", command.Mobile);
            throw new BadRequestException("درخواست ثبت‌نام یافت نشد",
                "لطفاً ابتدا فرآیند ثبت‌نام را شروع کنید یا مجدداً تلاش کنید");
        }
        // بررسی انقضای زمان (اضافی برای اطمینان)
        if (DateTime.UtcNow - tempData.CreatedAt > TimeSpan.FromMinutes(2))
        {
            logger.LogWarning("Expired registration attempt for mobile: {Mobile}", command.Mobile);
            await tempRegistrationService.RemoveTemporaryRegistrationAsync(command.Mobile);
            throw new BadRequestException("زمان ثبت‌نام به پایان رسیده است",
                "لطفاً فرآیند ثبت‌نام را مجدداً شروع کنید");
        }


        // تأیید OTP
        var isOtpValid = await otpService.ValidateOtpAsync(command.Mobile, command.Otp);
        if (!isOtpValid)
        {
            logger.LogWarning("Invalid OTP provided for mobile: {Mobile}", command.Mobile);
            throw new UnauthorizedDomainException("کد تأیید نامعتبر است",
                "لطفاً کد تأیید ارسال شده را بررسی کنید");
        }
        // شروع تراکنش برای ایجاد کاربر و کیف پول
        using var transaction = await uow.BeginTransactionAsync(cancellationToken);

        try
        {
            // بررسی مجدد عدم تکرار در دیتابیس (برای اطمینان)
            var mobileStillExists = await uow.Users.MobileExistsAsync(command.Mobile, cancellationToken);
            if (mobileStillExists)
            {
                logger.LogError("Mobile became duplicate during registration process: {Mobile}", command.Mobile);
                throw new BadRequestException("شماره موبایل در حین فرآیند ثبت‌نام تکراری شده است",
                    "لطفاً مجدداً تلاش کنید");
            }

            // ایجاد Identity
            var identity = new MasterIdentity
            {
                Id = Guid.NewGuid(),
                Mobile = command.Mobile,
                PasswordHash = tempData.PasswordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true  // ✅ فعال چون OTP تأیید شده
            };

            // ایجاد User
            var user = new User
            {
                Id = Guid.NewGuid(),
                IdentityId = identity.Id,
                Name = string.Empty,
                Family = string.Empty,
                NationalCode = string.Empty,
                Gender = null,
                BirthDate = default,
                CreatedAt = DateTime.UtcNow
            };

            // ذخیره در دیتابیس
            await uow.Users.AddIdentityAsync(identity, cancellationToken);
            await uow.Users.AddAsync(user);
            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("User created successfully: {UserId} for mobile: {Mobile}", user.Id, command.Mobile);

            // ایجاد کیف پول
            var walletCreated = await walletService.CreateWalletAsync(user.Id, "IRR");
            if (!walletCreated)
            {
                logger.LogError("Failed to create wallet for user: {UserId}", user.Id);
                throw new InternalServerException("خطا در ایجاد کیف پول",
                    "کاربر ایجاد شد اما کیف پول ساخته نشد");
            }

            logger.LogInformation("Wallet created successfully for user: {UserId}", user.Id);

            // commit تراکنش
            await uow.CommitTransactionAsync(cancellationToken);

            // پاک‌سازی اطلاعات موقت
            await tempRegistrationService.RemoveTemporaryRegistrationAsync(command.Mobile);

            logger.LogInformation("Registration completed successfully for mobile: {Mobile}, userId: {UserId}",
                command.Mobile, user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user creation process for mobile: {Mobile}", command.Mobile);

            // rollback تراکنش
            await uow.RollbackTransactionAsync(cancellationToken);

            // در صورت خطا، اطلاعات موقت را حفظ می‌کنیم تا کاربر بتواند مجدداً تلاش کند
            if (ex is not BadRequestException)
            {
                throw new InternalServerException("خطا در تکمیل فرآیند ثبت‌نام",
                    "لطفاً مجدداً کد تأیید را وارد کنید یا با پشتیبانی تماس بگیرید");
            }

            throw;
        }

        return Unit.Value;
    }
}
