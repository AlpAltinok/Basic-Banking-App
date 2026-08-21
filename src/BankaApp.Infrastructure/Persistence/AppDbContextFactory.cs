using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankaApp.Infrastructure.Persistence;

/// <summary>
/// `dotnet ef migrations add` komutu tasarım zamanında DbContext oluştururken bunu kullanır.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=BankaApp.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}
