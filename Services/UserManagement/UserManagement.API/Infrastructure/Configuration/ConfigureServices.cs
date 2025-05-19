using UserManagement.API.Infrastructure.Data;
using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Contracts.Security;
using UserManagement.API.Endpoints.AccessControl.Services;
using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Contracts;

namespace UserManagement.API.Infrastructure.Configuration
{
    public static class ConfigureServices
    {
        private static Assembly assembly = typeof(Program).Assembly;
        public static void ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("UserConnectionString")));
        }

        public static void ConfigureMediatR(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
                config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            });
        }
        public static void ConfigureService(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IPermissionService, UserPermissionService>();
            services.AddScoped<IUserManagementService, Services.UserManagementService>();
            services.AddSingleton<BuildingBlocks.Messaging.Contracts.IMessageBus, BuildingBlocks.Messaging.InMemory.InMemoryMessageBus>();
            services.AddSingleton<BuildingBlocks.Messaging.Contracts.IMessageBusSubscriptionsManager, BuildingBlocks.Messaging.InMemory.InMemoryMessageBusSubscriptionsManager>();


          
        }
    }
}