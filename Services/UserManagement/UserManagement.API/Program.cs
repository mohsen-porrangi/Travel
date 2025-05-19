using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using UserManagement.API.Infrastructure.Configuration;
using UserManagement.API.Infrastructure.Data;
using UserManagement.API.Infrastructure.Middleware;

namespace UserManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var assembly = typeof(Program).Assembly;

            builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("Database");          
          
            builder.Services.ConfigureSqlServer(builder.Configuration);
            builder.Services.ConfigureMediatR(builder.Configuration);           
            builder.Services.ConfigureService();           

            builder.Services.AddValidatorsFromAssembly(assembly);

            builder.Services.AddCarter();

            builder.Services.AddEndpointsApiExplorer(); 
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<ITokenService, JwtTokenService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddExceptionHandler<ErrorHandlerMiddleware>();
            builder.Services.AddProblemDetails();        

            //builder.Services.AddAuthentication("Bearer")
            //   .AddJwtBearer("Bearer", options =>
            //   {
            //       options.TokenValidationParameters = new TokenValidationParameters
            //       {
            //           
            //       };
            //   });

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireAssertion(_ => true)); //TODO its temp until impliment real token
            });


            var app = builder.Build();
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            //  if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseAuthentication();    
            app.UseAuthorization();

            app.MapCarter();

            app.UseHealthChecks("/health",
               new HealthCheckOptions
               {
                   ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
               });

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                DbInitializer.Seed(db);
            }
            app.Run();
         

        }
    }
}
