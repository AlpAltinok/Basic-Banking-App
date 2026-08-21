using BankaApp.Application.Interfaces;
using BankaApp.Application.Options;
using BankaApp.Infrastructure.Persistence;
using BankaApp.Infrastructure.Persistence.Repositories;
using BankaApp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankaApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // Provider: Sqlite (varsayılan) | SqlServer | InMemory
        var provider = configuration.GetValue("Database:Provider", "Sqlite");
        var migrationsAssembly = typeof(AppDbContext).Assembly.GetName().Name;

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (provider)
            {
                case "SqlServer":
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                    break;

                case "InMemory":
                    options.UseInMemoryDatabase(
                        configuration.GetValue<string>("Database:InMemoryName") ?? "BankaAppDb");
                    break;

                default:
                    options.UseSqlite(
                        configuration.GetConnectionString("SqliteConnection"),
                        sqlite => sqlite.MigrationsAssembly(migrationsAssembly));
                    break;
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
