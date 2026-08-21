using BankaApp.Application.Interfaces;
using BankaApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BankaApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITransferService, TransferService>();
        return services;
    }
}
