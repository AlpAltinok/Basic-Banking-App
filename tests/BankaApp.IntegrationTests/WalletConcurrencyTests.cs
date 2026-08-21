using BankaApp.Domain.Entities;
using BankaApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BankaApp.IntegrationTests;

/// <summary>
/// Proves optimistic concurrency: the second writer loses when Version mismatches.
/// </summary>
public class WalletConcurrencyTests
{
    [Fact]
    public async Task Second_Concurrent_Withdrawal_Should_Throw_DbUpdateConcurrencyException()
    {
        var dbName = Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source=file:Concurrency_{dbName}?mode=memory&cache=shared")
            .Options;

        // Shared in-memory SQLite requires an open keep-alive connection.
        await using var keepAlive = new AppDbContext(options);
        await keepAlive.Database.OpenConnectionAsync();
        await keepAlive.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        keepAlive.Users.Add(new User
        {
            Id = userId,
            Email = "concurrent@example.com",
            FullName = "Concurrent",
            PasswordHash = "hash"
        });
        keepAlive.Wallets.Add(new Wallet
        {
            Id = walletId,
            UserId = userId,
            Balance = 100m,
            Version = 1
        });
        await keepAlive.SaveChangesAsync();

        await using var db1 = new AppDbContext(options);
        await using var db2 = new AppDbContext(options);

        var wallet1 = await db1.Wallets.SingleAsync(x => x.Id == walletId);
        var wallet2 = await db2.Wallets.SingleAsync(x => x.Id == walletId);

        // First request wins
        wallet1.Balance -= 60m;
        wallet1.Version++;
        await db1.SaveChangesAsync();

        // Second request still holds stale Version=1 → concurrency failure
        wallet2.Balance -= 60m;
        wallet2.Version++;
        var act = async () => await db2.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();

        await using var verify = new AppDbContext(options);
        var final = await verify.Wallets.SingleAsync(x => x.Id == walletId);
        final.Balance.Should().Be(40m); // only the first withdrawal applied
        final.Version.Should().Be(2);
    }
}
