namespace UserManagement.API.Endpoints.Users.GetUsers
{
    internal sealed class GetAllUsersQueryHandler(IUserRepository repository)
     : IQueryHandler<GetAllUsersQuery, GetAllUsersResult>
    {
        public async Task<GetAllUsersResult> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
        {
            var users = await repository.GetAllAsync(cancellationToken);

            var result = users.Select(user => new UserDto(
                user.Id,
                user.Name,
                user.Family,
                user.MasterIdentity.Email,
                user.IsActive
            ));

            return new GetAllUsersResult(result);
        }
    }
}