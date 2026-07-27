using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Infrastructure.Identity;
using AYIGroupMarket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AYIGroupMarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<IApplicationDbContext>(provider =>
            provider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/account/access-denied";
        });

        services.AddScoped<IOrderNumberGenerator, Services.OrderNumberGenerator>();
        services.AddScoped<Payments.MobileMoneyPaymentGateway>();
        services.AddScoped<Payments.OrangeMoneyPaymentGateway>();
        services.AddScoped<Payments.CardPaymentGateway>();
        services.AddScoped<Payments.PayPalPaymentGateway>();
        services.AddScoped<Payments.WhatsAppManualPaymentGateway>();
        services.AddScoped<IPaymentGatewayResolver, Payments.PaymentGatewayResolver>();

        return services;
    }
}