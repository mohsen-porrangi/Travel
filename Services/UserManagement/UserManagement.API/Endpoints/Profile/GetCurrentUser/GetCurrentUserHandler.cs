namespace UserManagement.API.Endpoints.Profile.GetCurrentUser
{
    internal sealed class GetCurrentUserQueryHandler(IUserRepository repository)
     : IQueryHandler<GetCurrentUserQuery, GetCurrentUserResult>
    {
        public async Task<GetCurrentUserResult> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
        {
            var user = await repository.FirstOrDefaultAsync(x => x.IdentityId == query.IdentityId, track: false)
                       ?? throw new InvalidOperationException("کاربر یافت نشد");

            var identity = user.MasterIdentity;

            return new GetCurrentUserResult(
                user.Id,
                user.Name,
                user.Family,
                identity.Email,
                identity.Mobile,
                identity.IsActive
            );
        }
    }
}
