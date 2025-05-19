namespace UserManagement.API.Endpoints.Endpoints.Auth.CheckUserByMobile
{

    public record CheckUserByMobileQuery(string Mobile) : IQuery<CheckUserByMobileResponse>;

    public record CheckUserByMobileResponse(bool Exists);
}
