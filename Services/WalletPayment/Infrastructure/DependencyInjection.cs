using Application.Common.Contracts;
using BuildingBlocks.Contracts.Services;
using BuildingBlocks.Messaging.Extensions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.ExternalServices;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using WalletPayment.Application.Common.Contracts;
using WalletPayment.Application.EventHandlers;
using WalletPayment.Application.Payment.Contracts;
using WalletPayment.Infrastructure.BackgroundServices;
using WalletPayment.Infrastructure.Data.Context;
using WalletPayment.Infrastructure.ExternalServices.PaymentGateway;
using WalletPayment.Infrastructure.Services;

namespace WalletPayment.Infrastructure;
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DependencyInjection))]
    public static IServiceCollection AddWalletPaymentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // دیتابیس
        services.AddDbContext<WalletDbContext>(options =>
         options.UseSqlServer(
             configuration.GetConnectionString("WalletConnectionString"),
             b => b.MigrationsAssembly(typeof(WalletDbContext).Assembly.FullName)));

        services.AddScoped<IWalletDbContext>(provider => provider.GetService<WalletDbContext>());

        // ریپوزیتوری‌ها
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<ITransferFeeCalculator, TransferFeeCalculator>();
        services.AddSingleton<ICurrencyExchangeService, CurrencyExchangeService>();
        services.AddScoped<IAccountSnapshotService, AccountSnapshotService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
        services.AddScoped<IRefundService, RefundService>();       

        // اضافه کردن سرویس‌های پرداخت
        AddPaymentServices(services, configuration);

        // ثبت سرویس‌های Background
      //  services.AddHostedService<CreditDueDateCheckingService>();
       // services.AddHostedService<AccountSnapshotBackgroundService>();

        // سرویس‌های خارجی
        services.AddHttpClient<IUserManagementService, UserManagementServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ServiceUrls:UserManagement"] ?? "https://localhost:7072");
        });

        // سیستم پیام‌رسانی
        
         services.AddMessaging(configuration, typeof(UserActivatedEventHandler).Assembly);
        Console.WriteLine("[DEBUG] AddMessaging loaded from: " + typeof(UserActivatedEventHandler).Assembly.FullName);
        


        return services;
    }

    private static void AddPaymentServices(IServiceCollection services, IConfiguration configuration)
    {
        // ثبت تنظیمات درگاه‌های پرداخت
        services.Configure<PaymentGatewaySettings>(configuration.GetSection("Payment:Gateways"));

        // ثبت سرویس‌های پرداخت
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IIntegratedPurchaseService, IntegratedPurchaseService>();

        // ثبت HTTP Client برای درگاه‌های پرداخت
        services.AddHttpClient("ZarinPal", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("Zibal", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // تنظیمات بیشتر درگاه‌ها اضافه شود
    }
}