using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BankaApp.IntegrationTests;

/// <summary>
/// Boots the real API pipeline in-memory for HTTP-level tests.
/// Each factory instance gets an isolated InMemory database.
/// </summary>
public class BankaAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntegrationDb_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // UseSetting wins over appsettings — keeps JWT sign/validate keys aligned.
        builder.UseSetting("Database:Provider", "InMemory");
        builder.UseSetting("Database:InMemoryName", _dbName);
        builder.UseSetting("Jwt:Issuer", "BankaApp");
        builder.UseSetting("Jwt:Audience", "BankaAppClients");
        builder.UseSetting("Jwt:SecretKey", "DEV_ONLY_CHANGE_ME_TO_A_LONG_RANDOM_SECRET_KEY_32+");
        builder.UseSetting("Jwt:ExpirationMinutes", "60");
    }
}
