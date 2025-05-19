namespace UserManagement.API.Endpoints.Users.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : ICommand;
}
