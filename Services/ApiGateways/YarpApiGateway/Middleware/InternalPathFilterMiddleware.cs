namespace YarpApiGateway.Middleware;

public class InternalPathFilterMiddleware(
    RequestDelegate next,
    ILogger<InternalPathFilterMiddleware> logger) // حتماً ILogger باشه
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        if (path?.Contains("/api/internal/") == true)
        {
            logger.LogWarning("Blocked internal API access attempt: {Path}", path);

            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync("""
                {
                    "error": "Forbidden",
                    "message": "Access to internal APIs is not allowed via gateway",
                    "statusCode": 403
                }
                """);
            return;
        }

        await next(context);
    }
}