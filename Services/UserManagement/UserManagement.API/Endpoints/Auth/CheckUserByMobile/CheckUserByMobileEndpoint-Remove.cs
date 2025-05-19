//namespace UserManagement.API.Endpoints.Endpoints.Auth.CheckUserByMobile;

//public class CheckUserByMobileEndpoint : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapGet("/users/exists-by-mobile", async (string mobile, ISender sender) =>
//        {
//            var result = await sender.Send(new CheckUserByMobileQuery(mobile));
//            return Results.Ok(result);
//        });
//    }
//}
