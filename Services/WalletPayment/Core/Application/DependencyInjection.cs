using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Application;
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddWalletPaymentApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // ثبت خودکار Event Handler ها
        services.RegisterEventHandlers(Assembly.GetExecutingAssembly());

        return services;
    }
}