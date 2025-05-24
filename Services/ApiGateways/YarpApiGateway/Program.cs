using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using YarpApiGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "آفاق سیر - API Gateway",
        Version = "v1",
        Description = "API Gateway برای دسترسی به تمام سرویس‌های آفاق سیر",
        //Contact = new OpenApiContact
        //{
        //    Name = "تیم توسعه آفاق سیر",
        //    Email = ""
        //}
    });

    // اضافه کردن Authentication به Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add HttpClient for Swagger aggregation
builder.Services.AddHttpClient();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Rate Limiting
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 100;
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 10;
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Gateway Swagger
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway");

        // سرویس‌های اصلی (فقط Public APIs)
        c.SwaggerEndpoint("https://localhost:7072/swagger/v1/swagger.json", "User Management");
        c.SwaggerEndpoint("https://localhost:7240/swagger/v1/swagger.json", "Wallet Payment");


        c.RoutePrefix = "swagger";
        c.DocumentTitle = "آفاق سیر - API Documentation";
        c.DefaultModelsExpandDepth(-1); // مخفی کردن Models section
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
        
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}

// middlewares
app.UseCors("AllowAll");
app.UseRateLimiter();

// Custom middleware for blocking internal APIs
app.UseMiddleware<InternalPathFilterMiddleware>();

// Add a simple endpoint for health check
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "API Gateway",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}))
.WithName("HealthCheck")
.WithTags("System")
.Produces<object>(StatusCodes.Status200OK);

// Add endpoint to get service status
app.MapGet("/services/status", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();
    var services = new Dictionary<string, object>();

    // بررسی وضعیت سرویس‌ها
    var serviceChecks = new[]
    {
        ("UserManagement", "http://localhost:5001/health"),
        ("WalletPayment", "http://localhost:5002/health")
    };

    foreach (var (serviceName, healthUrl) in serviceChecks)
    {
        try
        {
            var response = await httpClient.GetAsync(healthUrl);
            services[serviceName] = new
            {
                Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                ResponseTime = response.Headers.Date?.ToString() ?? "Unknown"
            };
        }
        catch
        {
            services[serviceName] = new { Status = "Unreachable" };
        }
    }

    return Results.Ok(new
    {
        Gateway = "Healthy",
        Services = services,
        Timestamp = DateTime.UtcNow
    });
})
.WithName("ServicesStatus")
.WithTags("System")
.Produces<object>(StatusCodes.Status200OK);

app.UseStaticFiles();

// YARP reverse proxy
app.MapReverseProxy();

app.Run();