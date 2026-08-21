using BankaApp.Application.Interfaces;
using BankaApp.Domain.Entities;
using BankaApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankaApp.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _db;

    public WalletRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
        => _db.Wallets.AddAsync(wallet, cancellationToken).AsTask();

    public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
}
