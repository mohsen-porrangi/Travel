using UserManagement.API.Infrastructure.Data;
using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Contracts.Security;
using UserManagement.API.Endpoints.AccessControl.Services;
using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Contracts;
using UserManagement.API.Contracts;
using UserManagement.API.Services;

namespace UserManagement.API.Infrastructure.Configuration
{
    public static class ConfigureServices
    {
        private static Assembly assembly = typeof(Program).Assembly;
        public static IServiceCollection ConfigureSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            //  Configuration binding improvements
            var connectionString = configuration.GetConnectionString("UserConnectionString")
                ?? throw new InvalidOperationException("UserConnectionString not found in configuration");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString)
                       .EnableSensitiveDataLogging(false) // Production safety
                       .EnableDetailedErrors(false));

            return services;
        }

        public static IServiceCollection ConfigureMediatR(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(assembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
                config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            });

            return services;
        }
        public static IServiceCollection ConfigureService(this IServiceCollection services)
        {
            //  Keyed services for better organization
            services.AddKeyedScoped<IUserRepository, UserRepository>("primary");
            services.AddKeyedScoped<IUnitOfWork, UnitOfWork>("primary");

            // Core Services
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IPermissionService, UserPermissionService>();
            services.AddScoped<IUserManagementService, Services.UserManagementService>();

            //  Modern service configuration
            services.AddSingleton<ITemporaryRegistrationService, TemporaryRegistrationService>();
            services.AddScoped<IPermissionManagementService, PermissionManagementService>();

            //  Memory cache with options
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1000; // Limit cache size
                options.CompactionPercentage = 0.2; // Compact when 80% full
            });

            //  Messaging with keyed services
            services.AddKeyedSingleton<BuildingBlocks.Messaging.Contracts.IMessageBus, BuildingBlocks.Messaging.InMemory.InMemoryMessageBus>("memory");
            services.AddKeyedSingleton<BuildingBlocks.Messaging.Contracts.IMessageBusSubscriptionsManager, BuildingBlocks.Messaging.InMemory.InMemoryMessageBusSubscriptionsManager>("memory");

            // ✅ TODO: Wallet Service باید implement شود
            // services.AddScoped<IWalletPaymentService, WalletPaymentService>();

            //  Mock Implementation تا Wallet Service اصلی پیاده‌سازی شود
           // services.AddScoped<IWalletPaymentService, ModernMockWalletPaymentService>();

            return services;
        }
    }
}