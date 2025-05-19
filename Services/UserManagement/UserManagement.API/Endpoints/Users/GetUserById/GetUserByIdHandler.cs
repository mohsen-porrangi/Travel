using BuildingBlocks.Exceptions;

namespace UserManagement.API.Endpoints.Users.GetUserById
{
    internal sealed class GetUserByIdQueryHandler(IUserRepository repository)
       : IQueryHandler<GetUserByIdQuery, GetUserByIdResult>
    {
        public async Task<GetUserByIdResult> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await repository.GetByIdAsync(query.Id, cancellationToken, track: false);
            if (user is null)
                throw new NotFoundException("کاربر یافت نشد", $"کاربری با شناسه {query.Id} یافت نشد");

            var identity = user.MasterIdentity;
            if (identity is null)
                throw new InternalServerException("خطای داخلی سیستم", "اطلاعات هویتی کاربر یافت نشد");

            return new GetUserByIdResult(
              user.Id,
              user.Name,
              user.Family,
              identity.Email,
              user.IsActive
            );
        }
    }
}
