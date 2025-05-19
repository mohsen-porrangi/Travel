using Carter;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WalletPayment.API.Middleware;
using WalletPayment.Infrastructure;
using WalletPayment.Application;

namespace WalletPayment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembly = typeof(Program).Assembly;

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<WalletPayment.Infrastructure.Data.Context.WalletDbContext>("Database");

            // Infrastructure
            builder.Services.AddWalletPaymentInfrastructure(builder.Configuration);

            // Application
            builder.Services.AddWalletPaymentApplication();

            // API
            builder.Services.AddValidatorsFromAssembly(assembly);
            builder.Services.AddCarter();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddExceptionHandler<ErrorHandlerMiddleware>();
            builder.Services.AddProblemDetails();

            // Authentication & Authorization
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            if (app.Environment.IsDevelopment())
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


            app.Run();
        }
    }
}
