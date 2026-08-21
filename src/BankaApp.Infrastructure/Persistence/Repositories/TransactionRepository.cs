using BankaApp.Application.Interfaces;
using BankaApp.Domain.Entities;
using BankaApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankaApp.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _db;

    public TransactionRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        => _db.Transactions.AddAsync(transaction, cancellationToken).AsTask();

    public Task<Transaction?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
        => _db.Transactions.FirstOrDefaultAsync(
            x => x.IdempotencyKey == idempotencyKey,
            cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByWalletIdAsync(
        Guid walletId,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);

        return await _db.Transactions
            .AsNoTracking()
            .Where(x => x.FromWalletId == walletId || x.ToWalletId == walletId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
