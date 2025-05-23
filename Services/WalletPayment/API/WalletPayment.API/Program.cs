using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WalletPayment.API.Middleware;
using WalletPayment.Infrastructure;
using WalletPayment.Application;
using WalletPayment.API.Services;
using BuildingBlocks.Contracts;

namespace WalletPayment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembly = typeof(Program).Assembly;
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            // Health Checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<WalletPayment.Infrastructure.Data.Context.WalletDbContext>("Database");

            // Infrastructure
            builder.Services.AddWalletPaymentInfrastructure(builder.Configuration);

            // Application
            builder.Services.AddWalletPaymentApplication(builder.Configuration);

            // API
            builder.Services.AddValidatorsFromAssembly(assembly);
            builder.Services.AddCarter();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddExceptionHandler<ErrorHandlerMiddleware>();
            builder.Services.AddProblemDetails();

            builder.Services.AddHttpContextAccessor();

            // Authentication & Authorization
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            //Inject Service TODO move to dependency incejection
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseCurrentUser();
            app.UseAuthorization();

            app.MapCarter();

            app.UseHealthChecks("/health",
                new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });


            app.Run();
        }
    }
}
